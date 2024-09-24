using DapperMatic.Models;

namespace DapperMatic.Tests;

public abstract partial class DatabaseMethodsTests
{
    [Fact]
    protected virtual async Task Can_perform_simple_CRUD_on_Indexes_Async()
    {
        using var connection = await OpenConnectionAsync();

        var version = await connection.GetDatabaseVersionAsync();
        Assert.NotEmpty(version);

        var supportsDescendingColumnSorts = true;
        var dbType = connection.GetDbProviderType();
        if (dbType.HasFlag(DbProviderType.MySql))
        {
            if (version.StartsWith("5."))
            {
                supportsDescendingColumnSorts = false;
            }
        }
        try
        {
            const string tableName = "testWithIndex";
            const string columnName = "testColumn";
            const string indexName = "testIndex";

            var columns = new List<DxColumn>
            {
                new DxColumn(
                    null,
                    tableName,
                    columnName,
                    typeof(int),
                    defaultExpression: "1",
                    isNullable: false
                )
            };
            for (var i = 0; i < 10; i++)
            {
                columns.Add(
                    new DxColumn(
                        null,
                        tableName,
                        columnName + "_" + i,
                        typeof(int),
                        defaultExpression: i.ToString(),
                        isNullable: false
                    )
                );
            }

            await connection.DropTableIfExistsAsync(null, tableName);
            await connection.CreateTableIfNotExistsAsync(null, tableName, columns: [.. columns]);

            output.WriteLine($"Index Exists: {tableName}.{indexName}");
            var exists = await connection.IndexExistsAsync(null, tableName, indexName);
            Assert.False(exists);

            output.WriteLine($"Creating unique index: {tableName}.{indexName}");
            await connection.CreateIndexIfNotExistsAsync(
                null,
                tableName,
                indexName,
                [new DxOrderedColumn(columnName)],
                isUnique: true
            );

            output.WriteLine(
                $"Creating multiple column unique index: {tableName}.{indexName}_multi"
            );
            await connection.CreateIndexIfNotExistsAsync(
                null,
                tableName,
                indexName + "_multi",
                [
                    new DxOrderedColumn(columnName + "_1", DxColumnOrder.Descending),
                    new DxOrderedColumn(columnName + "_2")
                ],
                isUnique: true
            );

            output.WriteLine(
                $"Creating multiple column non unique index: {tableName}.{indexName}_multi2"
            );
            await connection.CreateIndexIfNotExistsAsync(
                null,
                tableName,
                indexName + "_multi2",
                [
                    new DxOrderedColumn(columnName + "_3", DxColumnOrder.Ascending),
                    new DxOrderedColumn(columnName + "_4", DxColumnOrder.Descending)
                ]
            );

            output.WriteLine($"Index Exists: {tableName}.{indexName}");
            exists = await connection.IndexExistsAsync(null, tableName, indexName);
            Assert.True(exists);
            exists = await connection.IndexExistsAsync(null, tableName, indexName + "_multi");
            Assert.True(exists);
            exists = await connection.IndexExistsAsync(null, tableName, indexName + "_multi2");
            Assert.True(exists);

            var indexNames = await connection.GetIndexNamesAsync(null, tableName);
            Assert.Contains(
                indexNames,
                i => i.Equals(indexName, StringComparison.OrdinalIgnoreCase)
            );
            Assert.Contains(
                indexNames,
                i => i.Equals(indexName + "_multi", StringComparison.OrdinalIgnoreCase)
            );
            Assert.Contains(
                indexNames,
                i => i.Equals(indexName + "_multi2", StringComparison.OrdinalIgnoreCase)
            );

            var indexes = await connection.GetIndexesAsync(null, tableName);
            Assert.True(indexes.Count() >= 3);
            var idxMulti1 = indexes.SingleOrDefault(i =>
                i.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase)
                && i.IndexName.Equals(indexName + "_multi", StringComparison.OrdinalIgnoreCase)
            );
            var idxMulti2 = indexes.SingleOrDefault(i =>
                i.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase)
                && i.IndexName.Equals(indexName + "_multi2", StringComparison.OrdinalIgnoreCase)
            );
            Assert.NotNull(idxMulti1);
            Assert.NotNull(idxMulti2);
            Assert.NotNull(idxMulti1);
            Assert.NotNull(idxMulti2);
            Assert.True(idxMulti1.IsUnique);
            Assert.True(idxMulti1.Columns.Length == 2);
            if (supportsDescendingColumnSorts)
            {
                Assert.Equal(DxColumnOrder.Descending, idxMulti1.Columns[0].Order);
                Assert.Equal(DxColumnOrder.Ascending, idxMulti1.Columns[1].Order);
            }
            Assert.False(idxMulti2.IsUnique);
            Assert.True(idxMulti2.Columns.Length == 2);
            Assert.Equal(DxColumnOrder.Ascending, idxMulti2.Columns[0].Order);
            if (supportsDescendingColumnSorts)
            {
                Assert.Equal(DxColumnOrder.Descending, idxMulti2.Columns[1].Order);
            }

            var indexesOnColumn = await connection.GetIndexesOnColumnAsync(
                null,
                tableName,
                columnName
            );
            Assert.NotEmpty(indexesOnColumn);

            output.WriteLine($"Dropping indexName: {tableName}.{indexName}");
            await connection.DropIndexIfExistsAsync(null, tableName, indexName);

            output.WriteLine($"Index Exists: {tableName}.{indexName}");
            exists = await connection.IndexExistsAsync(null, tableName, indexName);
            Assert.False(exists);

            await connection.DropTableIfExistsAsync(null, tableName);
        }
        finally
        {
            var sql = connection.GetLastSql();
            output.WriteLine("Last sql: " + sql);
        }
    }
}
