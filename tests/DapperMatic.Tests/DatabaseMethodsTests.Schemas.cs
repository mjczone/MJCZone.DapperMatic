using Microsoft.Extensions.Logging;

namespace DapperMatic.Tests;

public abstract partial class DatabaseMethodsTests
{
    [Theory]
    [InlineData("my_app")]
    protected virtual async Task Can_perform_simple_CRUD_on_Schemas_Async(string schemaName)
    {
        using var db = await OpenConnectionAsync();

        var supportsSchemas = db.SupportsSchemas();
        if (!supportsSchemas)
        {
            output.WriteLine("This test requires a database that supports schemas.");
            return;
        }

        var exists = await db.DoesSchemaExistAsync(schemaName);
        if (exists)
            await db.DropSchemaIfExistsAsync(schemaName);

        exists = await db.DoesSchemaExistAsync(schemaName);
        Assert.False(exists);

        output.WriteLine("Creating schemaName: {0}", schemaName);
        var created = await db.CreateSchemaIfNotExistsAsync(schemaName);
        Assert.True(created);
        exists = await db.DoesSchemaExistAsync(schemaName);
        Assert.True(exists);

        var schemas = await db.GetSchemaNamesAsync();
        Assert.Contains(schemaName, schemas, StringComparer.OrdinalIgnoreCase);

        output.WriteLine("Dropping schemaName: {0}", schemaName);
        var dropped = await db.DropSchemaIfExistsAsync(schemaName);
        Assert.True(dropped);

        exists = await db.DoesSchemaExistAsync(schemaName);
        Assert.False(exists);
    }
}
