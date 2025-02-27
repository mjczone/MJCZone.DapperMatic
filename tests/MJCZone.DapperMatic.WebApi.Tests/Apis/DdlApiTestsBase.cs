using System.Data.SQLite;
using System.Net;
using MJCZone.DapperMatic.WebApi.HandlerTypes;
using MJCZone.DapperMatic.WebApi.Models;
using MJCZone.DapperMatic.WebApi.Options;
using Xunit.Abstractions;

namespace MJCZone.DapperMatic.WebApi.Tests.Apis;

public class DdlApiTestsBase : IClassFixture<WebApiTestFactory>
{
    protected readonly HttpClient _client;
    protected readonly ITestOutputHelper _output;

    protected DdlApiTestsBase(WebApiTestFactory factory, ITestOutputHelper output)
    {
        _client = factory.CreateClient();
        _output = output;

        Directory.CreateDirectory(PathUtils.NormalizePath("../data")!);
        Dapper.SqlMapper.AddTypeHandler(typeof(Guid), new GuidHandler());
    }

    // EnsureConnectionStringIsSet
    protected const string _connectionStringName = "TestDdlConnectionString";

    protected async Task EnsureConnectionStringAsync()
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

    protected async Task EnsureDatabaseAsync()
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
}
