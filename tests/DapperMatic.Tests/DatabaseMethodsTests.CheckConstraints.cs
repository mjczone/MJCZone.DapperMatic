using System.Data;
using System.Data.Entity;
using Dapper;
using DapperMatic.Models;
using DapperMatic.Providers;
using Microsoft.VisualBasic;
using Newtonsoft.Json;
using Xunit.Abstractions;

namespace DapperMatic.Tests;

public abstract partial class DatabaseMethodsTests
{
    [Fact]
    protected virtual async Task Can_perform_simple_CRUD_on_CheckConstraints_Async()
    {
        using var connection = await OpenConnectionAsync();

        await connection.CreateTableIfNotExistsAsync(
            null,
            "testTable",
            [new DxColumn(null, "testTable", "testColumn", typeof(int))]
        );

        var constraintName = $"ck_testTable";
        var exists = await connection.CheckConstraintExistsAsync(null, "testTable", constraintName);

        if (exists)
            await connection.DropCheckConstraintIfExistsAsync(null, "testTable", constraintName);

        await connection.CreateCheckConstraintIfNotExistsAsync(
            null,
            "testTable",
            null,
            constraintName,
            "testColumn > 0"
        );

        exists = await connection.CheckConstraintExistsAsync(null, "testTable", constraintName);
        Assert.True(exists);

        var existingConstraint = await connection.GetCheckConstraintAsync(
            null,
            "testTable",
            constraintName
        );
        Assert.Equal(
            constraintName,
            existingConstraint?.ConstraintName,
            StringComparer.OrdinalIgnoreCase
        );

        var checkConstraintNames = await connection.GetCheckConstraintNamesAsync(null, "testTable");
        Assert.Contains(constraintName, checkConstraintNames, StringComparer.OrdinalIgnoreCase);

        await connection.DropCheckConstraintIfExistsAsync(null, "testTable", constraintName);
        exists = await connection.CheckConstraintExistsAsync(null, "testTable", constraintName);
        Assert.False(exists);
    }
}
