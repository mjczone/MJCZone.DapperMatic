using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MJCZone.DapperMatic.WebApi;

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

builder.Services.AddDapperMatic(options =>
{
    options.ApiPrefix = "/api/db";
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/secure-endpoint", () => "This is a secure endpoint").RequireAuthorization();
app.MapGet("/public-endpoint", () => "This is a public endpoint");

app.UseDapperMatic();

app.Run();

public partial class Program
{
    public const string TestKey = "SuperSecretKey12345SuperSecretKey12345";
}
