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

        var supportsSchemas = connection.SupportsSchemas();

        var tableForView = "testTableForView";
        await connection.CreateTableIfNotExistsAsync(
            null,
            tableForView,
            [
                new DxColumn(
                    null,
                    tableForView,
                    "id",
                    typeof(int),
                    isPrimaryKey: true,
                    isAutoIncrement: true
                ),
                new DxColumn(null, tableForView, "name", typeof(string))
            ]
        );

        var viewName = "testView";
        var definition = $"SELECT * FROM {connection.NormalizeName(tableForView)}";
        var created = await connection.CreateViewIfNotExistsAsync(null, viewName, definition);
        Assert.True(created);

        var createdAgain = await connection.CreateViewIfNotExistsAsync(null, viewName, definition);
        Assert.False(createdAgain);

        var exists = await connection.DoesViewExistAsync(null, viewName);
        Assert.True(exists);

        var view = await connection.GetViewAsync(null, viewName);
        Assert.NotNull(view);

        var viewNames = await connection.GetViewNamesAsync(null);
        Assert.Contains(viewName, viewNames, StringComparer.OrdinalIgnoreCase);

        await connection.ExecuteAsync(
            $"INSERT INTO {connection.NormalizeName(tableForView)} (name) VALUES ('test123')"
        );
        await connection.ExecuteAsync(
            $"INSERT INTO {connection.NormalizeName(tableForView)} (name) VALUES ('test456')"
        );
        var tableRowCount = await connection.ExecuteScalarAsync<int>(
            $"SELECT COUNT(*) FROM {connection.NormalizeName(tableForView)}"
        );
        var viewRowCount = await connection.ExecuteScalarAsync<int>(
            $"SELECT COUNT(*) FROM {connection.NormalizeName(viewName)}"
        );

        Assert.Equal(2, tableRowCount);
        Assert.Equal(2, viewRowCount);

        var updatedName = viewName + "blahblahblah";
        var updatedDefinition =
            $"SELECT * FROM {connection.NormalizeName(tableForView)} WHERE id = 1";
        var updated = await connection.UpdateViewIfExistsAsync(
            null,
            updatedName,
            updatedDefinition
        );
        Assert.False(updated); // view doesn't exist

        var renamed = await connection.RenameViewIfExistsAsync(null, viewName, updatedName);
        Assert.True(renamed);

        var renamedView = await connection.GetViewAsync(null, updatedName);
        Assert.NotNull(renamedView);
        Assert.Equal(view.Definition, renamedView.Definition);

        updated = await connection.UpdateViewIfExistsAsync(null, updatedName, updatedDefinition);
        Assert.True(updated);

        var updatedView = await connection.GetViewAsync(null, updatedName);
        Assert.NotNull(updatedView);
        Assert.Contains("id = 1", updatedView.Definition, StringComparison.OrdinalIgnoreCase);

        // databases often rewrite the definition, so we just check that it contains the updated definition
        Assert.StartsWith(
            "select ",
            updatedView.Definition.Trim(),
            StringComparison.OrdinalIgnoreCase
        );

        var dropped = await connection.DropViewIfExistsAsync(null, viewName);
        Assert.False(dropped);
        dropped = await connection.DropViewIfExistsAsync(null, updatedName);
        Assert.True(dropped);

        exists = await connection.DoesViewExistAsync(null, viewName);
        Assert.False(exists);
        exists = await connection.DoesViewExistAsync(null, updatedName);
        Assert.False(exists);
    }
}
