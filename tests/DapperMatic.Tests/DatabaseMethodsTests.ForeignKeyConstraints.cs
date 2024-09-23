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
    protected virtual async Task Can_perform_simple_CRUD_on_ForeignKeyConstraints_Async()
    {
        using var connection = await OpenConnectionAsync();
    }
}
