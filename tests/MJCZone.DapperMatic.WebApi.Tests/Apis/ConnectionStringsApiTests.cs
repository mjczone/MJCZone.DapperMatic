using System.Net;
using MJCZone.DapperMatic.WebApi.HandlerTypes;
using MJCZone.DapperMatic.WebApi.Options;
using MJCZone.DapperMatic.WebApi.Vaults;
using Xunit.Abstractions;

namespace MJCZone.DapperMatic.WebApi.Tests.Apis;

public class ConnectionStringsApiTests : IClassFixture<WebApiTestFactory>
{
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public ConnectionStringsApiTests(WebApiTestFactory factory, ITestOutputHelper output)
    {
        _client = factory.CreateClient();
        _output = output;

        Directory.CreateDirectory(PathUtils.NormalizePath("../data")!);
        Dapper.SqlMapper.AddTypeHandler(typeof(Guid), new GuidHandler());
    }

    [Fact]
    public async Task ConnectionStringsApi_CanGetConnectionStringVaultFactoryNames()
    {
        var request = WebApiTestUtils.CreateUserRequest(
            HttpMethod.Get,
            "/api/db/cs/vault-factories"
        );
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
        var request = WebApiTestUtils.CreateUserRequest(HttpMethod.Get, "/api/db/cs/vaults");
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

        var request = WebApiTestUtils.CreateUserRequest(HttpMethod.Put, "/api/db/cs/entries", body);
        var response = await _client.SendAsync(request);

        // the response should be a 403 Forbidden
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        // now change the token to an admin token
        request = WebApiTestUtils.CreateAdminRequest(HttpMethod.Put, "/api/db/cs/entries", body);

        // get the response as ApiResponse<List<ConnectionStringEntry>>
        response = await _client.SendAsync(request);

        // extract the content as a string
        var content = await response.Content.ReadAsStringAsync();
        _output.WriteLine("Setting connection string: " + content);

        // the response should be a 200 OK
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

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

        // get the response as ApiResponse<List<ConnectionStringEntry>>
        var request = WebApiTestUtils.CreateUserRequest(
            HttpMethod.Delete,
            "/api/db/cs/entries?name=TestConnectionString&vault=LocalFile"
        );
        var response = await _client.SendAsync(request);

        // extract the content as a string
        var content = await response.Content.ReadAsStringAsync();
        _output.WriteLine("Delete connection string content (1): " + content);

        // the response should be a 403 Forbidden
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);

        // now change the token to an admin token
        request = WebApiTestUtils.CreateAdminRequest(
            HttpMethod.Delete,
            "/api/db/cs/entries?name=TestConnectionString&vault=LocalFile"
        );

        // get the response as ApiResponse<List<ConnectionStringEntry>>
        response = await _client.SendAsync(request);

        // extract the content as a string
        content = await response.Content.ReadAsStringAsync();
        _output.WriteLine("Delete connection string content (2): " + content);

        // the response should be a 200 OK
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

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
