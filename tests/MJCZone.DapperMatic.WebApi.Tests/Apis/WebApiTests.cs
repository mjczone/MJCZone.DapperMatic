using System.Net;
using System.Net.Http.Headers;
using MJCZone.DapperMatic.WebApi.TestServer;
using Xunit.Abstractions;

namespace MJCZone.DapperMatic.WebApi.Tests.Apis;

public class WebApiTests : IClassFixture<WebApiTestFactory>
{
    private readonly HttpClient _client;
    private readonly string _userToken;
    private readonly ITestOutputHelper _output;

    public WebApiTests(WebApiTestFactory factory, ITestOutputHelper output)
    {
        _client = factory.CreateClient();
        _userToken = JwtTokenGenerator.GenerateTestToken();
        _output = output;
    }

    [Fact]
    public async Task AuthenticatedUser_CanAccessSecureEndpoint()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/secure-endpoint");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _userToken); // Add token to request

        var response = await _client.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            _output.WriteLine(content);
        }

        Assert.Equal(HttpStatusCode.OK, response.StatusCode); // Should pass if authentication works
    }

    [Fact]
    public async Task UnauthenticatedUser_CannotAccessSecureEndpoint()
    {
        var response = await _client.GetAsync("/secure-endpoint");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode); // Should return 401
    }

    [Fact]
    public async Task PublicEndpointReturnsSuccessAndCorrectContentType()
    {
        // Arrange
        // Act
        var response = await _client.GetAsync("/public-endpoint");

        // Assert
        response.EnsureSuccessStatusCode(); // Status Code 200-299

        if (response.Content.Headers.ContentType != null)
        {
            Assert.Equal(
                "text/plain; charset=utf-8",
                response.Content.Headers.ContentType.ToString()
            );
        }
        else
        {
            Assert.Fail("ContentType is null");
        }
    }
}
