using System.Data;
using Dapper;
using DapperMatic.Models;
using Microsoft.VisualBasic;
using Xunit.Abstractions;

namespace DapperMatic.Tests;

public abstract class DatabaseTests
{
    private readonly ITestOutputHelper output;

    protected DatabaseTests(ITestOutputHelper output)
    {
        Console.WriteLine($"Initializing tests for {GetType().Name}");
        output.WriteLine($"Initializing tests for {GetType().Name}");
        this.output = output;
    }

    public abstract Task<IDbConnection> OpenConnectionAsync();

    [Fact]
    protected virtual async Task Database_Can_RunArbitraryQueriesAsync()
    {
        using var connection = await OpenConnectionAsync();
        const int expected = 1;
        var actual = await connection.QueryFirstAsync<int>("SELECT 1");
        Assert.Equal(expected, actual);

        // run a statement with many sql statements at the same time
        await connection.ExecuteAsync(
            @"
            CREATE TABLE test (id INT PRIMARY KEY);
            INSERT INTO test VALUES (1);
            INSERT INTO test VALUES (2);
            INSERT INTO test VALUES (3);
            "
        );
        var values = await connection.QueryAsync<int>("SELECT id FROM test");
        Assert.Equal(3, values.Count());

        // run multiple select statements and read multiple result sets
        var result = await connection.QueryMultipleAsync(
            @"
            SELECT id FROM test WHERE id = 1;
            SELECT id FROM test WHERE id = 2;
            SELECT id FROM test;
            -- this statement is ignored by the grid reader
            -- because it doesn't return any results
            INSERT INTO test VALUES (4);
            SELECT id FROM test WHERE id = 4;
            "
        );
        var id1 = result.Read<int>().Single();
        var id2 = result.Read<int>().Single();
        var allIds = result.Read<int>().ToArray();
        var id4 = result.Read<int>().Single();
        Assert.Equal(1, id1);
        Assert.Equal(2, id2);
        Assert.Equal(3, allIds.Length);
        Assert.Equal(4, id4);
    }

    [Fact]
    protected virtual async Task Database_Can_CrudSchemasAsync()
    {
        using var connection = await OpenConnectionAsync();

        var supportsSchemas = await connection.SupportsSchemasAsync();

        const string schemaName = "test";

        // providers should just ignore this if the database doesn't support schemas
        await connection.DropSchemaIfExistsAsync(schemaName);

        output.WriteLine($"Schema Exists: {schemaName}");
        var exists = await connection.SchemaExistsAsync(schemaName);
        Assert.False(exists);

        output.WriteLine($"Creating schemaName: {schemaName}");
        var created = await connection.CreateSchemaIfNotExistsAsync(schemaName);
        if (supportsSchemas)
        {
            Assert.True(created);
        }
        else
        {
            Assert.False(created);
        }

        output.WriteLine($"Retrieving schemas");
        var schemas = (await connection.GetSchemasAsync()).ToArray();
        if (supportsSchemas)
        {
            Assert.True(schemas.Length > 0 && schemas.Contains(schemaName));
        }
        else
        {
            Assert.Empty(schemas);
        }

        schemas = (await connection.GetSchemasAsync(schemaName)).ToArray();
        if (supportsSchemas)
        {
            Assert.Single(schemas);
            Assert.Equal(schemaName, schemas.Single());
        }
        else
        {
            Assert.Empty(schemas);
        }

        output.WriteLine($"Dropping schemaName: {schemaName}");
        var dropped = await connection.DropSchemaIfExistsAsync(schemaName);
        if (supportsSchemas)
        {
            Assert.True(dropped);
        }
        else
        {
            Assert.False(dropped);
        }

        schemas = (await connection.GetSchemasAsync(schemaName)).ToArray();
        Assert.Empty(schemas);
    }

    [Fact]
    protected virtual async Task Database_Can_CrudTablesWithoutSchemasAsync()
    {
        using IDbConnection connection = await OpenConnectionAsync();
        const string tableName = "test";

        await connection.DropTableIfExistsAsync(tableName);

        output.WriteLine($"Table Exists: {tableName}");
        var exists = await connection.TableExistsAsync(tableName);
        Assert.False(exists);

        output.WriteLine($"Creating table: {tableName}");
        await connection.CreateTableIfNotExistsAsync(tableName);

        output.WriteLine($"Retrieving tables");
        var tables = (await connection.GetTablesAsync()).ToArray();
        Assert.True(tables.Length > 0 && tables.Contains(tableName));

        tables = (await connection.GetTablesAsync(tableName)).ToArray();
        Assert.Single(tables);
        Assert.Equal(tableName, tables.Single());

        output.WriteLine("Testing auto increment");
        for (var i = 0; i < 10; i++)
        {
            await connection.ExecuteAsync($"INSERT INTO {tableName} DEFAULT VALUES");
        }
        var count = await connection.ExecuteScalarAsync<int>($"SELECT COUNT(*) FROM {tableName}");
        Assert.Equal(10, count);

        output.WriteLine($"Dropping table: {tableName}");
        await connection.DropTableIfExistsAsync(tableName);

        const string columnIdName = "id";

        output.WriteLine($"Column Exists: {tableName}.{columnIdName}");
        await connection.ColumnExistsAsync(tableName, columnIdName);

        output.WriteLine($"Creating table with Guid PK: tableWithGuidPk");
        await connection.CreateTableIfNotExistsAsync(
            "tableWithGuidPk",
            primaryKeyColumnNames: new[] { "guidId" },
            primaryKeyDotnetTypes: new[] { typeof(Guid) }
        );
        exists = await connection.TableExistsAsync("tableWithGuidPk");
        Assert.True(exists);

        output.WriteLine($"Creating table with string PK: tableWithStringPk");
        await connection.CreateTableIfNotExistsAsync(
            "tableWithStringPk",
            primaryKeyColumnNames: new[] { "strId" },
            primaryKeyDotnetTypes: new[] { typeof(string) }
        );
        exists = await connection.TableExistsAsync("tableWithStringPk");
        Assert.True(exists);

        output.WriteLine($"Creating table with string PK 64 length: tableWithStringPk64");
        await connection.CreateTableIfNotExistsAsync(
            "tableWithStringPk64",
            primaryKeyColumnNames: new[] { "strId64" },
            primaryKeyDotnetTypes: new[] { typeof(string) },
            primaryKeyColumnLengths: new[] { (int?)64 }
        );
        exists = await connection.TableExistsAsync("tableWithStringPk64");
        Assert.True(exists);

        output.WriteLine($"Creating table with compound PK: tableWithCompoundPk");
        await connection.CreateTableIfNotExistsAsync(
            "tableWithCompoundPk",
            primaryKeyColumnNames: new[] { "longId", "guidId", "strId" },
            primaryKeyDotnetTypes: new[] { typeof(long), typeof(Guid), typeof(string) },
            primaryKeyColumnLengths: new int?[] { null, null, 128 }
        );
        exists = await connection.TableExistsAsync("tableWithCompoundPk");
        Assert.True(exists);
    }

    [Fact]
    protected virtual async Task Database_Can_CrudTableColumnsAsync()
    {
        using IDbConnection connection = await OpenConnectionAsync();
        const string tableName = "testWithColumn";
        const string columnName = "testColumn";

        string? defaultDateTimeSql = null;
        string? defaultGuidSql = null;
        var dbType = connection.GetDatabaseType();
        switch (dbType)
        {
            case DatabaseTypes.SqlServer:
                defaultDateTimeSql = "GETUTCDATE()";
                defaultGuidSql = "NEWID()";
                break;
            case DatabaseTypes.Sqlite:
                defaultDateTimeSql = "CURRENT_TIMESTAMP";
                //this could be supported IF the sqlite UUID extension was loaded and enabled
                //defaultGuidSql = "uuid_blob(uuid())";
                defaultGuidSql = null;
                break;
            case DatabaseTypes.PostgreSql:
                defaultDateTimeSql = "CURRENT_TIMESTAMP";
                defaultGuidSql = "uuid_generate_v4()";
                break;
            case DatabaseTypes.MySql:
                defaultDateTimeSql = "CURRENT_TIMESTAMP";
                // only supported after 8.0.13
                // defaultGuidSql = "UUID()";
                break;
        }

        await connection.CreateTableIfNotExistsAsync(tableName);

        output.WriteLine($"Column Exists: {tableName}.{columnName}");
        var exists = await connection.ColumnExistsAsync(tableName, columnName);
        Assert.False(exists);

        output.WriteLine($"Creating columnName: {tableName}.{columnName}");
        await connection.CreateColumnIfNotExistsAsync(
            tableName,
            columnName,
            typeof(int),
            defaultValue: "1",
            nullable: false
        );

        output.WriteLine($"Column Exists: {tableName}.{columnName}");
        exists = await connection.ColumnExistsAsync(tableName, columnName);
        Assert.True(exists);

        output.WriteLine($"Dropping columnName: {tableName}.{columnName}");
        await connection.DropColumnIfExistsAsync(tableName, columnName);

        output.WriteLine($"Column Exists: {tableName}.{columnName}");
        exists = await connection.ColumnExistsAsync(tableName, columnName);
        Assert.False(exists);

        // try adding a columnName of all the supported types
        await connection.CreateTableIfNotExistsAsync("testWithAllColumns");
        var columnCount = 1;
        await connection.CreateColumnIfNotExistsAsync(
            "testWithAllColumns",
            "createdDateColumn" + columnCount++,
            typeof(DateTime),
            defaultValue: defaultDateTimeSql
        );
        await connection.CreateColumnIfNotExistsAsync(
            "testWithAllColumns",
            "newidColumn" + columnCount++,
            typeof(Guid),
            defaultValue: defaultGuidSql
        );
        await connection.CreateColumnIfNotExistsAsync(
            "testWithAllColumns",
            "bigintColumn" + columnCount++,
            typeof(long)
        );
        await connection.CreateColumnIfNotExistsAsync(
            "testWithAllColumns",
            "binaryColumn" + columnCount++,
            typeof(byte[])
        );
        await connection.CreateColumnIfNotExistsAsync(
            "testWithAllColumns",
            "bitColumn" + columnCount++,
            typeof(bool)
        );
        await connection.CreateColumnIfNotExistsAsync(
            "testWithAllColumns",
            "charColumn" + columnCount++,
            typeof(string),
            length: 10
        );
        await connection.CreateColumnIfNotExistsAsync(
            "testWithAllColumns",
            "dateColumn" + columnCount++,
            typeof(DateTime)
        );
        await connection.CreateColumnIfNotExistsAsync(
            "testWithAllColumns",
            "datetimeColumn" + columnCount++,
            typeof(DateTime)
        );
        await connection.CreateColumnIfNotExistsAsync(
            "testWithAllColumns",
            "datetime2Column" + columnCount++,
            typeof(DateTime)
        );
        await connection.CreateColumnIfNotExistsAsync(
            "testWithAllColumns",
            "datetimeoffsetColumn" + columnCount++,
            typeof(DateTimeOffset)
        );
        await connection.CreateColumnIfNotExistsAsync(
            "testWithAllColumns",
            "decimalColumn" + columnCount++,
            typeof(decimal)
        );
        await connection.CreateColumnIfNotExistsAsync(
            "testWithAllColumns",
            "decimalColumnWithPrecision" + columnCount++,
            typeof(decimal),
            precision: 10
        );
        await connection.CreateColumnIfNotExistsAsync(
            "testWithAllColumns",
            "decimalColumnWithPrecisionAndScale" + columnCount++,
            typeof(decimal),
            precision: 10,
            scale: 5
        );
        await connection.CreateColumnIfNotExistsAsync(
            "testWithAllColumns",
            "floatColumn" + columnCount++,
            typeof(double)
        );
        await connection.CreateColumnIfNotExistsAsync(
            "testWithAllColumns",
            "imageColumn" + columnCount++,
            typeof(byte[])
        );
        await connection.CreateColumnIfNotExistsAsync(
            "testWithAllColumns",
            "intColumn" + columnCount++,
            typeof(int)
        );
        await connection.CreateColumnIfNotExistsAsync(
            "testWithAllColumns",
            "moneyColumn" + columnCount++,
            typeof(decimal)
        );
        await connection.CreateColumnIfNotExistsAsync(
            "testWithAllColumns",
            "ncharColumn" + columnCount++,
            typeof(string),
            length: 10
        );
        await connection.CreateColumnIfNotExistsAsync(
            "testWithAllColumns",
            "ntextColumn" + columnCount++,
            typeof(string),
            length: int.MaxValue
        );
        await connection.CreateColumnIfNotExistsAsync(
            "testWithAllColumns",
            "floatColumn2" + columnCount++,
            typeof(float)
        );
        await connection.CreateColumnIfNotExistsAsync(
            "testWithAllColumns",
            "doubleColumn2" + columnCount++,
            typeof(double)
        );
        await connection.CreateColumnIfNotExistsAsync(
            "testWithAllColumns",
            "guidArrayColumn" + columnCount++,
            typeof(Guid[])
        );
        await connection.CreateColumnIfNotExistsAsync(
            "testWithAllColumns",
            "intArrayColumn" + columnCount++,
            typeof(int[])
        );
        await connection.CreateColumnIfNotExistsAsync(
            "testWithAllColumns",
            "longArrayColumn" + columnCount++,
            typeof(long[])
        );
        await connection.CreateColumnIfNotExistsAsync(
            "testWithAllColumns",
            "doubleArrayColumn" + columnCount++,
            typeof(double[])
        );
        await connection.CreateColumnIfNotExistsAsync(
            "testWithAllColumns",
            "decimalArrayColumn" + columnCount++,
            typeof(decimal[])
        );
        await connection.CreateColumnIfNotExistsAsync(
            "testWithAllColumns",
            "stringArrayColumn" + columnCount++,
            typeof(string[])
        );
        await connection.CreateColumnIfNotExistsAsync(
            "testWithAllColumns",
            "stringDectionaryArrayColumn" + columnCount++,
            typeof(Dictionary<string, string>)
        );
        await connection.CreateColumnIfNotExistsAsync(
            "testWithAllColumns",
            "objectDectionaryArrayColumn" + columnCount++,
            typeof(Dictionary<string, object>)
        );

        var columnNames = await connection.GetColumnsAsync("testWithAllColumns");
        Assert.Equal(columnCount, columnNames.Count());
    }

    [Fact]
    protected virtual async Task Database_Can_CrudTableIndexesAsync()
    {
        using IDbConnection connection = await OpenConnectionAsync();
        try
        {
            // await connection.ExecuteAsync("DROP TABLE testWithIndex");
            const string tableName = "testWithIndex";
            const string columnName = "testColumn";
            const string indexName = "testIndex";

            await connection.DropTableIfExistsAsync(tableName);
            await connection.CreateTableIfNotExistsAsync(tableName);
            await connection.CreateColumnIfNotExistsAsync(
                tableName,
                columnName,
                typeof(int),
                defaultValue: "1",
                nullable: false
            );
            for (var i = 0; i < 10; i++)
            {
                await connection.CreateColumnIfNotExistsAsync(
                    tableName,
                    columnName + "_" + i,
                    typeof(int),
                    defaultValue: i.ToString(),
                    nullable: false
                );
            }

            output.WriteLine($"Index Exists: {tableName}.{indexName}");
            var exists = await connection.IndexExistsAsync(tableName, columnName, indexName);
            Assert.False(exists);

            output.WriteLine($"Creating unique index: {tableName}.{indexName}");
            await connection.CreateIndexIfNotExistsAsync(
                tableName,
                indexName,
                [columnName],
                unique: true
            );

            output.WriteLine(
                $"Creating multiple column unique index: {tableName}.{indexName}_multi"
            );
            await connection.CreateIndexIfNotExistsAsync(
                tableName,
                indexName + "_multi",
                [columnName + "_1 DESC", columnName + "_2"],
                unique: true
            );

            output.WriteLine(
                $"Creating multiple column non unique index: {tableName}.{indexName}_multi2"
            );
            await connection.CreateIndexIfNotExistsAsync(
                tableName,
                indexName + "_multi2",
                [columnName + "_3 ASC", columnName + "_4 DESC"]
            );

            output.WriteLine($"Index Exists: {tableName}.{indexName}");
            exists = await connection.IndexExistsAsync(tableName, indexName);
            Assert.True(exists);
            exists = await connection.IndexExistsAsync(tableName, indexName + "_multi");
            Assert.True(exists);
            exists = await connection.IndexExistsAsync(tableName, indexName + "_multi2");
            Assert.True(exists);

            var indexNames = await connection.GetIndexNamesAsync(tableName);
            // get all indexes in the database
            var indexNames2 = await connection.GetIndexNamesAsync(null);
            Assert.Contains(
                indexNames,
                i => i.Equals(indexName, StringComparison.OrdinalIgnoreCase)
            );
            Assert.Contains(
                indexNames2,
                i => i.Equals(indexName, StringComparison.OrdinalIgnoreCase)
            );
            Assert.Contains(
                indexNames,
                i => i.Equals(indexName + "_multi", StringComparison.OrdinalIgnoreCase)
            );
            Assert.Contains(
                indexNames2,
                i => i.Equals(indexName + "_multi", StringComparison.OrdinalIgnoreCase)
            );
            Assert.Contains(
                indexNames,
                i => i.Equals(indexName + "_multi2", StringComparison.OrdinalIgnoreCase)
            );
            Assert.Contains(
                indexNames2,
                i => i.Equals(indexName + "_multi2", StringComparison.OrdinalIgnoreCase)
            );

            var indexes = await connection.GetIndexesAsync(tableName);
            // get all indexes in the database
            var indexes2 = await connection.GetIndexesAsync(null);
            Assert.True(indexes.Count() >= 3);
            Assert.True(indexes2.Count() >= 3);
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
            idxMulti1 = indexes2.SingleOrDefault(i =>
                i.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase)
                && i.IndexName.Equals(indexName + "_multi", StringComparison.OrdinalIgnoreCase)
            );
            idxMulti2 = indexes2.SingleOrDefault(i =>
                i.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase)
                && i.IndexName.Equals(indexName + "_multi2", StringComparison.OrdinalIgnoreCase)
            );
            Assert.NotNull(idxMulti1);
            Assert.NotNull(idxMulti2);
            Assert.True(idxMulti1.Unique);
            Assert.True(idxMulti1.ColumnNames.Length == 2);
            Assert.EndsWith("desc", idxMulti1.ColumnNames[0], StringComparison.OrdinalIgnoreCase);
            Assert.EndsWith("asc", idxMulti1.ColumnNames[1], StringComparison.OrdinalIgnoreCase);
            Assert.False(idxMulti2.Unique);
            Assert.True(idxMulti2.ColumnNames.Length == 2);
            Assert.EndsWith("asc", idxMulti2.ColumnNames[0], StringComparison.OrdinalIgnoreCase);
            Assert.EndsWith("desc", idxMulti2.ColumnNames[1], StringComparison.OrdinalIgnoreCase);

            output.WriteLine($"Dropping indexName: {tableName}.{indexName}");
            await connection.DropIndexIfExistsAsync(tableName, indexName);

            output.WriteLine($"Index Exists: {tableName}.{indexName}");
            exists = await connection.IndexExistsAsync(tableName, indexName);
            Assert.False(exists);

            await connection.DropTableIfExistsAsync(tableName);
        }
        finally
        {
            var sql = connection.GetLastSql();
            output.WriteLine("Last sql: " + sql);
        }
    }

    [Fact]
    protected virtual async Task Database_Can_CrudTableForeignKeysAsync()
    {
        using IDbConnection connection = await OpenConnectionAsync();
        const string tableName = "testWithFk";
        const string refTableName = "testPk";
        const string columnName = "testFkColumn";
        const string foreignKeyName = "testFk";

        await connection.CreateTableIfNotExistsAsync(tableName);
        await connection.CreateTableIfNotExistsAsync(refTableName);
        await connection.CreateColumnIfNotExistsAsync(
            tableName,
            columnName,
            typeof(int),
            defaultValue: "1",
            nullable: false
        );

        output.WriteLine($"Foreign Key Exists: {tableName}.{foreignKeyName}");
        var exists = await connection.ForeignKeyExistsAsync(tableName, columnName, foreignKeyName);
        Assert.False(exists);

        output.WriteLine($"Creating foreign key: {tableName}.{foreignKeyName}");
        await connection.CreateForeignKeyIfNotExistsAsync(
            tableName,
            columnName,
            foreignKeyName,
            refTableName,
            "id",
            onDelete: ReferentialAction.Cascade.ToSql()
        );

        output.WriteLine($"Foreign Key Exists: {tableName}.{foreignKeyName}");
        exists = await connection.ForeignKeyExistsAsync(tableName, columnName, foreignKeyName);
        Assert.True(exists);

        output.WriteLine($"Get Foreign Keys: {tableName}");
        var fks = await connection.GetForeignKeysAsync(tableName);
        if (await connection.SupportsNamedForeignKeysAsync())
        {
            Assert.Contains(
                fks,
                fk => fk.Equals(foreignKeyName, StringComparison.OrdinalIgnoreCase)
            );
        }

        output.WriteLine($"Dropping foreign key: {tableName}.{foreignKeyName}");
        await connection.DropForeignKeyIfExistsAsync(tableName, columnName, foreignKeyName);

        output.WriteLine($"Foreign Key Exists: {tableName}.{foreignKeyName}");
        exists = await connection.ForeignKeyExistsAsync(tableName, columnName, foreignKeyName);
        Assert.False(exists);
    }

    [Fact]
    protected virtual async Task Database_Can_CrudTableUniqueConstraintsAsync()
    {
        using IDbConnection connection = await OpenConnectionAsync();
        const string tableName = "testWithUc";
        const string columnName = "testColumn";
        const string uniqueConstraintName = "testUc";

        await connection.CreateTableIfNotExistsAsync(tableName);
        await connection.CreateColumnIfNotExistsAsync(
            tableName,
            columnName,
            typeof(int),
            defaultValue: "1",
            nullable: false
        );

        output.WriteLine($"Unique Constraint Exists: {tableName}.{uniqueConstraintName}");
        var exists = await connection.UniqueConstraintExistsAsync(
            tableName,
            columnName,
            uniqueConstraintName
        );
        Assert.False(exists);

        output.WriteLine($"Creating unique constraint: {tableName}.{uniqueConstraintName}");
        await connection.CreateUniqueConstraintIfNotExistsAsync(
            tableName,
            uniqueConstraintName,
            [columnName]
        );

        output.WriteLine($"Unique Constraint Exists: {tableName}.{uniqueConstraintName}");
        exists = await connection.UniqueConstraintExistsAsync(tableName, uniqueConstraintName);
        Assert.True(exists);

        output.WriteLine($"Dropping unique constraint: {tableName}.{uniqueConstraintName}");
        await connection.DropUniqueConstraintIfExistsAsync(tableName, uniqueConstraintName);

        output.WriteLine($"Unique Constraint Exists: {tableName}.{uniqueConstraintName}");
        exists = await connection.UniqueConstraintExistsAsync(tableName, uniqueConstraintName);
        Assert.False(exists);
    }

    public virtual void Dispose() => output.WriteLine(GetType().Name);
}
