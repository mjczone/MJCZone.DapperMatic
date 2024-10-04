using DapperMatic.Models;
using Microsoft.Extensions.Logging;

namespace DapperMatic.Tests;

public abstract partial class DatabaseMethodsTests
{
    [Fact]
    protected virtual async Task Can_perform_simple_CRUD_on_UniqueConstraints_Async()
    {
        using var connection = await OpenConnectionAsync();

        var tableName = "testWithUc";
        var columnName = "testColumn";
        var columnName2 = "testColumn2";
        var uniqueConstraintName = "testUc";
        var uniqueConstraintName2 = "testUc2";

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

        Logger.LogInformation(
            "Unique Constraint Exists: {tableName}.{uniqueConstraintName}",
            tableName,
            uniqueConstraintName
        );
        var exists = await connection.DoesUniqueConstraintExistAsync(
            null,
            tableName,
            uniqueConstraintName
        );
        Assert.False(exists);

        Logger.LogInformation(
            "Unique Constraint2 Exists: {tableName}.{uniqueConstraintName2}",
            tableName,
            uniqueConstraintName2
        );
        exists = await connection.DoesUniqueConstraintExistAsync(
            null,
            tableName,
            uniqueConstraintName2
        );
        Assert.True(exists);
        exists = await connection.DoesUniqueConstraintExistOnColumnAsync(
            null,
            tableName,
            columnName2
        );
        Assert.True(exists);

        Logger.LogInformation(
            "Creating unique constraint: {tableName}.{uniqueConstraintName}",
            tableName,
            uniqueConstraintName
        );
        await connection.CreateUniqueConstraintIfNotExistsAsync(
            null,
            tableName,
            uniqueConstraintName,
            [new DxOrderedColumn(columnName)]
        );

        // make sure the new constraint is there
        Logger.LogInformation(
            "Unique Constraint Exists: {tableName}.{uniqueConstraintName}",
            tableName,
            uniqueConstraintName
        );
        exists = await connection.DoesUniqueConstraintExistAsync(
            null,
            tableName,
            uniqueConstraintName
        );
        Assert.True(exists);
        exists = await connection.DoesUniqueConstraintExistOnColumnAsync(
            null,
            tableName,
            columnName
        );
        Assert.True(exists);

        // make sure the original constraint is still there
        Logger.LogInformation(
            "Unique Constraint Exists: {tableName}.{uniqueConstraintName2}",
            tableName,
            uniqueConstraintName2
        );
        exists = await connection.DoesUniqueConstraintExistAsync(
            null,
            tableName,
            uniqueConstraintName2
        );
        Assert.True(exists);
        exists = await connection.DoesUniqueConstraintExistOnColumnAsync(
            null,
            tableName,
            columnName2
        );
        Assert.True(exists);

        Logger.LogInformation("Get Unique Constraint Names: {tableName}", tableName);
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

        Logger.LogInformation(
            "Dropping unique constraint: {tableName}.{uniqueConstraintName}",
            tableName,
            uniqueConstraintName
        );
        await connection.DropUniqueConstraintIfExistsAsync(null, tableName, uniqueConstraintName);

        Logger.LogInformation(
            "Unique Constraint Exists: {tableName}.{uniqueConstraintName}",
            tableName,
            uniqueConstraintName
        );
        exists = await connection.DoesUniqueConstraintExistAsync(
            null,
            tableName,
            uniqueConstraintName
        );
        Assert.False(exists);

        // test key ordering
        tableName = "testWithUc2";
        uniqueConstraintName = "uc_testWithUc2";
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
            uniqueConstraints:
            [
                new DxUniqueConstraint(
                    null,
                    tableName,
                    uniqueConstraintName,
                    [
                        new DxOrderedColumn(columnName2, DxColumnOrder.Ascending),
                        new DxOrderedColumn(columnName, DxColumnOrder.Descending)
                    ]
                )
            ]
        );

        var uniqueConstraint = await connection.GetUniqueConstraintAsync(
            null,
            tableName,
            uniqueConstraintName
        );
        Assert.NotNull(uniqueConstraint);
        Assert.NotNull(uniqueConstraint.Columns);
        Assert.Equal(2, uniqueConstraint.Columns.Length);
        Assert.Equal(
            columnName2,
            uniqueConstraint.Columns[0].ColumnName,
            StringComparer.OrdinalIgnoreCase
        );
        Assert.Equal(DxColumnOrder.Ascending, uniqueConstraint.Columns[0].Order);
        Assert.Equal(
            columnName,
            uniqueConstraint.Columns[1].ColumnName,
            StringComparer.OrdinalIgnoreCase
        );
        if (connection.SupportsOrderedKeysInConstraints())
        {
            Assert.Equal(DxColumnOrder.Descending, uniqueConstraint.Columns[1].Order);
        }
        await connection.DropTableIfExistsAsync(null, tableName);
    }
}
