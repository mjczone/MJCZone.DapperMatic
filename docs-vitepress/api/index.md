# API Reference

The DapperMatic API provides comprehensive DDL operations through extension methods on `IDbConnection`.

## Namespaces

- [MJCZone.DapperMatic](./mjczone-dappermatic) - Core extension methods and functionality

## Quick Reference

### Extension Method Categories

| Category | Description |
|----------|-------------|
| **General Methods** | Database version, provider detection |
| **Schema Methods** | Create, drop, query schemas |
| **Table Methods** | Create, alter, drop tables |
| **Column Methods** | Add, modify, drop columns |
| **Constraint Methods** | Primary keys, foreign keys, checks, defaults |
| **Index Methods** | Create, drop, query indexes |
| **View Methods** | Create, drop, query views |

## Getting Started with the API

```csharp
using MJCZone.DapperMatic;
using MJCZone.DapperMatic.Models;

// All methods extend IDbConnection
using var connection = new SqlConnection(connectionString);

// Example: Create a table
var table = new DmTable("Users") { /* ... */ };
await connection.CreateTableIfNotExistsAsync("dbo", table);
```

## Common Parameters

Most extension methods share these common parameters:

- `schemaName` - The schema/database name (provider-specific)
- `transaction` - Optional database transaction
- `cancellationToken` - Optional cancellation token
- `commandTimeout` - Optional command timeout in seconds

## Browse API

Explore the complete API documentation:

<ApiBrowser />