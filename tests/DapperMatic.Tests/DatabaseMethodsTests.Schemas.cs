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
            Logger.LogInformation("This test requires a database that supports schemas.");
            return;
        }

        var schemaName = "test";

        var exists = await connection.DoesSchemaExistAsync(schemaName);
        if (exists)
            await connection.DropSchemaIfExistsAsync(schemaName);

        exists = await connection.DoesSchemaExistAsync(schemaName);
        Assert.False(exists);

        Logger.LogInformation("Creating schemaName: {schemaName}", schemaName);
        var created = await connection.CreateSchemaIfNotExistsAsync(schemaName);
        Assert.True(created);
        exists = await connection.DoesSchemaExistAsync(schemaName);
        Assert.True(exists);

        var schemas = await connection.GetSchemaNamesAsync();
        Assert.Contains(schemaName, schemas, StringComparer.OrdinalIgnoreCase);

        Logger.LogInformation("Dropping schemaName: {schemaName}", schemaName);
        var dropped = await connection.DropSchemaIfExistsAsync(schemaName);
        Assert.True(dropped);

        exists = await connection.DoesSchemaExistAsync(schemaName);
        Assert.False(exists);
    }
}
