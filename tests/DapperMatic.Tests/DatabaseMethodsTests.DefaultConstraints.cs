using DapperMatic.Models;
using DapperMatic.Providers;

namespace DapperMatic.Tests;

public abstract partial class DatabaseMethodsTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("blah")]
    protected virtual async Task Can_perform_simple_CRUD_on_DefaultConstraints_Async(
        string? schemaName
    )
    {
        using var connection = await OpenConnectionAsync();

        if (!string.IsNullOrWhiteSpace(schemaName))
            await connection.CreateSchemaIfNotExistsAsync(schemaName);

        var testTableName = "testTableDefaultConstraints";
        var testColumnName = "testColumn";
        await connection.CreateTableIfNotExistsAsync(
            schemaName,
            testTableName,
            [new DxColumn(schemaName, testTableName, testColumnName, typeof(int))]
        );

        // in MySQL, default constraints are not named, so this MUST use the ProviderUtils method which is what DapperMatic uses internally
        var constraintName = ProviderUtils.GenerateDefaultConstraintName(
            testTableName,
            testColumnName
        );
        var exists = await connection.DoesDefaultConstraintExistAsync(
            schemaName,
            testTableName,
            constraintName
        );
        if (exists)
            await connection.DropDefaultConstraintIfExistsAsync(
                schemaName,
                testTableName,
                constraintName
            );

        await connection.CreateDefaultConstraintIfNotExistsAsync(
            schemaName,
            testTableName,
            testColumnName,
            constraintName,
            "0"
        );
        var existingConstraint = await connection.GetDefaultConstraintAsync(
            schemaName,
            testTableName,
            constraintName
        );
        Assert.Equal(
            constraintName,
            existingConstraint?.ConstraintName,
            StringComparer.OrdinalIgnoreCase
        );

        var defaultConstraintNames = await connection.GetDefaultConstraintNamesAsync(
            schemaName,
            testTableName
        );
        Assert.Contains(constraintName, defaultConstraintNames, StringComparer.OrdinalIgnoreCase);

        await connection.DropDefaultConstraintIfExistsAsync(
            schemaName,
            testTableName,
            constraintName
        );
        exists = await connection.DoesDefaultConstraintExistAsync(
            schemaName,
            testTableName,
            constraintName
        );
        Assert.False(exists);

        await connection.DropTableIfExistsAsync(schemaName, testTableName);

        await connection.CreateTableIfNotExistsAsync(
            schemaName,
            testTableName,
            [
                new DxColumn(schemaName, testTableName, testColumnName, typeof(int)),
                new DxColumn(
                    schemaName,
                    testTableName,
                    "testColumn2",
                    typeof(int),
                    defaultExpression: "0"
                )
            ]
        );
        var defaultConstraint = await connection.GetDefaultConstraintOnColumnAsync(
            schemaName,
            testTableName,
            "testColumn2"
        );
        Assert.NotNull(defaultConstraint);

        var tableDeleted = await connection.DropTableIfExistsAsync(schemaName, testTableName);
        Assert.True(tableDeleted);

        if (!string.IsNullOrWhiteSpace(schemaName))
            await connection.DropSchemaIfExistsAsync(schemaName);
    }
}
