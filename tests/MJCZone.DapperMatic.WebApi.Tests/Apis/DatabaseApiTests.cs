using System.Net;
using System.Net.Http.Headers;
using MJCZone.DapperMatic.WebApi.Handlers;
using MJCZone.DapperMatic.WebApi.Models;
using MJCZone.DapperMatic.WebApi.TestServer;
using Xunit.Abstractions;

namespace MJCZone.DapperMatic.WebApi.Tests.Apis;

public class DatabaseApiTests : IClassFixture<WebApiTestFactory>
{
    private readonly HttpClient _client;
    private readonly string _token;
    private readonly ITestOutputHelper _output;

    public DatabaseApiTests(WebApiTestFactory factory, ITestOutputHelper output)
    {
        _client = factory.CreateClient();
        _token = JwtTokenGenerator.GenerateTestToken();
        _output = output;

        Directory.CreateDirectory(@"..\data");
        Dapper.SqlMapper.AddTypeHandler(typeof(Guid), new GuidHandler());
    }

    [Fact]
    public async Task DatabaseApi_CanGetDatabases()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/dappermatic/databases");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token); // Add token to request

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
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/dappermatic/databases");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token); // Add token to request

        var nameExt = Guid.NewGuid().ToString("N");
        var databaseEntry = new DatabaseEntry
        {
            Name = "TestDatabase-" + nameExt,
            ConnectionStringVaultName = "LocalFile",
            ConnectionStringName =
                "Data Source=../data/dappermatic-databases.db;Version=3;BinaryGUID=False;",
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
    }
}
