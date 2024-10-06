using DapperMatic.Models;

namespace DapperMatic.Tests;

public abstract partial class DatabaseMethodsTests
{
    [Fact]
    protected virtual async Task Can_perform_simple_CRUD_on_CheckConstraints_Async()
    {
        using var connection = await OpenConnectionAsync();

        var testTableName = "testTableCheckConstraints";
        await connection.CreateTableIfNotExistsAsync(
            null,
            testTableName,
            [new DxColumn(null, testTableName, "testColumn", typeof(int))]
        );

        var constraintName = $"ck_testTable";
        var exists = await connection.DoesCheckConstraintExistAsync(
            null,
            testTableName,
            constraintName
        );

        if (exists)
            await connection.DropCheckConstraintIfExistsAsync(null, testTableName, constraintName);

        await connection.CreateCheckConstraintIfNotExistsAsync(
            null,
            testTableName,
            null,
            constraintName,
            "testColumn > 0"
        );

        exists = await connection.DoesCheckConstraintExistAsync(
            null,
            testTableName,
            constraintName
        );
        Assert.True(exists);

        var existingConstraint = await connection.GetCheckConstraintAsync(
            null,
            testTableName,
            constraintName
        );
        Assert.Equal(
            constraintName,
            existingConstraint?.ConstraintName,
            StringComparer.OrdinalIgnoreCase
        );

        var checkConstraintNames = await connection.GetCheckConstraintNamesAsync(
            null,
            testTableName
        );
        Assert.Contains(constraintName, checkConstraintNames, StringComparer.OrdinalIgnoreCase);

        await connection.DropCheckConstraintIfExistsAsync(null, testTableName, constraintName);
        exists = await connection.DoesCheckConstraintExistAsync(
            null,
            testTableName,
            constraintName
        );
        Assert.False(exists);

        await connection.DropTableIfExistsAsync(null, testTableName);

        await connection.CreateTableIfNotExistsAsync(
            null,
            testTableName,
            [
                new DxColumn(null, testTableName, "testColumn", typeof(int)),
                new DxColumn(
                    null,
                    testTableName,
                    "testColumn2",
                    typeof(int),
                    checkExpression: "testColumn2 > 0"
                )
            ]
        );

        var checkConstraint = await connection.GetCheckConstraintOnColumnAsync(
            null,
            testTableName,
            "testColumn2"
        );
        Assert.NotNull(checkConstraint);
    }
}
