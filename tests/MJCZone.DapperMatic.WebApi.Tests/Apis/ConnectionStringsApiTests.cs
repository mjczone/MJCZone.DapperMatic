using System.Net;
using System.Net.Http.Headers;
using MJCZone.DapperMatic.WebApi.Handlers;
using MJCZone.DapperMatic.WebApi.HandlerTypes;
using MJCZone.DapperMatic.WebApi.TestServer;
using Xunit.Abstractions;

namespace MJCZone.DapperMatic.WebApi.Tests.Apis;

public class ConnectionStringsApiTests : IClassFixture<WebApiTestFactory>
{
    private readonly HttpClient _client;
    private readonly string _userToken;
    private readonly string _adminToken;
    private readonly ITestOutputHelper _output;

    public ConnectionStringsApiTests(WebApiTestFactory factory, ITestOutputHelper output)
    {
        _client = factory.CreateClient();
        _userToken = JwtTokenGenerator.GenerateTestToken();
        _adminToken = JwtTokenGenerator.GenerateTestToken("Admin");
        _output = output;

        Directory.CreateDirectory(@"..\data");
        Dapper.SqlMapper.AddTypeHandler(typeof(Guid), new GuidHandler());
    }

    [Fact]
    public async Task ConnectionStringsApi_CanGetConnectionStringVaultFactoryNames()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/dappermatic/cs/vault-factories");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _userToken); // Add token to request

        var response = await _client.SendAsync(request);

        // the response should be a 200 OK
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // extract the content as a string
        var content = await response.Content.ReadAsStringAsync();

        // deserialize it to a StringListResponse
        var apiResponse = System.Text.Json.JsonSerializer.Deserialize<StringListResponse>(
            content,
            DapperMaticOptions.JsonSerializerOptions
        );

        Assert.NotNull(apiResponse);
        Assert.NotNull(apiResponse.Results);
        Assert.NotEmpty(apiResponse.Results);

        // the file vaule
        Assert.Contains(ConnectionStringsFileVaultFactory.FactoryName, apiResponse.Results);
        // the database value
        Assert.Contains(ConnectionStringsDatabaseVaultFactory.FactoryName, apiResponse.Results);
    }

    [Fact]
    public async Task ConnectionStringsApi_CanGetConnectionStringVaults()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/dappermatic/cs/vaults");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _userToken); // Add token to request

        // get the response as ApiResponse<List<ConnectionStringEntry>>
        var response = await _client.SendAsync(request);

        // the response should be a 200 OK
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // extract the content as a string
        var content = await response.Content.ReadAsStringAsync();

        // deserialize it to a StringListResponse
        var apiResponse =
            System.Text.Json.JsonSerializer.Deserialize<ConnectionStringsVaultInfoResponse>(
                content,
                DapperMaticOptions.JsonSerializerOptions
            );

        Assert.NotNull(apiResponse);
        Assert.NotNull(apiResponse.Results);
        Assert.NotEmpty(apiResponse.Results);

        // the file vaule
        Assert.Contains("LocalFile", apiResponse.Results.Select(x => x.Name));
        Assert.Contains("LocalDatabase", apiResponse.Results.Select(x => x.Name));
        Assert.True(apiResponse.Results.First(x => x.Name == "LocalFile").IsDefault);
    }

    [Fact]
    public async Task ConnectionStringsApi_CanSetConnectionStrings()
    {
        var body = new ConnectionStringsEntryRequest
        {
            Name = "TestConnectionString",
            ConnectionString = "TestConnectionStringValue",
            Vault = "LocalFile",
        };

        Func<string, HttpRequestMessage> createRequest = (token) =>
        {
            var request = new HttpRequestMessage(HttpMethod.Put, "/api/dappermatic/cs/entries");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token); // Add token to request

            // attach the body to the request
            request.Content = new StringContent(
                System.Text.Json.JsonSerializer.Serialize(
                    body,
                    DapperMaticOptions.JsonSerializerOptions
                ),
                System.Text.Encoding.UTF8,
                "application/json"
            );

            return request;
        };

        // get the response as ApiResponse<List<ConnectionStringEntry>>
        var request = createRequest(_userToken);
        var response = await _client.SendAsync(request);

        // the response should be a 403 Forbidden
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        // now change the token to an admin token
        request = createRequest(_adminToken);

        // get the response as ApiResponse<List<ConnectionStringEntry>>
        response = await _client.SendAsync(request);

        // the response should be a 200 OK
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // extract the content as a string
        var content = await response.Content.ReadAsStringAsync();

        // deserialize it to a StringListResponse
        var apiResponse = System.Text.Json.JsonSerializer.Deserialize<EmptyResponse>(
            content,
            DapperMaticOptions.JsonSerializerOptions
        );

        Assert.NotNull(apiResponse);

        // read the file to which the connection string was written
        var fileContent = await File.ReadAllTextAsync(
            DapperMaticOptions.DefaultDapperMaticConnectionStringsVaultFileName
        );
        Assert.Contains("TestConnectionString", fileContent);
    }

    [Fact]
    public async Task ConnectionStringsApi_CanDeleteConnectionStrings()
    {
        // create the connection string
        await ConnectionStringsApi_CanSetConnectionStrings();

        var fileContent = await File.ReadAllTextAsync(
            DapperMaticOptions.DefaultDapperMaticConnectionStringsVaultFileName
        );
        Assert.Contains("TestConnectionString", fileContent);

        var body = new ConnectionStringsEntryRequest
        {
            Name = "TestConnectionString",
            Vault = "LocalFile",
        };

        Func<string, HttpRequestMessage> createRequest = (token) =>
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, "/api/dappermatic/cs/entries");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token); // Add token to request

            // attach two query strings to the request
            request.RequestUri = new Uri(
                $"{request.RequestUri}?name={body.Name}&vault={body.Vault}",
                UriKind.Relative
            );

            return request;
        };

        // get the response as ApiResponse<List<ConnectionStringEntry>>
        var request = createRequest(_userToken);
        var response = await _client.SendAsync(request);

        // the response should be a 403 Forbidden
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        // now change the token to an admin token
        request = createRequest(_adminToken);

        // get the response as ApiResponse<List<ConnectionStringEntry>>
        response = await _client.SendAsync(request);

        // the response should be a 200 OK
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // extract the content as a string
        var content = await response.Content.ReadAsStringAsync();

        // deserialize it to a StringListResponse
        var apiResponse = System.Text.Json.JsonSerializer.Deserialize<EmptyResponse>(
            content,
            DapperMaticOptions.JsonSerializerOptions
        );

        Assert.NotNull(apiResponse);

        // read the file to which the connection string was written
        fileContent = await File.ReadAllTextAsync(
            DapperMaticOptions.DefaultDapperMaticConnectionStringsVaultFileName
        );
        Assert.DoesNotContain("TestConnectionString", fileContent);
    }
}
