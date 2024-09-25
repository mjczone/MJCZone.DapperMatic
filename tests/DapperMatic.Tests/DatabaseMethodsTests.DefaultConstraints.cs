using DapperMatic.Models;

namespace DapperMatic.Tests;

public abstract partial class DatabaseMethodsTests
{
    [Fact]
    protected virtual async Task Can_perform_simple_CRUD_on_DefaultConstraints_Async()
    {
        using var connection = await OpenConnectionAsync();

        await connection.CreateTableIfNotExistsAsync(
            null,
            "testTable",
            [new DxColumn(null, "testTable", "testColumn", typeof(int))]
        );
        var constraintName = $"df_testTable_testColumn";
        var exists = await connection.DoesDefaultConstraintExistAsync(
            null,
            "testTable",
            constraintName
        );
        if (exists)
            await connection.DropDefaultConstraintIfExistsAsync(null, "testTable", constraintName);

        await connection.CreateDefaultConstraintIfNotExistsAsync(
            null,
            "testTable",
            "testColumn",
            constraintName,
            "0"
        );

        exists = await connection.DoesDefaultConstraintExistAsync(
            null,
            "testTable",
            constraintName
        );
        Assert.True(exists);

        var existingConstraint = await connection.GetDefaultConstraintAsync(
            null,
            "testTable",
            constraintName
        );
        Assert.Equal(
            constraintName,
            existingConstraint?.ConstraintName,
            StringComparer.OrdinalIgnoreCase
        );
        var defaultConstraintNames = await connection.GetDefaultConstraintNamesAsync(
            null,
            "testTable"
        );
        Assert.Contains(constraintName, defaultConstraintNames, StringComparer.OrdinalIgnoreCase);
        await connection.DropDefaultConstraintIfExistsAsync(null, "testTable", constraintName);
        exists = await connection.DoesDefaultConstraintExistAsync(
            null,
            "testTable",
            constraintName
        );
        Assert.False(exists);

        await connection.DropTableIfExistsAsync(null, "testTable");

        await connection.CreateTableIfNotExistsAsync(
            null,
            "testTable",
            [
                new DxColumn(null, "testTable", "testColumn", typeof(int)),
                new DxColumn(null, "testTable", "testColumn2", typeof(int), defaultExpression: "0")
            ]
        );
        var defaultConstraint = await connection.GetDefaultConstraintOnColumnAsync(
            null,
            "testTable",
            "testColumn2"
        );
        Assert.NotNull(defaultConstraint);
    }
}
