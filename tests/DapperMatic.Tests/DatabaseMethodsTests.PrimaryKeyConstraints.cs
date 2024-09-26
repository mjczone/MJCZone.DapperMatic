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
        Logger.LogInformation(
            "Primary Key Exists: {tableName}.{primaryKeyName}",
            tableName,
            primaryKeyName
        );
        var exists = await connection.DoesPrimaryKeyConstraintExistAsync(null, tableName);
        Assert.False(exists);
        Logger.LogInformation(
            "Creating primary key: {tableName}.{primaryKeyName}",
            tableName,
            primaryKeyName
        );
        await connection.CreatePrimaryKeyConstraintIfNotExistsAsync(
            null,
            tableName,
            primaryKeyName,
            [new DxOrderedColumn(columnName)]
        );
        Logger.LogInformation(
            "Primary Key Exists: {tableName}.{primaryKeyName}",
            tableName,
            primaryKeyName
        );
        exists = await connection.DoesPrimaryKeyConstraintExistAsync(null, tableName);
        Assert.True(exists);
        Logger.LogInformation(
            "Dropping primary key: {tableName}.{primaryKeyName}",
            tableName,
            primaryKeyName
        );
        await connection.DropPrimaryKeyConstraintIfExistsAsync(null, tableName);
        Logger.LogInformation(
            "Primary Key Exists: {tableName}.{primaryKeyName}",
            tableName,
            primaryKeyName
        );
        exists = await connection.DoesPrimaryKeyConstraintExistAsync(null, tableName);
        Assert.False(exists);
        await connection.DropTableIfExistsAsync(null, tableName);
    }
}
