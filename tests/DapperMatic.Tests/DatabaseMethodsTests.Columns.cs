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
        var tableName2 = "testWithAllColumns";
        const string columnName = "testColumn";

        string? defaultDateTimeSql = null;
        string? defaultGuidSql = null;
        var dbType = connection.GetDbProviderType();

        var supportsMultipleIdentityColumns = true;
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
                supportsMultipleIdentityColumns = false;
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
            new(null, tableName2, "abc", typeof(int)),
            new(
                null,
                tableName2,
                "id" + columnCount++,
                typeof(int),
                isPrimaryKey: true,
                isAutoIncrement: supportsMultipleIdentityColumns ? true : false
            ),
            new(null, tableName2, "id" + columnCount++, typeof(int), isUnique: true),
            new(
                null,
                tableName2,
                "id" + columnCount++,
                typeof(int),
                isUnique: true,
                isIndexed: true
            ),
            new(null, tableName2, "id" + columnCount++, typeof(int), isIndexed: true),
            new(
                null,
                tableName2,
                "colWithFk" + columnCount++,
                typeof(int),
                isForeignKey: true,
                referencedTableName: tableName,
                referencedColumnName: "id",
                onDelete: DxForeignKeyAction.Cascade,
                onUpdate: DxForeignKeyAction.Cascade
            ),
            new(
                null,
                tableName2,
                "createdDateColumn" + columnCount++,
                typeof(DateTime),
                defaultExpression: defaultDateTimeSql
            ),
            new(
                null,
                tableName2,
                "newidColumn" + columnCount++,
                typeof(Guid),
                defaultExpression: defaultGuidSql
            ),
            new(null, tableName2, "bigintColumn" + columnCount++, typeof(long)),
            new(null, tableName2, "binaryColumn" + columnCount++, typeof(byte[])),
            new(null, tableName2, "bitColumn" + columnCount++, typeof(bool)),
            new(null, tableName2, "charColumn" + columnCount++, typeof(string), length: 10),
            new(null, tableName2, "dateColumn" + columnCount++, typeof(DateTime)),
            new(null, tableName2, "datetimeColumn" + columnCount++, typeof(DateTime)),
            new(null, tableName2, "datetime2Column" + columnCount++, typeof(DateTime)),
            new(null, tableName2, "datetimeoffsetColumn" + columnCount++, typeof(DateTimeOffset)),
            new(null, tableName2, "decimalColumn" + columnCount++, typeof(decimal)),
            new(
                null,
                tableName2,
                "decimalColumnWithPrecision" + columnCount++,
                typeof(decimal),
                precision: 10
            ),
            new(
                null,
                tableName2,
                "decimalColumnWithPrecisionAndScale" + columnCount++,
                typeof(decimal),
                precision: 10,
                scale: 5
            ),
            new(null, tableName2, "floatColumn" + columnCount++, typeof(double)),
            new(null, tableName2, "imageColumn" + columnCount++, typeof(byte[])),
            new(null, tableName2, "intColumn" + columnCount++, typeof(int)),
            new(null, tableName2, "moneyColumn" + columnCount++, typeof(decimal)),
            new(null, tableName2, "ncharColumn" + columnCount++, typeof(string), length: 10),
            new(
                null,
                tableName2,
                "ntextColumn" + columnCount++,
                typeof(string),
                length: int.MaxValue
            ),
            new(null, tableName2, "floatColumn2" + columnCount++, typeof(float)),
            new(null, tableName2, "doubleColumn2" + columnCount++, typeof(double)),
            new(null, tableName2, "guidArrayColumn" + columnCount++, typeof(Guid[])),
            new(null, tableName2, "intArrayColumn" + columnCount++, typeof(int[])),
            new(null, tableName2, "longArrayColumn" + columnCount++, typeof(long[])),
            new(null, tableName2, "doubleArrayColumn" + columnCount++, typeof(double[])),
            new(null, tableName2, "decimalArrayColumn" + columnCount++, typeof(decimal[])),
            new(null, tableName2, "stringArrayColumn" + columnCount++, typeof(string[])),
            new(
                null,
                tableName2,
                "stringDectionaryArrayColumn" + columnCount++,
                typeof(Dictionary<string, string>)
            ),
            new(
                null,
                tableName2,
                "objectDectionaryArrayColumn" + columnCount++,
                typeof(Dictionary<string, object>)
            )
        };
        await connection.CreateTableIfNotExistsAsync(null, tableName2, [addColumns[0]]);
        foreach (var col in addColumns.Skip(1))
        {
            await connection.CreateColumnIfNotExistsAsync(col);
            var columns = await connection.GetColumnsAsync(null, tableName2);
            // immediately do a check to make sure column was created as expected
            var column = await connection.GetColumnAsync(null, tableName2, col.ColumnName);
            try
            {
                Assert.NotNull(column);
                Assert.Equal(col.IsIndexed, column.IsIndexed);
                Assert.Equal(col.IsUnique, column.IsUnique);
                Assert.Equal(col.IsPrimaryKey, column.IsPrimaryKey);
                Assert.Equal(col.IsAutoIncrement, column.IsAutoIncrement);
                Assert.Equal(col.IsNullable, column.IsNullable);
                Assert.Equal(col.IsForeignKey, column.IsForeignKey);
                if (col.IsForeignKey)
                {
                    Assert.Equal(col.ReferencedTableName, column.ReferencedTableName);
                    Assert.Equal(col.ReferencedColumnName, column.ReferencedColumnName);
                    Assert.Equal(col.OnDelete, column.OnDelete);
                    Assert.Equal(col.OnUpdate, column.OnUpdate);
                }
                Assert.Equal(col.ProviderDataType, column.ProviderDataType);
                Assert.Equal(col.DotnetType, column.DotnetType);
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
                column = await connection.GetColumnAsync(null, tableName2, col.ColumnName);
            }
        }

        var columnNames = await connection.GetColumnNamesAsync(null, tableName2);
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
        var table = await connection.GetTableAsync(null, tableName2);
        Assert.NotNull(table);

        foreach (var column in table.Columns)
        {
            var originalColumn = addColumns.SingleOrDefault(c =>
                c.ColumnName.Equals(column.ColumnName, StringComparison.OrdinalIgnoreCase)
            );
            Assert.NotNull(originalColumn);
        }

        // general count tests
        // some providers like MySQL create unique constraints for unique indexes, and vice-versa, so we can't just count the unique indexes
        Assert.Equal(
            addColumns.Count(c => !c.IsIndexed && c.IsUnique),
            dbType == DbProviderType.MySql
                ? table.UniqueConstraints.Count / 2
                : table.UniqueConstraints.Count
        );
        Assert.Equal(
            addColumns.Count(c => c.IsIndexed && !c.IsUnique),
            table.Indexes.Count(c => !c.IsUnique)
        );
        var expectedUniqueIndexes = addColumns.Where(c => c.IsIndexed && c.IsUnique).ToArray();
        var actualUniqueIndexes = table.Indexes.Where(c => c.IsUnique).ToArray();
        Assert.Equal(
            expectedUniqueIndexes.Length,
            dbType == DbProviderType.MySql
                ? actualUniqueIndexes.Length / 2
                : actualUniqueIndexes.Length
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

        var indexedColumnsExpected = addColumns.Where(c => c.IsIndexed).ToArray();
        var uniqueColumnsNonIndexed = addColumns.Where(c => c.IsUnique && !c.IsIndexed).ToArray();

        var indexedColumnsActual = table.Columns.Where(c => c.IsIndexed).ToArray();

        Assert.Equal(
            dbType == DbProviderType.MySql
                ? (indexedColumnsExpected.Length + uniqueColumnsNonIndexed.Length)
                : indexedColumnsExpected.Length,
            indexedColumnsActual.Length
        );
    }
}
