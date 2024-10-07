using DapperMatic.Models;
using Microsoft.Extensions.Logging;

namespace DapperMatic.Tests;

public abstract partial class DatabaseMethodsTests
{
    [Fact]
    protected virtual async Task Can_perform_simple_CRUD_on_ForeignKeyConstraints_Async()
    {
        using var connection = await OpenConnectionAsync();

        const string tableName = "testWithFk";
        const string columnName = "testFkColumn";
        const string foreignKeyName = "testFk";
        const string refTableName = "testRefPk";
        const string refTableColumn = "id";

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
                )
            ]
        );
        await connection.CreateTableIfNotExistsAsync(
            null,
            refTableName,
            [
                new DxColumn(
                    null,
                    refTableName,
                    refTableColumn,
                    typeof(int),
                    defaultExpression: "1",
                    isPrimaryKey: true,
                    isNullable: false
                )
            ]
        );

        output.WriteLine(
            "Foreign Key Exists: {0}.{1}",
            tableName,
            foreignKeyName
        );
        var exists = await connection.DoesForeignKeyConstraintExistAsync(
            null,
            tableName,
            foreignKeyName
        );
        Assert.False(exists);

        output.WriteLine(
            "Creating foreign key: {0}.{1}",
            tableName,
            foreignKeyName
        );
        var created = await connection.CreateForeignKeyConstraintIfNotExistsAsync(
            null,
            tableName,
            foreignKeyName,
            [new DxOrderedColumn(columnName)],
            refTableName,
            [new DxOrderedColumn("id")],
            onDelete: DxForeignKeyAction.Cascade
        );
        Assert.True(created);

        output.WriteLine(
            "Foreign Key Exists: {0}.{1}",
            tableName,
            foreignKeyName
        );
        exists = await connection.DoesForeignKeyConstraintExistAsync(
            null,
            tableName,
            foreignKeyName
        );
        Assert.True(exists);
        exists = await connection.DoesForeignKeyConstraintExistOnColumnAsync(
            null,
            tableName,
            columnName
        );
        Assert.True(exists);

        output.WriteLine("Get Foreign Key Names: {0}", tableName);
        var fkNames = await connection.GetForeignKeyConstraintNamesAsync(null, tableName);
        Assert.Contains(
            fkNames,
            fk => fk.Equals(foreignKeyName, StringComparison.OrdinalIgnoreCase)
        );

        output.WriteLine("Get Foreign Keys: {0}", tableName);
        var fks = await connection.GetForeignKeyConstraintsAsync(null, tableName);
        Assert.Contains(
            fks,
            fk =>
                fk.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase)
                && fk.SourceColumns.Any(sc =>
                    sc.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase)
                )
                && fk.ConstraintName.Equals(foreignKeyName, StringComparison.OrdinalIgnoreCase)
                && fk.ReferencedTableName.Equals(refTableName, StringComparison.OrdinalIgnoreCase)
                && fk.ReferencedColumns.Any(sc =>
                    sc.ColumnName.Equals("id", StringComparison.OrdinalIgnoreCase)
                )
                && fk.OnDelete.Equals(DxForeignKeyAction.Cascade)
        );

        output.WriteLine("Dropping foreign key: {0}", foreignKeyName);
        await connection.DropForeignKeyConstraintIfExistsAsync(null, tableName, foreignKeyName);

        output.WriteLine("Foreign Key Exists: {0}", foreignKeyName);
        exists = await connection.DoesForeignKeyConstraintExistAsync(
            null,
            tableName,
            foreignKeyName
        );
        Assert.False(exists);
        exists = await connection.DoesForeignKeyConstraintExistOnColumnAsync(
            null,
            tableName,
            columnName
        );
        Assert.False(exists);

        await connection.DropTableIfExistsAsync(null, tableName);
        await connection.DropTableIfExistsAsync(null, refTableName);
    }
}
