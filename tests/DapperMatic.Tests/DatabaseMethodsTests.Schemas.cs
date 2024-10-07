using Microsoft.Extensions.Logging;

namespace DapperMatic.Tests;

public abstract partial class DatabaseMethodsTests
{
    [Fact]
    protected virtual async Task Can_perform_simple_CRUD_on_Schemas_Async()
    {
        using var connection = await OpenConnectionAsync();

        var supportsSchemas = connection.SupportsSchemas();
        if (!supportsSchemas)
        {
            output.WriteLine("This test requires a database that supports schemas.");
            return;
        }

        var schemaName = "test";

        var exists = await connection.DoesSchemaExistAsync(schemaName);
        if (exists)
            await connection.DropSchemaIfExistsAsync(schemaName);

        exists = await connection.DoesSchemaExistAsync(schemaName);
        Assert.False(exists);

        output.WriteLine("Creating schemaName: {0}", schemaName);
        var created = await connection.CreateSchemaIfNotExistsAsync(schemaName);
        Assert.True(created);
        exists = await connection.DoesSchemaExistAsync(schemaName);
        Assert.True(exists);

        var schemas = await connection.GetSchemaNamesAsync();
        Assert.Contains(schemaName, schemas, StringComparer.OrdinalIgnoreCase);

        output.WriteLine("Dropping schemaName: {0}", schemaName);
        var dropped = await connection.DropSchemaIfExistsAsync(schemaName);
        Assert.True(dropped);

        exists = await connection.DoesSchemaExistAsync(schemaName);
        Assert.False(exists);
    }
}
