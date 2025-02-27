using System.Net.Http.Headers;
using MJCZone.DapperMatic.WebApi.Options;
using MJCZone.DapperMatic.WebApi.TestServer;

namespace MJCZone.DapperMatic.WebApi.Tests;

public class WebApiTestUtils
{
    public static HttpRequestMessage CreateUserRequest(
        HttpMethod method,
        string path,
        object? body = null
    )
    {
        var token = JwtTokenGenerator.GenerateTestToken();
        return CreateRequest(token, method, path, body);
    }

    public static HttpRequestMessage CreateAdminRequest(
        HttpMethod method,
        string path,
        object? body = null
    )
    {
        var token = JwtTokenGenerator.GenerateTestToken("Admin");
        return CreateRequest(token, method, path, body);
    }

    public static HttpRequestMessage CreateRequest(
        string token,
        HttpMethod method,
        string path,
        object? body = null
    )
    {
        var request = new HttpRequestMessage(method, path);

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token); // Add token to request

        if (body != null)
        {
            request.Content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(
                    body,
                    DapperMaticOptions.JsonSerializerOptions
                ),
                System.Text.Encoding.UTF8,
                "application/json"
            );
        }

        return request;
    }
}
