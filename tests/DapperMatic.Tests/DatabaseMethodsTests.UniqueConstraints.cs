using DapperMatic.Models;

namespace DapperMatic.Tests;

public abstract partial class DatabaseMethodsTests
{
    [Fact]
    protected virtual async Task Can_perform_simple_CRUD_on_UniqueConstraints_Async()
    {
        using var connection = await OpenConnectionAsync();

        const string tableName = "testWithUc";
        const string columnName = "testColumn";
        const string uniqueConstraintName = "testUc";

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

        output.WriteLine($"Unique Constraint Exists: {tableName}.{uniqueConstraintName}");
        var exists = await connection.UniqueConstraintExistsAsync(
            null,
            tableName,
            uniqueConstraintName
        );
        Assert.False(exists);

        output.WriteLine($"Creating unique constraint: {tableName}.{uniqueConstraintName}");
        await connection.CreateUniqueConstraintIfNotExistsAsync(
            null,
            tableName,
            uniqueConstraintName,
            [new DxOrderedColumn(columnName)]
        );

        output.WriteLine($"Unique Constraint Exists: {tableName}.{uniqueConstraintName}");
        exists = await connection.UniqueConstraintExistsAsync(
            null,
            tableName,
            uniqueConstraintName
        );
        Assert.True(exists);
        exists = await connection.UniqueConstraintExistsOnColumnAsync(null, tableName, columnName);
        Assert.True(exists);

        var uniqueConstraintNames = await connection.GetUniqueConstraintNamesAsync(null, tableName);
        Assert.Contains(
            uniqueConstraintName,
            uniqueConstraintNames,
            StringComparer.OrdinalIgnoreCase
        );

        var uniqueConstraints = await connection.GetUniqueConstraintsAsync(null, tableName);
        Assert.Contains(
            uniqueConstraints,
            uc => uc.ConstraintName.Equals(uniqueConstraintName, StringComparison.OrdinalIgnoreCase)
        );

        output.WriteLine($"Dropping unique constraint: {tableName}.{uniqueConstraintName}");
        await connection.DropUniqueConstraintIfExistsAsync(null, tableName, uniqueConstraintName);

        output.WriteLine($"Unique Constraint Exists: {tableName}.{uniqueConstraintName}");
        exists = await connection.UniqueConstraintExistsAsync(
            null,
            tableName,
            uniqueConstraintName
        );
        Assert.False(exists);
    }
}
