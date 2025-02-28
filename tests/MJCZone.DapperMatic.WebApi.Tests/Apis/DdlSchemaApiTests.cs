using System.Net;
using MJCZone.DapperMatic.WebApi.HandlerTypes;
using MJCZone.DapperMatic.WebApi.Options;
using Xunit.Abstractions;

namespace MJCZone.DapperMatic.WebApi.Tests.Apis;

public class DdlSchemaApiTests : DdlApiTestsBase
{
    public DdlSchemaApiTests(WebApiTestFactory factory, ITestOutputHelper output)
        : base(factory, output) { }

    [Fact]
    public async Task DdlApi_CanCrudSchemas()
    {
        await EnsureDatabaseAsync();

        // GET schemas
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

        // CREATE schema
        var schemaName = "TestSchema";
        var createRequest = WebApiTestUtils.CreateAdminRequest(
            HttpMethod.Post,
            $"/api/db/databases/test-ddl-database/schemas",
            new CreateSchemaRequest { SchemaName = schemaName }
        );
        var createResponse = await _client.SendAsync(createRequest);
        var createContent = await createResponse.Content.ReadAsStringAsync();

        if (!createResponse.IsSuccessStatusCode)
        {
            _output.WriteLine(createContent);
        }

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var boolResponse = System.Text.Json.JsonSerializer.Deserialize<BoolResponse>(
            createContent,
            DapperMaticOptions.JsonSerializerOptions
        );

        // if SQLite, the bool will be false, since SQLite doesn't support schemas
        Assert.False(boolResponse?.Results);

        // DELETE schema
        var deleteRequest = WebApiTestUtils.CreateAdminRequest(
            HttpMethod.Delete,
            $"/api/db/databases/test-ddl-database/schemas/{schemaName}"
        );
        var deleteResponse = await _client.SendAsync(deleteRequest);
        var deleteContent = await deleteResponse.Content.ReadAsStringAsync();

        if (!deleteResponse.IsSuccessStatusCode)
        {
            _output.WriteLine(deleteContent);
        }

        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        var deleteBoolResponse = System.Text.Json.JsonSerializer.Deserialize<BoolResponse>(
            deleteContent,
            DapperMaticOptions.JsonSerializerOptions
        );

        // if SQLite, the bool will be false, since SQLite doesn't support schemas
        Assert.False(deleteBoolResponse?.Results);
    }
}
