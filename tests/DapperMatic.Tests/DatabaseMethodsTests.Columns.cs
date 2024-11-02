using DapperMatic.Models;
using DapperMatic.Providers;

namespace DapperMatic.Tests;

public abstract partial class DatabaseMethodsTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("my_app")]
    protected virtual async Task Can_set_common_default_expressions_on_Columns_Async(
        string? schemaName
    )
    {
        using var db = await OpenConnectionAsync();
        await InitFreshSchemaAsync(db, schemaName);

        const string tableName = "testTableWithExpressions";
        const string columnName1 = "testColumnWithDefaultDate";
        const string columnName2 = "testColumnWithDefaultDateSetAfterCreate";
        const string columnName3 = "testColumnWithDefaultUuid";
        const string columnName4 = "testColumnWithDefaultShort";
        const string columnName5 = "testColumnWithDefaultBool";

        string? defaultDateTimeSql = null;
        string? defaultGuidSql = null;
        var dbType = db.GetDbProviderType();

        var version = await db.GetDatabaseVersionAsync();

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
                defaultDateTimeSql = version > new Version(5, 6, 5) ? "CURRENT_TIMESTAMP" : null;
                // only supported after 8.0.13
                // LEADS TO THIS ERROR:
                // Statement is unsafe because it uses a system function that may return a different value on the replication slave.
                // MySQL isn't a good database for auto-generating GUIDs. Don't do it!
                // defaultGuidSql =
                //     version > new Version(8, 0, 13) && version < new Version(10, 0, 0)
                //         ? "(UUID())"
                //         : null;
                defaultGuidSql = null;
                break;
        }

        // Create table with a column with an expression
        await db.CreateTableIfNotExistsAsync(
            schemaName,
            tableName,
            [
                new DxColumn(
                    schemaName,
                    tableName,
                    "id",
                    typeof(int),
                    isPrimaryKey: true,
                    isAutoIncrement: true
                ),
                new DxColumn(
                    schemaName,
                    tableName,
                    columnName1,
                    typeof(DateTime),
                    defaultExpression: defaultDateTimeSql
                )
            ]
        );

        // Add a column with a default expression after the table is created
        await db.CreateColumnIfNotExistsAsync(
            new DxColumn(
                schemaName,
                tableName,
                columnName2,
                typeof(DateTime),
                defaultExpression: defaultDateTimeSql
            )
        );

        if (defaultGuidSql != null)
        {
            // Add a column with a default expression after the table is created
            await db.CreateColumnIfNotExistsAsync(
                new DxColumn(
                    schemaName,
                    tableName,
                    columnName3,
                    typeof(Guid),
                    defaultExpression: defaultGuidSql
                )
            );
        }

        // Add a column with a default expression after the table is created
        await db.CreateColumnIfNotExistsAsync(
            new DxColumn(schemaName, tableName, columnName4, typeof(short), defaultExpression: "4")
        );
        await db.CreateColumnIfNotExistsAsync(
            new DxColumn(schemaName, tableName, columnName5, typeof(bool), defaultExpression: "1")
        );

        // Now check to make sure the default expressions are set
        var table = await db.GetTableAsync(schemaName, tableName);
        var columns = await db.GetColumnsAsync(schemaName, tableName);
        var column1 = columns.SingleOrDefault(c => c.ColumnName == columnName1);
        var column2 = columns.SingleOrDefault(c => c.ColumnName == columnName2);
        var column4 = columns.SingleOrDefault(c => c.ColumnName == columnName4);
        var column5 = columns.SingleOrDefault(c => c.ColumnName == columnName5);

        Assert.NotNull(column1);
        Assert.NotNull(column1.DefaultExpression);
        Assert.NotEmpty(column1.DefaultExpression);

        Assert.NotNull(column2);
        Assert.NotNull(column2.DefaultExpression);
        Assert.NotEmpty(column2.DefaultExpression);

        Assert.NotNull(column4);
        Assert.NotNull(column4.DefaultExpression);
        Assert.NotEmpty(column4.DefaultExpression);

        Assert.NotNull(column5);
        Assert.NotNull(column5.DefaultExpression);
        Assert.NotEmpty(column5.DefaultExpression);

        if (defaultGuidSql != null)
        {
            var column3 = columns.SingleOrDefault(c => c.ColumnName == columnName3);
            Assert.NotNull(column3);
            Assert.NotNull(column3.DefaultExpression);
            Assert.NotEmpty(column3.DefaultExpression);
        }

        // Now try to remove the default expressions (first using the column name, then using the constraint name)
        Assert.True(
            await db.DropDefaultConstraintOnColumnIfExistsAsync(schemaName, tableName, columnName1)
        );
        var constraintName = table
            ?.DefaultConstraints.First(dc =>
                dc.ColumnName.Equals(column2.ColumnName, StringComparison.OrdinalIgnoreCase)
            )
            .ConstraintName;
        Assert.NotNull(constraintName);
        Assert.NotEmpty(constraintName);
        Assert.True(
            await db.DropDefaultConstraintIfExistsAsync(schemaName, tableName, constraintName)
        );

        // TODO: timestamp columns can't have default values dropped in MariaDB, WEIRD!
        // might have to change syntax to use ALTER TABLE table_name MODIFY COLUMN column_name TIMESTAMP NULL;
        if (
            db.GetDbProviderType() != DbProviderType.MySql
            || (version.Major != 10 && version.Major != 11)
        )
        {
            var table2 = await db.GetTableAsync(schemaName, tableName);
            columns = await db.GetColumnsAsync(schemaName, tableName);
            column1 = columns.SingleOrDefault(c => c.ColumnName == columnName1);
            column2 = columns.SingleOrDefault(c => c.ColumnName == columnName2);

            Assert.Equal(table!.DefaultConstraints.Count - 2, table2!.DefaultConstraints.Count);

            Assert.NotNull(column1);
            Assert.Null(column1.DefaultExpression);

            Assert.NotNull(column2);
            Assert.Null(column2.DefaultExpression);
        }

        await db.DropTableIfExistsAsync(schemaName, tableName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("my_app")]
    protected virtual async Task Can_perform_simple_CRUD_on_Columns_Async(string? schemaName)
    {
        using var db = await OpenConnectionAsync();
        await InitFreshSchemaAsync(db, schemaName);

        var dbType = db.GetDbProviderType();
        var dbTypeMap = db.GetProviderTypeMap();

        const string tableName = "testWithTypedColumns";
        const string columnName = "testColumn";

        await db.CreateTableIfNotExistsAsync(
            schemaName,
            tableName,
            [
                new DxColumn(
                    schemaName,
                    tableName,
                    "id",
                    typeof(int),
                    isPrimaryKey: true,
                    isAutoIncrement: true
                )
            ]
        );

        // try adding a columnName of all the supported types
        var i = 0;
        foreach (var type in GetSupportedTypes(dbTypeMap))
        {
            // create a column with the supported type
            var uniqueColumnName = $"{columnName}_{type.Name.ToAlpha()}_{i++}";
            var column = new DxColumn(
                schemaName,
                tableName,
                uniqueColumnName,
                type,
                isNullable: true
            );
            var columnCreated = await db.CreateColumnIfNotExistsAsync(column);

            if (!columnCreated)
            {
                columnCreated = await db.CreateColumnIfNotExistsAsync(column);
                Assert.True(columnCreated);
            }
        }

        await db.DropTableIfExistsAsync(schemaName, tableName);

        // Output.WriteLine("Column Exists: {0}.{1}", tableName, columnName);
        // exists = await db.DoesColumnExistAsync(schemaName, tableName, columnName);
        // Assert.True(exists);

        // Output.WriteLine("Dropping columnName: {0}.{1}", tableName, columnName);
        // await db.DropColumnIfExistsAsync(schemaName, tableName, columnName);

        // Output.WriteLine("Column Exists: {0}.{1}", tableName, columnName);
        // exists = await db.DoesColumnExistAsync(schemaName, tableName, columnName);
        // Assert.False(exists);

        // // try adding a columnName of all the supported types
        // var columnCount = 1;
        // var addColumns = new List<DxColumn>
        // {
        //     new(schemaName, tableName2, "intid" + columnCount++, typeof(int)),
        //     new(
        //         schemaName,
        //         tableName2,
        //         "intpkid" + columnCount++,
        //         typeof(int),
        //         isPrimaryKey: true,
        //         isAutoIncrement: supportsMultipleIdentityColumns ? true : false
        //     ),
        //     new(schemaName, tableName2, "intucid" + columnCount++, typeof(int), isUnique: true),
        //     new(
        //         schemaName,
        //         tableName2,
        //         "id" + columnCount++,
        //         typeof(int),
        //         isUnique: true,
        //         isIndexed: true
        //     ),
        //     new(schemaName, tableName2, "intixid" + columnCount++, typeof(int), isIndexed: true),
        //     new(
        //         schemaName,
        //         tableName2,
        //         "colWithFk" + columnCount++,
        //         typeof(int),
        //         isForeignKey: true,
        //         referencedTableName: tableName,
        //         referencedColumnName: "id",
        //         onDelete: DxForeignKeyAction.Cascade,
        //         onUpdate: DxForeignKeyAction.Cascade
        //     ),
        //     new(
        //         schemaName,
        //         tableName2,
        //         "createdDateColumn" + columnCount++,
        //         typeof(DateTime),
        //         defaultExpression: defaultDateTimeSql
        //     ),
        //     new(
        //         schemaName,
        //         tableName2,
        //         "newidColumn" + columnCount++,
        //         typeof(Guid),
        //         defaultExpression: defaultGuidSql
        //     ),
        //     new(schemaName, tableName2, "bigintColumn" + columnCount++, typeof(long)),
        //     new(schemaName, tableName2, "binaryColumn" + columnCount++, typeof(byte[])),
        //     new(schemaName, tableName2, "bitColumn" + columnCount++, typeof(bool)),
        //     new(schemaName, tableName2, "charColumn" + columnCount++, typeof(string), length: 10),
        //     new(schemaName, tableName2, "dateColumn" + columnCount++, typeof(DateTime)),
        //     new(schemaName, tableName2, "datetimeColumn" + columnCount++, typeof(DateTime)),
        //     new(schemaName, tableName2, "datetime2Column" + columnCount++, typeof(DateTime)),
        //     new(
        //         schemaName,
        //         tableName2,
        //         "datetimeoffsetColumn" + columnCount++,
        //         typeof(DateTimeOffset)
        //     ),
        //     new(
        //         schemaName,
        //         tableName2,
        //         "decimalColumn" + columnCount++,
        //         typeof(decimal),
        //         precision: 16,
        //         scale: 3
        //     ),
        //     new(
        //         schemaName,
        //         tableName2,
        //         "decimalColumnWithPrecision" + columnCount++,
        //         typeof(decimal),
        //         precision: 10
        //     ),
        //     new(
        //         schemaName,
        //         tableName2,
        //         "decimalColumnWithPrecisionAndScale" + columnCount++,
        //         typeof(decimal),
        //         precision: 10,
        //         scale: 5
        //     ),
        //     new(schemaName, tableName2, "floatColumn" + columnCount++, typeof(double)),
        //     new(schemaName, tableName2, "imageColumn" + columnCount++, typeof(byte[])),
        //     new(schemaName, tableName2, "intColumn" + columnCount++, typeof(int)),
        //     new(schemaName, tableName2, "moneyColumn" + columnCount++, typeof(decimal)),
        //     new(schemaName, tableName2, "ncharColumn" + columnCount++, typeof(string), length: 10),
        //     new(
        //         schemaName,
        //         tableName2,
        //         "ntextColumn" + columnCount++,
        //         typeof(string),
        //         length: int.MaxValue
        //     ),
        //     new(schemaName, tableName2, "floatColumn2" + columnCount++, typeof(float)),
        //     new(schemaName, tableName2, "doubleColumn2" + columnCount++, typeof(double)),
        //     new(schemaName, tableName2, "guidArrayColumn" + columnCount++, typeof(Guid[])),
        //     new(schemaName, tableName2, "intArrayColumn" + columnCount++, typeof(int[])),
        //     new(schemaName, tableName2, "longArrayColumn" + columnCount++, typeof(long[])),
        //     new(schemaName, tableName2, "doubleArrayColumn" + columnCount++, typeof(double[])),
        //     new(schemaName, tableName2, "decimalArrayColumn" + columnCount++, typeof(decimal[])),
        //     new(schemaName, tableName2, "stringArrayColumn" + columnCount++, typeof(string[])),
        //     new(
        //         schemaName,
        //         tableName2,
        //         "stringDictionaryArrayColumn" + columnCount++,
        //         typeof(Dictionary<string, string>)
        //     ),
        //     new(
        //         schemaName,
        //         tableName2,
        //         "objectDitionaryArrayColumn" + columnCount++,
        //         typeof(Dictionary<string, object>)
        //     )
        // };
        // await db.DropTableIfExistsAsync(schemaName, tableName2);
        // await db.CreateTableIfNotExistsAsync(schemaName, tableName2, [addColumns[0]]);
        // foreach (var col in addColumns.Skip(1))
        // {
        //     await db.CreateColumnIfNotExistsAsync(col);
        //     var columns = await db.GetColumnsAsync(schemaName, tableName2);
        //     // immediately do a check to make sure column was created as expected
        //     var column = await db.GetColumnAsync(schemaName, tableName2, col.ColumnName);
        //     Assert.NotNull(column);

        //     if (!string.IsNullOrWhiteSpace(schemaName) && db.SupportsSchemas())
        //     {
        //         Assert.Equal(schemaName, column.SchemaName, true);
        //     }

        //     try
        //     {
        //         Assert.Equal(col.IsIndexed, column.IsIndexed);
        //         Assert.Equal(col.IsUnique, column.IsUnique);
        //         Assert.Equal(col.IsPrimaryKey, column.IsPrimaryKey);
        //         Assert.Equal(col.IsAutoIncrement, column.IsAutoIncrement);
        //         Assert.Equal(col.IsNullable, column.IsNullable);
        //         Assert.Equal(col.IsForeignKey, column.IsForeignKey);
        //         if (col.IsForeignKey)
        //         {
        //             Assert.Equal(col.ReferencedTableName, column.ReferencedTableName, true);
        //             Assert.Equal(col.ReferencedColumnName, column.ReferencedColumnName, true);
        //             Assert.Equal(col.OnDelete, column.OnDelete);
        //             Assert.Equal(col.OnUpdate, column.OnUpdate);
        //         }
        //         Assert.Equal(col.DotnetType, column.DotnetType);
        //         Assert.Equal(col.Length, column.Length);
        //         Assert.Equal(col.Precision, column.Precision);
        //         Assert.Equal(col.Scale ?? 0, column.Scale ?? 0);
        //     }
        //     catch (Exception ex)
        //     {
        //         Output.WriteLine("Error validating column {0}: {1}", col.ColumnName, ex.Message);
        //         column = await db.GetColumnAsync(schemaName, tableName2, col.ColumnName);
        //     }

        //     Assert.NotNull(column?.ProviderDataType);
        //     Assert.NotEmpty(column.ProviderDataType);
        //     if (!string.IsNullOrWhiteSpace(col.ProviderDataType))
        //     {
        //         if (
        //             !col.ProviderDataType.Equals(
        //                 column.ProviderDataType,
        //                 StringComparison.OrdinalIgnoreCase
        //             )
        //         )
        //         {
        //             // then we want to make sure that the new provider data type in the database is more complete than the one we provided
        //             // sometimes, if you tell a database to create a column with a type of "decimal", it will actually create it as "decimal(11)" or something similar
        //             // in our case here, too, when creating a numeric(10, 5) column, the database might create it as decimal(10, 5)
        //             // so we CAN'T just compare the two strings directly
        //             // Assert.True(col.ProviderDataType.Length < column.ProviderDataType.Length);

        //             // sometimes, it's tricky to know what the database will do, so we just want to make sure that the database type is at least as specific as the one we provided
        //             if (col.Length.HasValue)
        //                 Assert.Equal(col.Length, column.Length);
        //             if (col.Precision.HasValue)
        //                 Assert.Equal(col.Precision, column.Precision);
        //             if (col.Scale.HasValue)
        //                 Assert.Equal(col.Scale, column.Scale);
        //         }
        //     }
        // }

        // var actualColumns = await db.GetColumnsAsync(schemaName, tableName2);
        // Output.WriteLine(JsonConvert.SerializeObject(actualColumns, Formatting.Indented));
        // var columnNames = await db.GetColumnNamesAsync(schemaName, tableName2);
        // var expectedColumnNames = addColumns
        //     .OrderBy(c => c.ColumnName.ToLowerInvariant())
        //     .Select(c => c.ColumnName.ToLowerInvariant())
        //     .ToArray();
        // var actualColumnNames = columnNames
        //     .OrderBy(s => s.ToLowerInvariant())
        //     .Select(s => s.ToLowerInvariant())
        //     .ToArray();
        // Output.WriteLine("Expected columns: {0}", string.Join(", ", expectedColumnNames));
        // Output.WriteLine("Actual columns: {0}", string.Join(", ", actualColumnNames));
        // Output.WriteLine("Expected columns count: {0}", expectedColumnNames.Length);
        // Output.WriteLine("Actual columns count: {0}", actualColumnNames.Length);
        // Output.WriteLine(
        //     "Expected not in actual: {0}",
        //     string.Join(", ", expectedColumnNames.Except(actualColumnNames))
        // );
        // Output.WriteLine(
        //     "Actual not in expected: {0}",
        //     string.Join(", ", actualColumnNames.Except(expectedColumnNames))
        // );
        // Assert.Equal(expectedColumnNames.Length, actualColumnNames.Length);
        // // Assert.Same(expectedColumnNames, actualColumnNames);

        // // validate that:
        // // - all columns are of the expected types
        // // - all indexes are created correctly
        // // - all foreign keys are created correctly
        // // - all default values are set correctly
        // // - all column lengths are set correctly
        // // - all column scales are set correctly
        // // - all column precision is set correctly
        // // - all columns are nullable or not nullable as specified
        // // - all columns are unique or not unique as specified
        // // - all columns are indexed or not indexed as specified
        // // - all columns are foreign key or not foreign key as specified
        // var table = await db.GetTableAsync(schemaName, tableName2);
        // Assert.NotNull(table);

        // foreach (var column in table.Columns)
        // {
        //     var originalColumn = addColumns.SingleOrDefault(c =>
        //         c.ColumnName.Equals(column.ColumnName, StringComparison.OrdinalIgnoreCase)
        //     );
        //     Assert.NotNull(originalColumn);
        // }

        // // general count tests
        // // some providers like MySQL create unique constraints for unique indexes, and vice-versa, so we can't just count the unique indexes
        // Assert.Equal(
        //     addColumns.Count(c => !c.IsIndexed && c.IsUnique),
        //     dbType == DbProviderType.MySql
        //         ? table.UniqueConstraints.Count / 2
        //         : table.UniqueConstraints.Count
        // );
        // Assert.Equal(
        //     addColumns.Count(c => c.IsIndexed && !c.IsUnique),
        //     table.Indexes.Count(c => !c.IsUnique)
        // );
        // var expectedUniqueIndexes = addColumns.Where(c => c.IsIndexed && c.IsUnique).ToArray();
        // var actualUniqueIndexes = table.Indexes.Where(c => c.IsUnique).ToArray();
        // Assert.Equal(
        //     expectedUniqueIndexes.Length,
        //     dbType == DbProviderType.MySql
        //         ? actualUniqueIndexes.Length / 2
        //         : actualUniqueIndexes.Length
        // );
        // Assert.Equal(addColumns.Count(c => c.IsForeignKey), table.ForeignKeyConstraints.Count());
        // Assert.Equal(
        //     addColumns.Count(c => c.DefaultExpression != null),
        //     table.DefaultConstraints.Count()
        // );
        // Assert.Equal(
        //     addColumns.Count(c => c.CheckExpression != null),
        //     table.CheckConstraints.Count()
        // );
        // Assert.Equal(addColumns.Count(c => c.IsNullable), table.Columns.Count(c => c.IsNullable));
        // Assert.Equal(
        //     addColumns.Count(c => c.IsPrimaryKey && c.IsAutoIncrement),
        //     table.Columns.Count(c => c.IsPrimaryKey && c.IsAutoIncrement)
        // );
        // Assert.Equal(addColumns.Count(c => c.IsUnique), table.Columns.Count(c => c.IsUnique));

        // var indexedColumnsExpected = addColumns.Where(c => c.IsIndexed).ToArray();
        // var uniqueColumnsNonIndexed = addColumns.Where(c => c.IsUnique && !c.IsIndexed).ToArray();

        // var indexedColumnsActual = table.Columns.Where(c => c.IsIndexed).ToArray();

        // Assert.Equal(
        //     dbType == DbProviderType.MySql
        //         ? indexedColumnsExpected.Length + uniqueColumnsNonIndexed.Length
        //         : indexedColumnsExpected.Length,
        //     indexedColumnsActual.Length
        // );

        // await db.DropTableIfExistsAsync(schemaName, tableName2);
        // await db.DropTableIfExistsAsync(schemaName, tableName);
    }
}
