using Dapper;
using DapperMatic.Models;
using Microsoft.Extensions.Logging;

namespace DapperMatic.Tests;

public abstract partial class DatabaseMethodsTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("my_app")]
    protected virtual async Task Can_perform_simple_CRUD_on_Tables_Async(string? schemaName)
    {
        using var db = await OpenConnectionAsync();
        await InitFreshSchemaAsync(db, schemaName);

        var supportsSchemas = db.SupportsSchemas();

        var tableName = "testTable";

        var exists = await db.DoesTableExistAsync(schemaName, tableName);
        if (exists)
            await db.DropTableIfExistsAsync(schemaName, tableName);

        exists = await db.DoesTableExistAsync(schemaName, tableName);
        Assert.False(exists);

        var nonExistentTable = await db.GetTableAsync(schemaName, tableName);
        Assert.Null(nonExistentTable);

        var table = new DxTable(
            schemaName,
            tableName,
            [
                new DxColumn(
                    schemaName,
                    tableName,
                    "id",
                    typeof(int),
                    null,
                    isPrimaryKey: true,
                    isAutoIncrement: true
                ),
                new DxColumn(schemaName, tableName, "name", typeof(string), null, isUnique: true)
            ]
        );
        var created = await db.CreateTableIfNotExistsAsync(table);
        Assert.True(created);

        var createdAgain = await db.CreateTableIfNotExistsAsync(table);
        Assert.False(createdAgain);

        exists = await db.DoesTableExistAsync(schemaName, tableName);
        Assert.True(exists);

        var tableNames = await db.GetTableNamesAsync(schemaName);
        Assert.NotEmpty(tableNames);
        Assert.Contains(tableName, tableNames, StringComparer.OrdinalIgnoreCase);

        var existingTable = await db.GetTableAsync(schemaName, tableName);
        Assert.NotNull(existingTable);

        if (supportsSchemas)
        {
            Assert.NotNull(existingTable.SchemaName);
            Assert.NotEmpty(existingTable.SchemaName);
        }
        Assert.Equal(tableName, existingTable.TableName, true);
        Assert.Equal(2, existingTable.Columns.Count);

        // rename the table
        var newName = "newTestTable";
        var renamed = await db.RenameTableIfExistsAsync(schemaName, tableName, newName);
        Assert.True(renamed);

        exists = await db.DoesTableExistAsync(schemaName, tableName);
        Assert.False(exists);

        exists = await db.DoesTableExistAsync(schemaName, newName);
        Assert.True(exists);

        existingTable = await db.GetTableAsync(schemaName, newName);
        Assert.NotNull(existingTable);
        Assert.Equal(newName, existingTable.TableName, true);

        tableNames = await db.GetTableNamesAsync(schemaName);
        Assert.Contains(newName, tableNames, StringComparer.OrdinalIgnoreCase);

        var schemaQualifiedTableName = db.GetSchemaQualifiedTableName(schemaName, newName);

        // add a new row
        var newRow = new { id = 0, name = "Test" };
        await db.ExecuteAsync(
            @$"INSERT INTO {schemaQualifiedTableName} (name) VALUES (@name)",
            newRow
        );

        // get all rows
        var rows = await db.QueryAsync<dynamic>(
            @$"SELECT * FROM {schemaQualifiedTableName}",
            new { }
        );
        Assert.Single(rows);

        // truncate the table
        await db.TruncateTableIfExistsAsync(schemaName, newName);
        rows = await db.QueryAsync<dynamic>(@$"SELECT * FROM {schemaQualifiedTableName}", new { });
        Assert.Empty(rows);

        // drop the table
        await db.DropTableIfExistsAsync(schemaName, newName);

        exists = await db.DoesTableExistAsync(schemaName, newName);
        Assert.False(exists);

        Output.WriteLine($"Table names: {0}", string.Join(", ", tableNames));
    }
}
