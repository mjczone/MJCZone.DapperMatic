using DapperMatic.Models;

namespace DapperMatic.Tests;

public abstract partial class DatabaseMethodsTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("my_app")]
    protected virtual async Task Can_perform_simple_CRUD_on_UniqueConstraints_Async(
        string? schemaName
    )
    {
        using var db = await OpenConnectionAsync();
        await InitFreshSchemaAsync(db, schemaName);

        var tableName = "testWithUc" + DateTime.Now.Ticks;
        var columnName = "testColumn";
        var columnName2 = "testColumn2";
        var uniqueConstraintName = "testUc";
        var uniqueConstraintName2 = "testUc2";

        await db.CreateTableIfNotExistsAsync(
            schemaName,
            tableName,
            [
                new DxColumn(
                    schemaName,
                    tableName,
                    columnName,
                    typeof(int),
                    defaultExpression: "1",
                    isNullable: false
                ),
                new DxColumn(
                    schemaName,
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
                    schemaName,
                    tableName,
                    uniqueConstraintName2,
                    [new DxOrderedColumn(columnName2)]
                )
            ]
        );

        Output.WriteLine("Unique Constraint Exists: {0}.{1}", tableName, uniqueConstraintName);
        var exists = await db.DoesUniqueConstraintExistAsync(
            schemaName,
            tableName,
            uniqueConstraintName
        );
        Assert.False(exists);

        Output.WriteLine("Unique Constraint2 Exists: {0}.{1}", tableName, uniqueConstraintName2);
        exists = await db.DoesUniqueConstraintExistAsync(
            schemaName,
            tableName,
            uniqueConstraintName2
        );
        Assert.True(exists);
        exists = await db.DoesUniqueConstraintExistOnColumnAsync(
            schemaName,
            tableName,
            columnName2
        );
        Assert.True(exists);

        Output.WriteLine("Creating unique constraint: {0}.{1}", tableName, uniqueConstraintName);
        await db.CreateUniqueConstraintIfNotExistsAsync(
            schemaName,
            tableName,
            uniqueConstraintName,
            [new DxOrderedColumn(columnName)]
        );

        // make sure the new constraint is there
        Output.WriteLine("Unique Constraint Exists: {0}.{1}", tableName, uniqueConstraintName);
        exists = await db.DoesUniqueConstraintExistAsync(
            schemaName,
            tableName,
            uniqueConstraintName
        );
        Assert.True(exists);
        exists = await db.DoesUniqueConstraintExistOnColumnAsync(schemaName, tableName, columnName);
        Assert.True(exists);

        // make sure the original constraint is still there
        Output.WriteLine("Unique Constraint Exists: {0}.{1}", tableName, uniqueConstraintName2);
        exists = await db.DoesUniqueConstraintExistAsync(
            schemaName,
            tableName,
            uniqueConstraintName2
        );
        Assert.True(exists);
        exists = await db.DoesUniqueConstraintExistOnColumnAsync(
            schemaName,
            tableName,
            columnName2
        );
        Assert.True(exists);

        Output.WriteLine("Get Unique Constraint Names: {0}", tableName);
        var uniqueConstraintNames = await db.GetUniqueConstraintNamesAsync(schemaName, tableName);
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

        var uniqueConstraints = await db.GetUniqueConstraintsAsync(schemaName, tableName);
        Assert.Contains(
            uniqueConstraints,
            uc =>
                uc.ConstraintName.Equals(uniqueConstraintName2, StringComparison.OrdinalIgnoreCase)
        );
        Assert.Contains(
            uniqueConstraints,
            uc => uc.ConstraintName.Equals(uniqueConstraintName, StringComparison.OrdinalIgnoreCase)
        );

        Output.WriteLine("Dropping unique constraint: {0}.{1}", tableName, uniqueConstraintName);
        await db.DropUniqueConstraintIfExistsAsync(schemaName, tableName, uniqueConstraintName);

        Output.WriteLine("Unique Constraint Exists: {0}.{1}", tableName, uniqueConstraintName);
        exists = await db.DoesUniqueConstraintExistAsync(
            schemaName,
            tableName,
            uniqueConstraintName
        );
        Assert.False(exists);

        // test key ordering
        tableName = "testWithUc2";
        uniqueConstraintName = "uc_testWithUc2";
        await db.CreateTableIfNotExistsAsync(
            schemaName,
            tableName,
            [
                new DxColumn(
                    schemaName,
                    tableName,
                    columnName,
                    typeof(int),
                    defaultExpression: "1",
                    isNullable: false
                ),
                new DxColumn(
                    schemaName,
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
                    schemaName,
                    tableName,
                    uniqueConstraintName,
                    [
                        new DxOrderedColumn(columnName2),
                        new DxOrderedColumn(columnName, DxColumnOrder.Descending)
                    ]
                )
            ]
        );

        var uniqueConstraint = await db.GetUniqueConstraintAsync(
            schemaName,
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
        if (await db.SupportsOrderedKeysInConstraintsAsync())
        {
            Assert.Equal(DxColumnOrder.Descending, uniqueConstraint.Columns[1].Order);
        }
        await db.DropTableIfExistsAsync(schemaName, tableName);
    }
}
