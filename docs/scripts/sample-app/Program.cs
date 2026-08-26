// Sample ASP.NET Core app used to generate the OpenAPI specification for documentation.
// The document is written at BUILD time by Microsoft.Extensions.ApiDescription.Server --
// see OpenApiDocumentsDirectory in sample-app.csproj. The app never needs to be run.
using Microsoft.OpenApi;
using MJCZone.DapperMatic.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Configure to listen on port 5000
builder.WebHost.UseUrls("http://localhost:5000");

// Add services to the container
builder.Services.AddDapperMatic();

// Built-in ASP.NET Core OpenAPI document generation (replaces Swashbuckle)
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer(
        (document, context, cancellationToken) =>
        {
            document.Info = new OpenApiInfo
            {
                Title = "DapperMatic REST API",
                Version = "v1",
                Description = "Database schema management REST API using DapperMatic",
                Contact = new OpenApiContact
                {
                    Name = "MJCZone Inc.",
                    Url = new Uri("https://github.com/mjczone/dappermatic"),
                },
                License = new OpenApiLicense
                {
                    Name = "LGPL v3",
                    Url = new Uri("https://www.gnu.org/licenses/lgpl-3.0.html"),
                },
            };
            return Task.CompletedTask;
        }
    );
});

var app = builder.Build();

// Serve the document at runtime too, for anyone who wants to browse it locally
app.MapOpenApi();

app.UseRouting();

// Map DapperMatic endpoints
app.UseDapperMatic();

app.Run();
