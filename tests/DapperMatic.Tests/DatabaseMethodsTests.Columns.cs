using DapperMatic.Models;
using Microsoft.Extensions.Logging;

namespace DapperMatic.Tests;

public abstract partial class DatabaseMethodsTests
{
    [Fact]
    protected virtual async Task Can_perform_simple_CRUD_on_Columns_Async()
    {
        using var connection = await OpenConnectionAsync();

        const string tableName = "testWithColumn";
        const string columnName = "testColumn";

        string? defaultDateTimeSql = null;
        string? defaultGuidSql = null;
        var dbType = connection.GetDbProviderType();
        switch (dbType)
        {
            case DbProviderType.SqlServer:
                defaultDateTimeSql = "GETUTCDATE()";
                defaultGuidSql = "NEWID()";
                break;
            case DbProviderType.Sqlite:
                defaultDateTimeSql = "CURRENT_TIMESTAMP";
                //this could be supported IF the sqlite UUID extension was loaded and enabled
                //defaultGuidSql = "uuid_blob(uuid())";
                defaultGuidSql = null;
                break;
            case DbProviderType.PostgreSql:
                defaultDateTimeSql = "CURRENT_TIMESTAMP";
                defaultGuidSql = "uuid_generate_v4()";
                break;
            case DbProviderType.MySql:
                defaultDateTimeSql = "CURRENT_TIMESTAMP";
                // only supported after 8.0.13
                // defaultGuidSql = "UUID()";
                break;
        }

        await connection.DropColumnIfExistsAsync(null, tableName, columnName);

        Logger.LogInformation("Column Exists: {tableName}.{columnName}", tableName, columnName);
        var exists = await connection.DoesColumnExistAsync(null, tableName, columnName);
        Assert.False(exists);

        await connection.CreateTableIfNotExistsAsync(
            null,
            tableName,
            [
                new DxColumn(
                    null,
                    tableName,
                    "id",
                    typeof(int),
                    isPrimaryKey: true,
                    isAutoIncrement: true,
                    isNullable: false
                ),
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

        Logger.LogInformation("Column Exists: {tableName}.{columnName}", tableName, columnName);
        exists = await connection.DoesColumnExistAsync(null, tableName, columnName);
        Assert.True(exists);

        Logger.LogInformation(
            "Dropping columnName: {tableName}.{columnName}",
            tableName,
            columnName
        );
        await connection.DropColumnIfExistsAsync(null, tableName, columnName);

        Logger.LogInformation("Column Exists: {tableName}.{columnName}", tableName, columnName);
        exists = await connection.DoesColumnExistAsync(null, tableName, columnName);
        Assert.False(exists);

        // try adding a columnName of all the supported types
        var columnCount = 1;
        var addColumns = new List<DxColumn>
        {
            new(null, "testWithAllColumns", "abc", typeof(int)),
            new(
                null,
                "testWithAllColumns",
                "id" + columnCount++,
                typeof(int),
                isPrimaryKey: true,
                isAutoIncrement: true
            ),
            new(null, "testWithAllColumns", "id" + columnCount++, typeof(int), isUnique: true),
            new(
                null,
                "testWithAllColumns",
                "id" + columnCount++,
                typeof(int),
                isUnique: true,
                isIndexed: true
            ),
            new(null, "testWithAllColumns", "id" + columnCount++, typeof(int), isIndexed: true),
            //     new(
            //         null,
            //         "testWithAllColumns",
            //         "id" + columnCount++,
            //         typeof(int),
            //         isForeignKey: true,
            //         referencedTableName: tableName,
            //         referencedColumnName: "id",
            //         onDelete: DxForeignKeyAction.Cascade,
            //         onUpdate: DxForeignKeyAction.Cascade
            //     ),
            //     new(
            //         null,
            //         "testWithAllColumns",
            //         "createdDateColumn" + columnCount++,
            //         typeof(DateTime),
            //         defaultExpression: defaultDateTimeSql
            //     ),
            //     new(
            //         null,
            //         "testWithAllColumns",
            //         "newidColumn" + columnCount++,
            //         typeof(Guid),
            //         defaultExpression: defaultGuidSql
            //     ),
            //     new(null, "testWithAllColumns", "bigintColumn" + columnCount++, typeof(long)),
            //     new(null, "testWithAllColumns", "binaryColumn" + columnCount++, typeof(byte[])),
            //     new(null, "testWithAllColumns", "bitColumn" + columnCount++, typeof(bool)),
            //     new(
            //         null,
            //         "testWithAllColumns",
            //         "charColumn" + columnCount++,
            //         typeof(string),
            //         length: 10
            //     ),
            //     new(null, "testWithAllColumns", "dateColumn" + columnCount++, typeof(DateTime)),
            //     new(null, "testWithAllColumns", "datetimeColumn" + columnCount++, typeof(DateTime)),
            //     new(null, "testWithAllColumns", "datetime2Column" + columnCount++, typeof(DateTime)),
            //     new(
            //         null,
            //         "testWithAllColumns",
            //         "datetimeoffsetColumn" + columnCount++,
            //         typeof(DateTimeOffset)
            //     ),
            //     new(null, "testWithAllColumns", "decimalColumn" + columnCount++, typeof(decimal)),
            //     new(
            //         null,
            //         "testWithAllColumns",
            //         "decimalColumnWithPrecision" + columnCount++,
            //         typeof(decimal),
            //         precision: 10
            //     ),
            //     new(
            //         null,
            //         "testWithAllColumns",
            //         "decimalColumnWithPrecisionAndScale" + columnCount++,
            //         typeof(decimal),
            //         precision: 10,
            //         scale: 5
            //     ),
            //     new(null, "testWithAllColumns", "floatColumn" + columnCount++, typeof(double)),
            //     new(null, "testWithAllColumns", "imageColumn" + columnCount++, typeof(byte[])),
            //     new(null, "testWithAllColumns", "intColumn" + columnCount++, typeof(int)),
            //     new(null, "testWithAllColumns", "moneyColumn" + columnCount++, typeof(decimal)),
            //     new(
            //         null,
            //         "testWithAllColumns",
            //         "ncharColumn" + columnCount++,
            //         typeof(string),
            //         length: 10
            //     ),
            //     new(
            //         null,
            //         "testWithAllColumns",
            //         "ntextColumn" + columnCount++,
            //         typeof(string),
            //         length: int.MaxValue
            //     ),
            //     new(null, "testWithAllColumns", "floatColumn2" + columnCount++, typeof(float)),
            //     new(null, "testWithAllColumns", "doubleColumn2" + columnCount++, typeof(double)),
            //     new(null, "testWithAllColumns", "guidArrayColumn" + columnCount++, typeof(Guid[])),
            //     new(null, "testWithAllColumns", "intArrayColumn" + columnCount++, typeof(int[])),
            //     new(null, "testWithAllColumns", "longArrayColumn" + columnCount++, typeof(long[])),
            //     new(null, "testWithAllColumns", "doubleArrayColumn" + columnCount++, typeof(double[])),
            //     new(
            //         null,
            //         "testWithAllColumns",
            //         "decimalArrayColumn" + columnCount++,
            //         typeof(decimal[])
            //     ),
            //     new(null, "testWithAllColumns", "stringArrayColumn" + columnCount++, typeof(string[])),
            //     new(
            //         null,
            //         "testWithAllColumns",
            //         "stringDectionaryArrayColumn" + columnCount++,
            //         typeof(Dictionary<string, string>)
            //     ),
            //     new(
            //         null,
            //         "testWithAllColumns",
            //         "objectDectionaryArrayColumn" + columnCount++,
            //         typeof(Dictionary<string, object>)
            //     )
        };
        await connection.CreateTableIfNotExistsAsync(null, "testWithAllColumns", [addColumns[0]]);
        foreach (var col in addColumns.Skip(1))
        {
            await connection.CreateColumnIfNotExistsAsync(col);
            var columns = await connection.GetColumnsAsync(null, "testWithAllColumns");
            // immediately do a check to make sure column was created as expected
            var column = await connection.GetColumnAsync(
                null,
                "testWithAllColumns",
                col.ColumnName
            );
            try
            {
                Assert.NotNull(column);
                Assert.Equal(col.IsIndexed, column.IsIndexed);
                Assert.Equal(col.IsUnique, column.IsUnique);
                Assert.Equal(col.IsPrimaryKey, column.IsPrimaryKey);
                Assert.Equal(col.IsAutoIncrement, column.IsAutoIncrement);
                Assert.Equal(col.IsNullable, column.IsNullable);
                Assert.Equal(col.IsForeignKey, column.IsForeignKey);
                // Assert.Equal(col.DotnetType, column.DotnetType);
                // Assert.Equal(col.ProviderDataType, column.ProviderDataType);
                Assert.Equal(col.Length, column.Length);
                Assert.Equal(col.Precision, column.Precision);
                Assert.Equal(col.Scale ?? 0, column.Scale ?? 0);
            }
            catch (Exception ex)
            {
                Logger.LogError(
                    ex,
                    "Error validating column {columnName}: {message}",
                    col.ColumnName,
                    ex.Message
                );
                column = await connection.GetColumnAsync(
                    null,
                    "testWithAllColumns",
                    col.ColumnName
                );
            }
        }

        var columnNames = await connection.GetColumnNamesAsync(null, "testWithAllColumns");
        Assert.Equal(columnCount, columnNames.Count());

        // validate that:
        // - all columns are of the expected types
        // - all indexes are created correctly
        // - all foreign keys are created correctly
        // - all default values are set correctly
        // - all column lengths are set correctly
        // - all column scales are set correctly
        // - all column precision is set correctly
        // - all columns are nullable or not nullable as specified
        // - all columns are unique or not unique as specified
        // - all columns are indexed or not indexed as specified
        // - all columns are foreign key or not foreign key as specified
        var table = await connection.GetTableAsync(null, "testWithAllColumns");
        Assert.NotNull(table);

        foreach (var column in table.Columns)
        {
            var originalColumn = addColumns.SingleOrDefault(c => c.ColumnName == column.ColumnName);
            Assert.NotNull(originalColumn);
        }

        // general count tests
        Assert.Equal(
            addColumns.Count(c => !c.IsIndexed && c.IsUnique),
            table.UniqueConstraints.Count()
        );
        Assert.Equal(
            addColumns.Count(c => c.IsIndexed && !c.IsUnique),
            table.Indexes.Count(c => !c.IsUnique)
        );
        Assert.Equal(
            addColumns.Count(c => c.IsIndexed && c.IsUnique),
            table.Indexes.Count(c => c.IsUnique)
        );
        Assert.Equal(addColumns.Count(c => c.IsForeignKey), table.ForeignKeyConstraints.Count());
        Assert.Equal(
            addColumns.Count(c => c.DefaultExpression != null),
            table.DefaultConstraints.Count()
        );
        Assert.Equal(
            addColumns.Count(c => c.CheckExpression != null),
            table.CheckConstraints.Count()
        );
        Assert.Equal(addColumns.Count(c => c.IsNullable), table.Columns.Count(c => c.IsNullable));
        Assert.Equal(
            addColumns.Count(c => c.IsPrimaryKey && c.IsAutoIncrement),
            table.Columns.Count(c => c.IsPrimaryKey && c.IsAutoIncrement)
        );
        Assert.Equal(addColumns.Count(c => c.IsUnique), table.Columns.Count(c => c.IsUnique));
        Assert.Equal(addColumns.Count(c => c.IsIndexed), table.Columns.Count(c => c.IsIndexed));
    }
}
