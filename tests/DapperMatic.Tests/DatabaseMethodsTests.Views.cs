using Dapper;
using DapperMatic.Models;
using Microsoft.Extensions.Logging;

namespace DapperMatic.Tests;

public abstract partial class DatabaseMethodsTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("my_app")]
    protected virtual async Task Can_perform_simple_CRUD_on_Views_Async(string? schemaName)
    {
        using var db = await OpenConnectionAsync();
        await InitFreshSchemaAsync(db, schemaName);

        var supportsSchemas = db.SupportsSchemas();

        var tableForView = "testTableForView";

        var schemaQualifiedTableName = db.GetSchemaQualifiedTableName(schemaName, tableForView);

        await db.CreateTableIfNotExistsAsync(
            schemaName,
            tableForView,
            [
                new DxColumn(
                    schemaName,
                    tableForView,
                    "id",
                    typeof(int),
                    isPrimaryKey: true,
                    isAutoIncrement: true
                ),
                new DxColumn(schemaName, tableForView, "name", typeof(string))
            ]
        );

        var viewName = "testView";
        var definition = $"SELECT * FROM {schemaQualifiedTableName}";
        var created = await db.CreateViewIfNotExistsAsync(schemaName, viewName, definition);
        Assert.True(created);

        var createdAgain = await db.CreateViewIfNotExistsAsync(schemaName, viewName, definition);
        Assert.False(createdAgain);

        var exists = await db.DoesViewExistAsync(schemaName, viewName);
        Assert.True(exists);

        var view = await db.GetViewAsync(schemaName, viewName);
        Assert.NotNull(view);

        var viewNames = await db.GetViewNamesAsync(schemaName);
        Assert.Contains(viewName, viewNames, StringComparer.OrdinalIgnoreCase);

        await db.ExecuteAsync($"INSERT INTO {schemaQualifiedTableName} (name) VALUES ('test123')");
        await db.ExecuteAsync($"INSERT INTO {schemaQualifiedTableName} (name) VALUES ('test456')");
        var tableRowCount = await db.ExecuteScalarAsync<int>(
            $"SELECT COUNT(*) FROM {schemaQualifiedTableName}"
        );
        var viewRowCount = await db.ExecuteScalarAsync<int>(
            $"SELECT COUNT(*) FROM {schemaQualifiedTableName}"
        );

        Assert.Equal(2, tableRowCount);
        Assert.Equal(2, viewRowCount);

        var updatedName = viewName + "blahblahblah";
        var updatedDefinition = $"SELECT * FROM {schemaQualifiedTableName} WHERE id = 1";
        var updated = await db.UpdateViewIfExistsAsync(schemaName, updatedName, updatedDefinition);
        Assert.False(updated); // view doesn't exist

        var renamed = await db.RenameViewIfExistsAsync(schemaName, viewName, updatedName);
        Assert.True(renamed);

        var renamedView = await db.GetViewAsync(schemaName, updatedName);
        Assert.NotNull(renamedView);
        Assert.Equal(view.Definition, renamedView.Definition);

        updated = await db.UpdateViewIfExistsAsync(schemaName, updatedName, updatedDefinition);
        Assert.True(updated);

        var updatedView = await db.GetViewAsync(schemaName, updatedName);
        Assert.NotNull(updatedView);
        Assert.Contains("= 1", updatedView.Definition, StringComparison.OrdinalIgnoreCase);

        // databases often rewrite the definition, so we just check that it contains the updated definition
        Assert.StartsWith(
            "select ",
            updatedView.Definition.Trim(),
            StringComparison.OrdinalIgnoreCase
        );

        var dropped = await db.DropViewIfExistsAsync(schemaName, viewName);
        Assert.False(dropped);
        dropped = await db.DropViewIfExistsAsync(schemaName, updatedName);
        Assert.True(dropped);

        exists = await db.DoesViewExistAsync(schemaName, viewName);
        Assert.False(exists);
        exists = await db.DoesViewExistAsync(schemaName, updatedName);
        Assert.False(exists);

        await db.DropTableIfExistsAsync(schemaName, tableForView);
    }
}
