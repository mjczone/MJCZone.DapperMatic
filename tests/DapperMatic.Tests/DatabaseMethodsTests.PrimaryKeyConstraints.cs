using DapperMatic.Models;

namespace DapperMatic.Tests;

public abstract partial class DatabaseMethodsTests
{
    [Fact]
    protected virtual async Task Can_perform_simple_CRUD_on_PrimaryKeyConstraints_Async()
    {
        using var connection = await OpenConnectionAsync();

        const string tableName = "testWithPk";
        const string columnName = "testColumn";
        const string primaryKeyName = "testPk";

        await connection.CreateTableIfNotExistsAsync(
            null,
            tableName,
            [
                new DxColumn(
                    null,
                    tableName,
                    columnName,
                    typeof(int),
                    defaultExpression: "1",
                    isNullable: false
                )
            ]
        );
        output.WriteLine($"Primary Key Exists: {tableName}.{primaryKeyName}");
        var exists = await connection.PrimaryKeyConstraintExistsAsync(
            null,
            tableName,
            primaryKeyName
        );
        Assert.False(exists);
        output.WriteLine($"Creating primary key: {tableName}.{primaryKeyName}");
        await connection.CreatePrimaryKeyConstraintIfNotExistsAsync(
            null,
            tableName,
            primaryKeyName,
            [new DxOrderedColumn(columnName)]
        );
        output.WriteLine($"Primary Key Exists: {tableName}.{primaryKeyName}");
        exists = await connection.PrimaryKeyConstraintExistsAsync(null, tableName, primaryKeyName);
        Assert.True(exists);
        exists = await connection.PrimaryKeyConstraintExistsOnColumnAsync(
            null,
            tableName,
            columnName
        );
        Assert.True(exists);
        var primaryKeyNames = await connection.GetPrimaryKeyConstraintNamesAsync(null, tableName);
        Assert.Contains(primaryKeyName, primaryKeyNames, StringComparer.OrdinalIgnoreCase);
        var primaryKeys = await connection.GetPrimaryKeyConstraintsAsync(null, tableName);
        Assert.Contains(
            primaryKeys,
            pk => pk.ConstraintName.Equals(primaryKeyName, StringComparison.OrdinalIgnoreCase)
        );
        output.WriteLine($"Dropping primary key: {tableName}.{primaryKeyName}");
        await connection.DropPrimaryKeyConstraintIfExistsAsync(null, tableName, primaryKeyName);
        output.WriteLine($"Primary Key Exists: {tableName}.{primaryKeyName}");
        exists = await connection.PrimaryKeyConstraintExistsAsync(null, tableName, primaryKeyName);
        Assert.False(exists);
        await connection.DropTableIfExistsAsync(null, tableName);
    }
}
