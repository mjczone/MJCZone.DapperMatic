using DapperMatic.Models;

namespace DapperMatic.Tests;

public abstract partial class DatabaseMethodsTests
{
    [Fact]
    protected virtual async Task Can_perform_simple_CRUD_on_ForeignKeyConstraints_Async()
    {
        using var connection = await OpenConnectionAsync();

        const string tableName = "testWithFk";
        const string refTableName = "testPk";
        const string columnName = "testFkColumn";
        const string foreignKeyName = "testFk";

        await connection.CreateTableIfNotExistsAsync(null, tableName);
        await connection.CreateTableIfNotExistsAsync(null, refTableName);
        await connection.CreateColumnIfNotExistsAsync(
            null,
            tableName,
            columnName,
            typeof(int),
            defaultExpression: "1",
            isNullable: false
        );

        output.WriteLine($"Foreign Key Exists: {tableName}.{foreignKeyName}");
        var exists = await connection.ForeignKeyConstraintExistsAsync(
            null,
            tableName,
            foreignKeyName
        );
        Assert.False(exists);

        output.WriteLine($"Creating foreign key: {tableName}.{foreignKeyName}");
        await connection.CreateForeignKeyConstraintIfNotExistsAsync(
            null,
            tableName,
            foreignKeyName,
            [new DxOrderedColumn(columnName)],
            refTableName,
            [new DxOrderedColumn("id")],
            onDelete: DxForeignKeyAction.Cascade
        );

        output.WriteLine($"Foreign Key Exists: {tableName}.{foreignKeyName}");
        exists = await connection.ForeignKeyConstraintExistsAsync(null, tableName, foreignKeyName);
        Assert.True(exists);
        exists = await connection.ForeignKeyConstraintExistsOnColumnAsync(
            null,
            tableName,
            columnName
        );
        Assert.True(exists);

        output.WriteLine($"Get Foreign Key Names: {tableName}");
        var fkNames = await connection.GetForeignKeyConstraintNamesAsync(null, tableName);
        Assert.Contains(
            fkNames,
            fk => fk.Equals(foreignKeyName, StringComparison.OrdinalIgnoreCase)
        );

        output.WriteLine($"Get Foreign Keys: {tableName}");
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

        output.WriteLine($"Dropping foreign key: {foreignKeyName}");
        await connection.DropForeignKeyConstraintIfExistsAsync(null, tableName, foreignKeyName);

        output.WriteLine($"Foreign Key Exists: {foreignKeyName}");
        exists = await connection.ForeignKeyConstraintExistsAsync(null, tableName, foreignKeyName);
        Assert.False(exists);
        exists = await connection.ForeignKeyConstraintExistsOnColumnAsync(
            null,
            tableName,
            columnName
        );
        Assert.False(exists);
    }
}
