using System.Net;
using System.Net.Http.Headers;
using MJCZone.DapperMatic.WebApi.HandlerTypes;
using MJCZone.DapperMatic.WebApi.Models;
using MJCZone.DapperMatic.WebApi.Options;
using MJCZone.DapperMatic.WebApi.TestServer;
using Xunit.Abstractions;

namespace MJCZone.DapperMatic.WebApi.Tests.Apis;

public class DatabaseApiTests : IClassFixture<WebApiTestFactory>
{
    private readonly HttpClient _client;
    private readonly string _userToken;
    private readonly string _adminToken;
    private readonly ITestOutputHelper _output;

    public DatabaseApiTests(WebApiTestFactory factory, ITestOutputHelper output)
    {
        _client = factory.CreateClient();
        _userToken = JwtTokenGenerator.GenerateTestToken();
        _adminToken = JwtTokenGenerator.GenerateTestToken("Admin");
        _output = output;

        Directory.CreateDirectory(PathUtils.NormalizePath("../data")!);
        Dapper.SqlMapper.AddTypeHandler(typeof(Guid), new GuidHandler());
    }

    [Fact]
    public async Task DatabaseApi_CanGetDatabases()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/db/databases");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _userToken); // Add token to request

        // get the response as ApiResponse<List<DatabaseEntry>>
        var response = await _client.SendAsync(request);

        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _output.WriteLine(content);
        }

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // extract the content as a string
        var apiResponse = System.Text.Json.JsonSerializer.Deserialize<DatabasesResponse>(
            content,
            DapperMaticOptions.JsonSerializerOptions
        );

        Assert.NotNull(apiResponse);
        Assert.NotNull(apiResponse.Results);
    }

    [Fact]
    public async Task DatabaseApi_CanAddDatabase()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/db/databases");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _userToken); // Add token to request

        var nameExt = Guid.NewGuid().ToString("N");
        var databaseEntry = new DatabaseEntry
        {
            Name = "TestDatabase-" + nameExt,
            ConnectionStringVaultName = "LocalFile",
            ConnectionStringName = "TestDatabase-" + nameExt,
            ProviderType = DbProviderType.Sqlite,
            Description = "Test Database",
            IsActive = true,
            Slug = "test-database-" + nameExt,
        };

        request.Content = new StringContent(
            System.Text.Json.JsonSerializer.Serialize(databaseEntry),
            System.Text.Encoding.UTF8,
            "application/json"
        );

        var response = await _client.SendAsync(request);

        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _output.WriteLine(content);
        }

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var created = System.Text.Json.JsonSerializer.Deserialize<DatabaseResponse>(
            content,
            DapperMaticOptions.JsonSerializerOptions
        );

        Assert.NotNull(created?.Results?.Id);
        Assert.NotEqual(Guid.Empty, created.Results.Id);

        // now get the database
        var getRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/db/databases/{created.Results.Id}"
        );
        getRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _userToken); // Add token to request

        var getResponse = await _client.SendAsync(getRequest);

        content = await getResponse.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _output.WriteLine(content);
        }

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var db = System.Text.Json.JsonSerializer.Deserialize<DatabaseResponse>(
            content,
            DapperMaticOptions.JsonSerializerOptions
        );

        Assert.NotNull(db?.Results?.Id);
        Assert.NotEqual(Guid.Empty, db.Results.Id);

        // now delete the database
        var deleteRequest = new HttpRequestMessage(
            HttpMethod.Delete,
            $"/api/db/databases/{created.Results.Id}"
        );
        deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _userToken); // Add token to request

        var deleteResponse = await _client.SendAsync(deleteRequest);

        if (!deleteResponse.IsSuccessStatusCode)
        {
            _output.WriteLine(await deleteResponse.Content.ReadAsStringAsync());
        }

        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);
    }
}
