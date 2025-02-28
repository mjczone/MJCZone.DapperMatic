using System.Net;
using MJCZone.DapperMatic.WebApi.HandlerTypes;
using MJCZone.DapperMatic.WebApi.Options;
using Xunit.Abstractions;

namespace MJCZone.DapperMatic.WebApi.Tests.Apis;

public class DdlTableApiTests : DdlApiTestsBase
{
    public DdlTableApiTests(WebApiTestFactory factory, ITestOutputHelper output)
        : base(factory, output) { }

    public Func<string?, string> ApiUrl = (s) =>
        $"/api/db/databases/test-ddl-database/schemas/_/tables{(s == null ? "" : $"/{s}")}";

    [Fact]
    public async Task DdlApi_CanCrudTables()
    {
        await EnsureDatabaseAsync();

        // GET tables
        await GetTablesTestAsync(0);

        // CREATE tables
        await CreateTableTestAsync("TestTable1");
        await CreateTableTestAsync("TestTable2");
        await GetTablesTestAsync(2);

        // DELETE tables
        await DeleteTableTestAsync("TestTable1");
        await GetTablesTestAsync(1);
        await DeleteTableTestAsync("TestTable2");
        await GetTablesTestAsync(0);
    }

    private async Task DeleteTableTestAsync(string tableName)
    {
        var deleteTableRequest = WebApiTestUtils.CreateAdminRequest(
            HttpMethod.Delete,
            ApiUrl(tableName)
        );
        var deleteTableResponse = await _client.SendAsync(deleteTableRequest);
        var deleteTableContent = await deleteTableResponse.Content.ReadAsStringAsync();

        if (!deleteTableResponse.IsSuccessStatusCode)
        {
            _output.WriteLine(deleteTableContent);
        }

        Assert.Equal(HttpStatusCode.OK, deleteTableResponse.StatusCode);

        // get BoolResponse from content
        var boolResponse = System.Text.Json.JsonSerializer.Deserialize<BoolResponse>(
            deleteTableContent,
            DapperMaticOptions.JsonSerializerOptions
        );

        Assert.NotNull(boolResponse);
        Assert.True(boolResponse.Results);
    }

    private async Task CreateTableTestAsync(
        string tableName,
        Action<CreateTableRequest>? requestAction = null
    )
    {
        var createTableRequestDto = new CreateTableRequest { TableName = tableName };
        var createTableRequest = WebApiTestUtils.CreateAdminRequest(
            HttpMethod.Post,
            ApiUrl(null),
            createTableRequestDto
        );
        requestAction?.Invoke(createTableRequestDto);

        var createTableResponse = await _client.SendAsync(createTableRequest);
        var createTableContent = await createTableResponse.Content.ReadAsStringAsync();

        if (!createTableResponse.IsSuccessStatusCode)
        {
            _output.WriteLine(createTableContent);
        }

        // if running this out of sequence, sometimes the table already exists
        // if (createTableResponse.StatusCode == HttpStatusCode.Conflict)
        //     return;

        Assert.Equal(HttpStatusCode.Created, createTableResponse.StatusCode);

        var tableResponse = System.Text.Json.JsonSerializer.Deserialize<TableResponse>(
            createTableContent,
            DapperMaticOptions.JsonSerializerOptions
        );

        Assert.NotNull(tableResponse?.Results);
        Assert.Equal(tableName, tableResponse.Results.TableName);

        Assert.NotNull(tableResponse?.Results?.Columns);
        Assert.Single(tableResponse.Results.Columns);
        Assert.Equal("id", tableResponse.Results.Columns[0].ColumnName);
    }

    private async Task GetTablesTestAsync(int expectedCount)
    {
        var getTablesRequest = WebApiTestUtils.CreateAdminRequest(HttpMethod.Get, ApiUrl(null));
        var getTablesResponse = await _client.SendAsync(getTablesRequest);
        var getTablesContent = await getTablesResponse.Content.ReadAsStringAsync();

        if (!getTablesResponse.IsSuccessStatusCode)
        {
            _output.WriteLine(getTablesContent);
        }

        Assert.Equal(HttpStatusCode.OK, getTablesResponse.StatusCode);

        // extract the content as a string
        var getTablesApiResponse = System.Text.Json.JsonSerializer.Deserialize<TableListResponse>(
            getTablesContent,
            DapperMaticOptions.JsonSerializerOptions
        );

        Assert.NotNull(getTablesApiResponse);
        Assert.NotNull(getTablesApiResponse.Results);

        Assert.Equal(expectedCount, getTablesApiResponse.Results.Count);
    }
}
