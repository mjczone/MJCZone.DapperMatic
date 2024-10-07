using DapperMatic.Models;
using Microsoft.Extensions.Logging;

namespace DapperMatic.Tests;

public abstract partial class DatabaseMethodsTests
{
    [Fact]
    protected virtual async Task Can_perform_simple_CRUD_on_PrimaryKeyConstraints_Async()
    {
        using var connection = await OpenConnectionAsync();

        const string tableName = "testWithPk";
        const string columnName = "testColumn";
        const string primaryKeyName = "testPk";

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
        output.WriteLine(
            "Primary Key Exists: {0}.{1}",
            tableName,
            primaryKeyName
        );
        var exists = await connection.DoesPrimaryKeyConstraintExistAsync(null, tableName);
        Assert.False(exists);
        output.WriteLine(
            "Creating primary key: {0}.{1}",
            tableName,
            primaryKeyName
        );
        await connection.CreatePrimaryKeyConstraintIfNotExistsAsync(
            null,
            tableName,
            primaryKeyName,
            [new DxOrderedColumn(columnName)]
        );
        output.WriteLine(
            "Primary Key Exists: {0}.{1}",
            tableName,
            primaryKeyName
        );
        exists = await connection.DoesPrimaryKeyConstraintExistAsync(null, tableName);
        Assert.True(exists);
        output.WriteLine(
            "Dropping primary key: {0}.{1}",
            tableName,
            primaryKeyName
        );
        await connection.DropPrimaryKeyConstraintIfExistsAsync(null, tableName);
        output.WriteLine(
            "Primary Key Exists: {0}.{1}",
            tableName,
            primaryKeyName
        );
        exists = await connection.DoesPrimaryKeyConstraintExistAsync(null, tableName);
        Assert.False(exists);
        await connection.DropTableIfExistsAsync(null, tableName);
    }
}
