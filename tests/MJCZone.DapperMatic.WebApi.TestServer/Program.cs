using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using Microsoft.OpenApi.Writers;
using MJCZone.DapperMatic.WebApi;
using MJCZone.DapperMatic.WebApi.TestServer;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add JWT Authentication (Hardcoded Token)
builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestKey)),
            ValidAlgorithms = [SecurityAlgorithms.HmacSha256]
        };
    });
builder.Services.AddAuthorization();

// see: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/aspnetcore-openapi?view=aspnetcore-9.0&tabs=visual-studio
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(policy => policy.Expire(TimeSpan.FromMinutes(10)));
});
builder.Services.AddOpenApi(options =>
{
    options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0;
    options.ShouldInclude = (type) => true;
    // options.AddSchemaTransformer(
    //     (schema, context, cancellationToken) =>
    //     {
    //         if (schema.Properties is not null)
    //         {
    //             foreach (var property in schema.Properties)
    //             {
    //                 if (schema.Required?.Contains(property.Key) != true)
    //                 {
    //                     property.Value.Nullable = false;
    //                 }
    //             }
    //         }
    //         return Task.CompletedTask;
    //     }
    // );
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    // options.AddDocumentTransformer<DuplicateModelNameFixer>();
});

builder.Services.AddDapperMatic(options =>
{
    options.ApiPrefix = "/api/db";
});

var app = builder.Build();

app.UseOutputCache();
app.UseAuthentication();
app.UseAuthorization();
app.MapOpenApi()
// .CacheOutput()
;
app.MapScalarApiReference(options =>
{
    var userToken = JwtTokenGenerator.GenerateTestToken();
    var adminToken = JwtTokenGenerator.GenerateTestToken("Admin");

    options.Title = "DapperMatic API";
    options.AddHeaderContent(
        @"
        <script>
        var dt = 0;
        function displayTokens() {
            if (dt++ > 10) return;
            setTimeout(() => {
                var h1Div = document.querySelector('.section-content');
                if (!h1Div) displayTokens();
                var div = document.createElement('div');
                div.innerHTML = '<div style=""padding:10px 0;font-size:8pt;""><div><strong>User Token</strong>: "
            + userToken
            + @"</div><div><strong>Admin Token</strong>: "
            + adminToken
            + @"</div></div>';
                h1Div.appendChild(div);
            }, 250);
        }
        document.addEventListener('DOMContentLoaded', displayTokens, false);
        </script>".Trim()
    );
    options.Layout = ScalarLayout.Classic;
    options.Theme = ScalarTheme.DeepSpace;
    options.DefaultHttpClient = new(ScalarTarget.JavaScript, ScalarClient.Fetch);
    options.CustomCss = "";
    options.ShowSidebar = true;
    options.Authentication = new ScalarAuthenticationOptions { PreferredSecurityScheme = "Bearer" };
    options.WithDarkMode(false);
});

app.MapGet("/secure-endpoint", () => "This is a secure endpoint")
    .WithTags("Test Endpoints")
    .RequireAuthorization();
app.MapGet("/public-endpoint", () => "This is a public endpoint").WithTags("Test Endpoints");

app.UseDapperMatic();

app.Run();

public partial class Program
{
    public const string TestKey = "SuperSecretKey12345SuperSecretKey12345";
}

internal sealed class BearerSecuritySchemeTransformer(
    IAuthenticationSchemeProvider authenticationSchemeProvider
) : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        var authenticationSchemes = await authenticationSchemeProvider.GetAllSchemesAsync();
        if (authenticationSchemes.Any(authScheme => authScheme.Name == "Bearer"))
        {
            var requirements = new Dictionary<string, OpenApiSecurityScheme>
            {
                ["Bearer"] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    In = ParameterLocation.Header,
                    BearerFormat = "Json Web Token",
                    Description = "Enter 'Bearer' [space] and your token",
                },
            };
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes = requirements;
        }
    }
}

internal sealed class DuplicateModelNameFixer : IOpenApiDocumentTransformer
{
    public async Task TransformAsync(
        OpenApiDocument document,
        OpenApiDocumentTransformerContext context,
        CancellationToken cancellationToken
    )
    {
        await Task.Yield();

        var outputPath = Path.GetFullPath("openapi.spec.json");
        Console.WriteLine($"Writing document to {outputPath}");
        using FileStream? fileStream = new(outputPath, FileMode.Create, FileAccess.Write);
        var writerSettings = new OpenApiWriterSettings()
        {
            InlineLocalReferences = true,
            InlineExternalReferences = true
        };
        using var writer = new StreamWriter(fileStream);
        document.SerializeAsV3(new OpenApiJsonWriter(writer, writerSettings));

        var documentJson = document.SerializeAsJson(
            Microsoft.OpenApi.OpenApiSpecVersion.OpenApi3_0
        );
        var reader = new OpenApiStringReader(new OpenApiReaderSettings());
        var newDocument = reader.Read(documentJson, out var _);

        // Console.WriteLine($"Writing document to {"openapi.spec.json"}");
        // await File.WriteAllTextAsync(outputPath, documentJson, cancellationToken);

        var modelNamesToRemove = new Dictionary<string, string>();
        foreach (var schema in document.Components.Schemas)
        {
            var name = schema.Key;
            Console.WriteLine($"{name}: " + char.IsDigit(name[^1]));

            if (char.IsDigit(name[^1]))
            {
                var nameWithoutDigits = name;
                while (char.IsDigit(nameWithoutDigits[^1]))
                {
                    nameWithoutDigits = nameWithoutDigits[..^1];
                }
                modelNamesToRemove.TryAdd(name, nameWithoutDigits);
            }
        }

        // remove from schemas all the keys in modelNamesToRemove
        Console.WriteLine(modelNamesToRemove.Count + " components to remove");
        foreach (var name in modelNamesToRemove)
        {
            document.Components.Schemas.Remove(name.Key);
            Console.WriteLine("Removing component: " + name.Key);
        }

        // go through all the operations and endpoints and point the schema names without the number in the schema name
        // foreach (var path in document.Paths)
        // {
        //     foreach (var operation in path.Value.Operations)
        //     {
        //         foreach (var response in operation.Value.Responses)
        //         {
        //             if (response.Value.Content is not null)
        //             {
        //                 foreach (var mediaType in response.Value.Content)
        //                 {
        //                     if (mediaType.Value.Schema?.Reference?.Id is not null)
        //                     {
        //                         var schemaName = mediaType.Value.Schema.Reference.Id;
        //                         if (modelNamesToRemove.ContainsKey(schemaName))
        //                         {
        //                             mediaType.Value.Schema.Reference.Id = modelNamesToRemove[
        //                                 schemaName
        //                             ];
        //                         }
        //                     }
        //                 }
        //             }
        //         }
        //     }
        // }

        // return Task.CompletedTask;
    }
}
