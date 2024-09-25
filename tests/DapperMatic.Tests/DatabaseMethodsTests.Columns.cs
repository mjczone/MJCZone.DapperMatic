using DapperMatic.Models;

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

        output.WriteLine($"Column Exists: {tableName}.{columnName}");
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

        output.WriteLine($"Column Exists: {tableName}.{columnName}");
        exists = await connection.DoesColumnExistAsync(null, tableName, columnName);
        Assert.True(exists);

        output.WriteLine($"Dropping columnName: {tableName}.{columnName}");
        await connection.DropColumnIfExistsAsync(null, tableName, columnName);

        output.WriteLine($"Column Exists: {tableName}.{columnName}");
        exists = await connection.DoesColumnExistAsync(null, tableName, columnName);
        Assert.False(exists);

        // try adding a columnName of all the supported types
        await connection.CreateTableIfNotExistsAsync(
            null,
            "testWithAllColumns",
            [new DxColumn(null, "testWithAllColumns", "id", typeof(int), isPrimaryKey: true)]
        );
        var columnCount = 1;
        await connection.CreateColumnIfNotExistsAsync(
            null,
            "testWithAllColumns",
            "createdDateColumn" + columnCount++,
            typeof(DateTime),
            defaultExpression: defaultDateTimeSql
        );
        await connection.CreateColumnIfNotExistsAsync(
            null,
            "testWithAllColumns",
            "newidColumn" + columnCount++,
            typeof(Guid),
            defaultExpression: defaultGuidSql
        );
        await connection.CreateColumnIfNotExistsAsync(
            null,
            "testWithAllColumns",
            "bigintColumn" + columnCount++,
            typeof(long)
        );
        await connection.CreateColumnIfNotExistsAsync(
            null,
            "testWithAllColumns",
            "binaryColumn" + columnCount++,
            typeof(byte[])
        );
        await connection.CreateColumnIfNotExistsAsync(
            null,
            "testWithAllColumns",
            "bitColumn" + columnCount++,
            typeof(bool)
        );
        await connection.CreateColumnIfNotExistsAsync(
            null,
            "testWithAllColumns",
            "charColumn" + columnCount++,
            typeof(string),
            length: 10
        );
        await connection.CreateColumnIfNotExistsAsync(
            null,
            "testWithAllColumns",
            "dateColumn" + columnCount++,
            typeof(DateTime)
        );
        await connection.CreateColumnIfNotExistsAsync(
            null,
            "testWithAllColumns",
            "datetimeColumn" + columnCount++,
            typeof(DateTime)
        );
        await connection.CreateColumnIfNotExistsAsync(
            null,
            "testWithAllColumns",
            "datetime2Column" + columnCount++,
            typeof(DateTime)
        );
        await connection.CreateColumnIfNotExistsAsync(
            null,
            "testWithAllColumns",
            "datetimeoffsetColumn" + columnCount++,
            typeof(DateTimeOffset)
        );
        await connection.CreateColumnIfNotExistsAsync(
            null,
            "testWithAllColumns",
            "decimalColumn" + columnCount++,
            typeof(decimal)
        );
        await connection.CreateColumnIfNotExistsAsync(
            null,
            "testWithAllColumns",
            "decimalColumnWithPrecision" + columnCount++,
            typeof(decimal),
            precision: 10
        );
        await connection.CreateColumnIfNotExistsAsync(
            null,
            "testWithAllColumns",
            "decimalColumnWithPrecisionAndScale" + columnCount++,
            typeof(decimal),
            precision: 10,
            scale: 5
        );
        await connection.CreateColumnIfNotExistsAsync(
            null,
            "testWithAllColumns",
            "floatColumn" + columnCount++,
            typeof(double)
        );
        await connection.CreateColumnIfNotExistsAsync(
            null,
            "testWithAllColumns",
            "imageColumn" + columnCount++,
            typeof(byte[])
        );
        await connection.CreateColumnIfNotExistsAsync(
            null,
            "testWithAllColumns",
            "intColumn" + columnCount++,
            typeof(int)
        );
        await connection.CreateColumnIfNotExistsAsync(
            null,
            "testWithAllColumns",
            "moneyColumn" + columnCount++,
            typeof(decimal)
        );
        await connection.CreateColumnIfNotExistsAsync(
            null,
            "testWithAllColumns",
            "ncharColumn" + columnCount++,
            typeof(string),
            length: 10
        );
        await connection.CreateColumnIfNotExistsAsync(
            null,
            "testWithAllColumns",
            "ntextColumn" + columnCount++,
            typeof(string),
            length: int.MaxValue
        );
        await connection.CreateColumnIfNotExistsAsync(
            null,
            "testWithAllColumns",
            "floatColumn2" + columnCount++,
            typeof(float)
        );
        await connection.CreateColumnIfNotExistsAsync(
            null,
            "testWithAllColumns",
            "doubleColumn2" + columnCount++,
            typeof(double)
        );
        await connection.CreateColumnIfNotExistsAsync(
            null,
            "testWithAllColumns",
            "guidArrayColumn" + columnCount++,
            typeof(Guid[])
        );
        await connection.CreateColumnIfNotExistsAsync(
            null,
            "testWithAllColumns",
            "intArrayColumn" + columnCount++,
            typeof(int[])
        );
        await connection.CreateColumnIfNotExistsAsync(
            null,
            "testWithAllColumns",
            "longArrayColumn" + columnCount++,
            typeof(long[])
        );
        await connection.CreateColumnIfNotExistsAsync(
            null,
            "testWithAllColumns",
            "doubleArrayColumn" + columnCount++,
            typeof(double[])
        );
        await connection.CreateColumnIfNotExistsAsync(
            null,
            "testWithAllColumns",
            "decimalArrayColumn" + columnCount++,
            typeof(decimal[])
        );
        await connection.CreateColumnIfNotExistsAsync(
            null,
            "testWithAllColumns",
            "stringArrayColumn" + columnCount++,
            typeof(string[])
        );
        await connection.CreateColumnIfNotExistsAsync(
            null,
            "testWithAllColumns",
            "stringDectionaryArrayColumn" + columnCount++,
            typeof(Dictionary<string, string>)
        );
        await connection.CreateColumnIfNotExistsAsync(
            null,
            "testWithAllColumns",
            "objectDectionaryArrayColumn" + columnCount++,
            typeof(Dictionary<string, object>)
        );

        var columnNames = await connection.GetColumnNamesAsync(null, "testWithAllColumns");
        Assert.Equal(columnCount, columnNames.Count());
    }
}
