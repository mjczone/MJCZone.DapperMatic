using Dapper;
using DapperMatic.Models;
using Microsoft.Extensions.Logging;

namespace DapperMatic.Tests;

public abstract partial class DatabaseMethodsTests
{
    [Fact]
    protected virtual async Task Can_perform_simple_CRUD_on_Views_Async()
    {
        using var connection = await OpenConnectionAsync();

        var supportsSchemas = await connection.SupportsSchemasAsync();

        await connection.CreateTableIfNotExistsAsync(
            null,
            "testTableForView",
            [
                new DxColumn(
                    null,
                    "testTableForView",
                    "id",
                    typeof(int),
                    isPrimaryKey: true,
                    isAutoIncrement: true
                ),
                new DxColumn(null, "testTableForView", "name", typeof(string))
            ]
        );

        var viewName = "testView";
        var definition = "SELECT * FROM testTableForView";
        var created = await connection.CreateViewIfNotExistsAsync(null, viewName, definition);
        Assert.True(created);

        var createdAgain = await connection.CreateViewIfNotExistsAsync(null, viewName, definition);
        Assert.False(createdAgain);

        var exists = await connection.DoesViewExistAsync(null, viewName);
        Assert.True(exists);

        var view = await connection.GetViewAsync(null, viewName);
        Assert.NotNull(view);

        var viewNames = await connection.GetViewNamesAsync(null);
        Assert.Contains(viewName, viewNames);

        await connection.ExecuteAsync("INSERT INTO testTableForView (name) VALUES ('test123')");
        await connection.ExecuteAsync("INSERT INTO testTableForView (name) VALUES ('test456')");
        var tableRowCount = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM testTableForView"
        );
        var viewRowCount = await connection.ExecuteScalarAsync<int>(
            "SELECT COUNT(*) FROM testView"
        );

        Assert.Equal(2, tableRowCount);
        Assert.Equal(2, viewRowCount);

        var updatedDefinition = " SELECT * FROM testTableForView WHERE id = 1";
        var updated = await connection.UpdateViewIfExistsAsync(
            null,
            viewName + "blahblahblah",
            updatedDefinition
        );
        Assert.False(updated);

        updated = await connection.UpdateViewIfExistsAsync(null, viewName, updatedDefinition);
        Assert.True(updated);

        var updatedView = await connection.GetViewAsync(null, viewName);
        Assert.NotNull(updatedView);
        Assert.Equal(updatedDefinition.Trim(), updatedView.Definition);

        var dropped = await connection.DropViewIfExistsAsync(null, viewName);
        Assert.True(dropped);

        exists = await connection.DoesViewExistAsync(null, viewName);
        Assert.False(exists);
    }
}
