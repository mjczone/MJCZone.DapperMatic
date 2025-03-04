using System.Net;
using MJCZone.DapperMatic.WebApi.HandlerTypes;
using MJCZone.DapperMatic.WebApi.Options;
using Xunit.Abstractions;

namespace MJCZone.DapperMatic.WebApi.Tests.Apis;

public class DdlViewApiTests : DdlApiTestsBase
{
    public DdlViewApiTests(WebApiTestFactory factory, ITestOutputHelper output)
        : base(factory, output) { }

    public Func<string?, string> ApiUrl = (s) =>
        $"/api/db/databases/test-ddl-database/schemas/_/views{(s == null ? "" : $"/{s}")}";

    [Fact]
    public async Task DdlApi_CanCrudViews()
    {
        await EnsureDatabaseAsync();

        // GET views
        await GetViewsTestAsync(0);

        // CREATE views
        await CreateViewTestAsync("TestView1");
        await CreateViewTestAsync("TestView2");
        await GetViewsTestAsync(2);

        // DELETE views
        await DeleteViewTestAsync("TestView1");
        await GetViewsTestAsync(1);
        await DeleteViewTestAsync("TestView2");
        await GetViewsTestAsync(0);
    }

    private async Task CreateViewTestAsync(
        string viewName,
        Action<CreateViewRequest>? requestAction = null
    )
    {
        var createViewRequestDto = new CreateViewRequest
        {
            ViewName = viewName,
            Definition = "SELECT 1 AS TestColumn"
        };
        var createViewRequest = WebApiTestUtils.CreateAdminRequest(
            HttpMethod.Post,
            ApiUrl(null),
            createViewRequestDto
        );
        requestAction?.Invoke(createViewRequestDto);

        var createViewResponse = await _client.SendAsync(createViewRequest);
        var createViewContent = await createViewResponse.Content.ReadAsStringAsync();

        if (!createViewResponse.IsSuccessStatusCode)
        {
            _output.WriteLine(createViewContent);
        }

        Assert.Equal(HttpStatusCode.Created, createViewResponse.StatusCode);

        var viewResponse = System.Text.Json.JsonSerializer.Deserialize<ViewResponse>(
            createViewContent,
            DapperMaticOptions.JsonSerializerOptions
        );

        Assert.NotNull(viewResponse);
        Assert.NotNull(viewResponse.Results);
        Assert.Equal(viewName, viewResponse.Results.ViewName);
        Assert.Equal("SELECT 1 AS TestColumn", viewResponse.Results.Definition);
    }

    private async Task DeleteViewTestAsync(string viewName)
    {
        var deleteViewRequest = WebApiTestUtils.CreateAdminRequest(
            HttpMethod.Delete,
            ApiUrl(viewName)
        );
        var deleteViewResponse = await _client.SendAsync(deleteViewRequest);
        var deleteViewContent = await deleteViewResponse.Content.ReadAsStringAsync();

        if (!deleteViewResponse.IsSuccessStatusCode)
        {
            _output.WriteLine(deleteViewContent);
        }

        Assert.Equal(HttpStatusCode.OK, deleteViewResponse.StatusCode);

        // get BoolResponse from content
        var boolResponse = System.Text.Json.JsonSerializer.Deserialize<BoolResponse>(
            deleteViewContent,
            DapperMaticOptions.JsonSerializerOptions
        );

        Assert.NotNull(boolResponse);
        Assert.True(boolResponse.Results);
    }

    private async Task GetViewsTestAsync(int expectedCount)
    {
        var getViewsRequest = WebApiTestUtils.CreateAdminRequest(HttpMethod.Get, ApiUrl(null));
        var getViewsResponse = await _client.SendAsync(getViewsRequest);
        var getViewsContent = await getViewsResponse.Content.ReadAsStringAsync();

        if (!getViewsResponse.IsSuccessStatusCode)
        {
            _output.WriteLine(getViewsContent);
        }

        Assert.Equal(HttpStatusCode.OK, getViewsResponse.StatusCode);

        // extract the content as a string
        var getViewsApiResponse = System.Text.Json.JsonSerializer.Deserialize<ViewListResponse>(
            getViewsContent,
            DapperMaticOptions.JsonSerializerOptions
        );

        Assert.NotNull(getViewsApiResponse);
        Assert.NotNull(getViewsApiResponse.Results);

        Assert.Equal(expectedCount, getViewsApiResponse.Results.Count);
    }
}
