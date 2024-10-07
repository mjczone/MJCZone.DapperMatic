using DapperMatic.Models;

namespace DapperMatic.Tests;

public abstract partial class DatabaseMethodsTests
{
    [Fact]
    protected virtual async Task Can_perform_simple_CRUD_on_CheckConstraints_Async()
    {
        using var connection = await OpenConnectionAsync();

        var supportsCheckConstraints = await connection.SupportsCheckConstraintsAsync();

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
        Assert.True(supportsCheckConstraints ? exists : !exists);

        var existingConstraint = await connection.GetCheckConstraintAsync(
            null,
            testTableName,
            constraintName
        );
        if (!supportsCheckConstraints)
            Assert.Null(existingConstraint);
        else
            Assert.Equal(
                constraintName,
                existingConstraint?.ConstraintName,
                StringComparer.OrdinalIgnoreCase
            );

        var checkConstraintNames = await connection.GetCheckConstraintNamesAsync(
            null,
            testTableName
        );
        if (!supportsCheckConstraints)
            Assert.Empty(checkConstraintNames);
        else
            Assert.Contains(constraintName, checkConstraintNames, StringComparer.OrdinalIgnoreCase);

        var dropped = await connection.DropCheckConstraintIfExistsAsync(
            null,
            testTableName,
            constraintName
        );
        if (!supportsCheckConstraints)
            Assert.False(dropped);
        else
        {
            Assert.True(dropped);
            exists = await connection.DoesCheckConstraintExistAsync(
                null,
                testTableName,
                constraintName
            );
        }

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
        if (!supportsCheckConstraints)
            Assert.Null(checkConstraint);
        else
            Assert.NotNull(checkConstraint);
    }
}
