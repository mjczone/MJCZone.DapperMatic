using System.Data.SQLite;
using System.Net;
using System.Net.Http.Headers;
using MJCZone.DapperMatic.Models;
using MJCZone.DapperMatic.WebApi.Handlers;
using MJCZone.DapperMatic.WebApi.HandlerTypes;
using MJCZone.DapperMatic.WebApi.Models;
using MJCZone.DapperMatic.WebApi.TestServer;
using Xunit.Abstractions;

namespace MJCZone.DapperMatic.WebApi.Tests.Apis;

public class DdlApiTests : IClassFixture<WebApiTestFactory>
{
    private readonly HttpClient _client;
    private readonly ITestOutputHelper _output;

    public DdlApiTests(WebApiTestFactory factory, ITestOutputHelper output)
    {
        _client = factory.CreateClient();
        _output = output;

        Directory.CreateDirectory(PathUtils.NormalizePath("../data")!);
        Dapper.SqlMapper.AddTypeHandler(typeof(Guid), new GuidHandler());
    }

    // EnsureConnectionStringIsSet
    private const string _connectionStringName = "TestDdlConnectionString";

    private async Task EnsureConnectionStringAsync()
    {
        var body = new ConnectionStringsEntryRequest
        {
            Name = _connectionStringName,
            ConnectionString = new SQLiteConnectionStringBuilder
            {
                DataSource = "TestDdd.db"
            }.ConnectionString,
            Vault = "LocalFile"
        };

        // Only admins can set connection strings
        var request = WebApiTestUtils.CreateAdminRequest(
            HttpMethod.Put,
            "/api/db/cs/entries",
            body
        );
        var response = await _client.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            _output.WriteLine(content);
        }

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task EnsureDatabaseAsync()
    {
        await EnsureConnectionStringAsync();

        var body = new DatabaseEntry
        {
            Name = "TestDdlDatabase",
            Slug = "test-ddl-database",
            ConnectionStringName = _connectionStringName,
            ConnectionStringVaultName = "LocalFile",
            ManagementRoles = ["Admin"],
            ExecutionRoles = [],
            IsActive = true,
            ProviderType = DbProviderType.Sqlite,
        };

        // does it exist
        var getRequest = WebApiTestUtils.CreateAdminRequest(
            HttpMethod.Get,
            "/api/db/databases/test-ddl-database"
        );
        var getResponse = await _client.SendAsync(getRequest);
        if (getResponse.IsSuccessStatusCode)
        {
            var content = await getResponse.Content.ReadAsStringAsync();
            var apiResponse = System.Text.Json.JsonSerializer.Deserialize<DatabaseResponse>(
                content,
                DapperMaticOptions.JsonSerializerOptions
            );
            if (apiResponse?.Results?.Id != null)
            {
                return;
            }
        }

        // create it if it doesn't
        var addRequest = WebApiTestUtils.CreateAdminRequest(
            HttpMethod.Post,
            "/api/db/databases",
            body
        );
        var response = await _client.SendAsync(addRequest);
    }

    // Add a test for the DDL API
    [Fact]
    public async Task DdlApi_CanGetSchemas()
    {
        await EnsureDatabaseAsync();

        // GET /api/db/databases/{databaseSlug}/schemas
        var request = WebApiTestUtils.CreateAdminRequest(
            HttpMethod.Get,
            "/api/db/databases/test-ddl-database/schemas"
        );
        var response = await _client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _output.WriteLine(content);
        }

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // extract the content as a string
        var apiResponse = System.Text.Json.JsonSerializer.Deserialize<StringListResponse>(
            content,
            DapperMaticOptions.JsonSerializerOptions
        );

        Assert.NotNull(apiResponse);
        Assert.NotNull(apiResponse.Results);
    }
}
