using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace MJCZone.DapperMatic.WebApi.TestServer;

public static class JwtTokenGenerator
{
    public static string GenerateTestToken(string roles = "")
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var key = Encoding.UTF8.GetBytes(Program.TestKey);

        var claims = new List<Claim> { new Claim(ClaimTypes.Name, "TestUser") };
        if (!string.IsNullOrEmpty(roles))
        {
            var roleClaims = roles
                .Split(
                    [',', ';', ' '],
                    StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
                )
                .Select(role => new Claim(ClaimTypes.Role, role));
            claims.AddRange(roleClaims);
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
                new[]
                {
                    new Claim(ClaimTypes.Name, "TestUser"),
                    new Claim(ClaimTypes.Role, "Admin") // Add roles if needed
                }
            ),

            Expires = DateTime.UtcNow.AddHours(1),

            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }
}
