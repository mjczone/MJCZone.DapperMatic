using Dapper;
using DapperMatic.Models;
using Microsoft.Extensions.Logging;

namespace DapperMatic.Tests;

public abstract partial class DatabaseMethodsTests
{
    [Fact]
    protected virtual async Task Can_perform_simple_CRUD_on_Tables_Async()
    {
        using var connection = await OpenConnectionAsync();

        var supportsSchemas = connection.SupportsSchemas();

        var tableName = "testTable";

        var exists = await connection.DoesTableExistAsync(null, tableName);
        if (exists)
            await connection.DropTableIfExistsAsync(null, tableName);

        exists = await connection.DoesTableExistAsync(null, tableName);
        Assert.False(exists);

        var nonExistentTable = await connection.GetTableAsync(null, tableName);
        Assert.Null(nonExistentTable);

        var table = new DxTable(
            null,
            tableName,
            [
                new DxColumn(
                    null,
                    tableName,
                    "id",
                    typeof(int),
                    null,
                    isPrimaryKey: true,
                    isAutoIncrement: true
                ),
                new DxColumn(null, tableName, "name", typeof(string), null, isUnique: true)
            ]
        );
        var created = await connection.CreateTableIfNotExistsAsync(table);
        Assert.True(created);

        var createdAgain = await connection.CreateTableIfNotExistsAsync(table);
        Assert.False(createdAgain);

        exists = await connection.DoesTableExistAsync(null, tableName);
        Assert.True(exists);

        var tableNames = await connection.GetTableNamesAsync(null);
        Assert.NotEmpty(tableNames);
        Assert.Contains(tableName, tableNames, StringComparer.OrdinalIgnoreCase);

        var existingTable = await connection.GetTableAsync(null, tableName);
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
        var renamed = await connection.RenameTableIfExistsAsync(null, tableName, newName);
        Assert.True(renamed);

        exists = await connection.DoesTableExistAsync(null, tableName);
        Assert.False(exists);

        exists = await connection.DoesTableExistAsync(null, newName);
        Assert.True(exists);

        existingTable = await connection.GetTableAsync(null, newName);
        Assert.NotNull(existingTable);
        Assert.Equal(newName, existingTable.TableName, true);

        tableNames = await connection.GetTableNamesAsync(null);
        Assert.Contains(newName, tableNames, StringComparer.OrdinalIgnoreCase);

        // add a new row
        var newRow = new { id = 0, name = "Test" };
        await connection.ExecuteAsync(@$"INSERT INTO {newName} (name) VALUES (@name)", newRow);

        // get all rows
        var rows = await connection.QueryAsync<dynamic>(@$"SELECT * FROM {newName}", new { });
        Assert.Single(rows);

        // truncate the table
        await connection.TruncateTableIfExistsAsync(null, newName);
        rows = await connection.QueryAsync<dynamic>(@$"SELECT * FROM {newName}", new { });
        Assert.Empty(rows);

        // drop the table
        await connection.DropTableIfExistsAsync(null, newName);

        exists = await connection.DoesTableExistAsync(null, newName);
        Assert.False(exists);

        output.WriteLine($"Table names: {0}", string.Join(", ", tableNames));
    }
}
