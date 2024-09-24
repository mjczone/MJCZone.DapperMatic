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
        const string columnName2 = "testColumn2";
        const string uniqueConstraintName = "testUc";
        const string uniqueConstraintName2 = "testUc2";

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
                ),
                new DxColumn(
                    null,
                    tableName,
                    columnName2,
                    typeof(int),
                    defaultExpression: "1",
                    isNullable: false
                )
            ],
            uniqueConstraints: new[]
            {
                new DxUniqueConstraint(
                    null,
                    tableName,
                    uniqueConstraintName2,
                    [new DxOrderedColumn(columnName2)]
                )
            }
        );

        output.WriteLine($"Unique Constraint Exists: {tableName}.{uniqueConstraintName}");
        var exists = await connection.UniqueConstraintExistsAsync(
            null,
            tableName,
            uniqueConstraintName
        );
        Assert.False(exists);

        output.WriteLine($"Unique Constraint2 Exists: {tableName}.{uniqueConstraintName2}");
        exists = await connection.UniqueConstraintExistsAsync(
            null,
            tableName,
            uniqueConstraintName2
        );
        Assert.True(exists);
        exists = await connection.UniqueConstraintExistsOnColumnAsync(null, tableName, columnName2);
        Assert.True(exists);

        output.WriteLine($"Creating unique constraint: {tableName}.{uniqueConstraintName}");
        await connection.CreateUniqueConstraintIfNotExistsAsync(
            null,
            tableName,
            uniqueConstraintName,
            [new DxOrderedColumn(columnName)]
        );

        // make sure the new constraint is there
        output.WriteLine($"Unique Constraint Exists: {tableName}.{uniqueConstraintName}");
        exists = await connection.UniqueConstraintExistsAsync(
            null,
            tableName,
            uniqueConstraintName
        );
        Assert.True(exists);
        exists = await connection.UniqueConstraintExistsOnColumnAsync(null, tableName, columnName);
        Assert.True(exists);

        // make sure the original constraint is still there
        output.WriteLine($"Unique Constraint Exists: {tableName}.{uniqueConstraintName2}");
        exists = await connection.UniqueConstraintExistsAsync(
            null,
            tableName,
            uniqueConstraintName2
        );
        Assert.True(exists);
        exists = await connection.UniqueConstraintExistsOnColumnAsync(null, tableName, columnName2);
        Assert.True(exists);

        output.WriteLine($"Get Unique Constraint Names: {tableName}");
        var uniqueConstraintNames = await connection.GetUniqueConstraintNamesAsync(null, tableName);
        Assert.Contains(
            uniqueConstraintName2,
            uniqueConstraintNames,
            StringComparer.OrdinalIgnoreCase
        );
        Assert.Contains(
            uniqueConstraintName,
            uniqueConstraintNames,
            StringComparer.OrdinalIgnoreCase
        );

        var uniqueConstraints = await connection.GetUniqueConstraintsAsync(null, tableName);
        Assert.Contains(
            uniqueConstraints,
            uc =>
                uc.ConstraintName.Equals(uniqueConstraintName2, StringComparison.OrdinalIgnoreCase)
        );
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
