# Database Providers

DapperMatic supports multiple database providers, each with their own connection types and specific features. This page covers the supported providers and their capabilities.

## Supported Providers

### SQL Server

**Supported Versions:** SQL Server 2019+  
**Connection Package:** `Microsoft.Data.SqlClient`  
**Schema Support:** Full schema support with `dbo` as default

```csharp
using Microsoft.Data.SqlClient;

var connectionString = "Server=localhost;Database=MyApp;Integrated Security=true;";
using var connection = new SqlConnection(connectionString);

// SQL Server uses schemas extensively
await connection.CreateTableIfNotExistsAsync("dbo", table);
```

**Key Features:**
- Full DDL support including all constraint types
- Auto-increment columns via `IDENTITY`
- Rich data type support including spatial types
- Computed columns and check constraints
- Filtered indexes and included columns

**Limitations:**
- Schema name required for most operations
- Some DDL operations require elevated permissions

### MySQL / MariaDB

**Supported Versions:** MySQL 8.0+, MariaDB 10.5+  
**Connection Package:** `MySqlConnector`  
**Schema Support:** Database-level organization (no schemas)

```csharp
using MySqlConnector;

var connectionString = "Server=localhost;Database=myapp;Uid=user;Pwd=password;";
using var connection = new MySqlConnection(connectionString);

// MySQL doesn't use schemas - pass null or database name
await connection.CreateTableIfNotExistsAsync(null, table);
```

**Key Features:**
- Auto-increment columns via `AUTO_INCREMENT`
- Full-text search indexes
- Spatial data types and functions
- JSON column type support
- Partitioning support

**Limitations:**
- No schema concept (database = schema)
- Limited check constraint support (MySQL 8.0.16+)
- Some DDL operations don't support transactions

### PostgreSQL

**Supported Versions:** PostgreSQL 12+  
**Connection Package:** `Npgsql`  
**Schema Support:** Full schema support with `public` as default

```csharp
using Npgsql;

var connectionString = "Host=localhost;Database=myapp;Username=user;Password=password;";
using var connection = new NpgsqlConnection(connectionString);

// PostgreSQL is case-sensitive for quoted identifiers
await connection.CreateTableIfNotExistsAsync("public", table);
```

**Key Features:**
- Advanced data types (arrays, JSON, UUID, etc.)
- Full ACID compliance
- Excellent performance with large datasets
- Rich indexing options (GiST, GIN, SP-GiST, BRIN)
- Native array support in DapperMatic

**Limitations:**
- Case-sensitive for quoted identifiers
- Some advanced features may not be portable

### SQLite

**Supported Versions:** SQLite 3.35+  
**Connection Package:** `Microsoft.Data.Sqlite`  
**Schema Support:** Single database file (no schemas)

```csharp
using Microsoft.Data.Sqlite;

var connectionString = "Data Source=myapp.db";
using var connection = new SqliteConnection(connectionString);

// SQLite doesn't use schemas
await connection.CreateTableIfNotExistsAsync(null, table);
```

**Key Features:**
- Zero-configuration embedded database
- Full ACID compliance
- Cross-platform compatibility
- JSON support (SQLite 3.38+)
- Excellent for development and testing

**Limitations:**
- Limited ALTER TABLE support (DapperMatic handles this)
- No native date/time types (stored as TEXT/INTEGER)
- Single writer at a time
- No schemas or stored procedures

## Provider-Specific Considerations

### Connection String Management

::: code-group
```csharp [SQL Server]
// Windows Authentication
"Server=localhost;Database=MyApp;Integrated Security=true;"

// SQL Authentication
"Server=localhost;Database=MyApp;User Id=user;Password=pass;"
```

```csharp [MySQL]
// Standard connection
"Server=localhost;Database=myapp;Uid=user;Pwd=password;"

// With SSL
"Server=localhost;Database=myapp;Uid=user;Pwd=password;SslMode=Required;"
```

```csharp [PostgreSQL]
// Standard connection
"Host=localhost;Database=myapp;Username=user;Password=password;"

// With connection pooling
"Host=localhost;Database=myapp;Username=user;Password=password;Pooling=true;Maximum Pool Size=20;"
```

```csharp [SQLite]
// File-based
"Data Source=myapp.db"

// In-memory (testing)
"Data Source=:memory:"
```
:::

### Data Type Mapping

| .NET Type | SQL Server | MySQL | PostgreSQL | SQLite |
|-----------|------------|-------|------------|--------|
| `int` | `INT` | `INT` | `INTEGER` | `INTEGER` |
| `long` | `BIGINT` | `BIGINT` | `BIGINT` | `INTEGER` |
| `string` | `NVARCHAR` | `VARCHAR` | `TEXT` | `TEXT` |
| `DateTime` | `DATETIME2` | `DATETIME` | `TIMESTAMP` | `TEXT` |
| `bool` | `BIT` | `BOOLEAN` | `BOOLEAN` | `INTEGER` |
| `decimal` | `DECIMAL` | `DECIMAL` | `NUMERIC` | `TEXT` |
| `Guid` | `UNIQUEIDENTIFIER` | `CHAR(36)` | `UUID` | `TEXT` |

### Auto-Increment Patterns

::: code-group
```csharp [SQL Server]
new DmColumn("Id", typeof(int)) 
{ 
    IsNullable = false, 
    IsAutoIncrement = true // Creates IDENTITY(1,1)
}
```

```csharp [MySQL]
new DmColumn("Id", typeof(int)) 
{ 
    IsNullable = false, 
    IsAutoIncrement = true // Creates AUTO_INCREMENT
}
```

```csharp [PostgreSQL]
new DmColumn("Id", typeof(int)) 
{ 
    IsNullable = false, 
    IsAutoIncrement = true // Creates SERIAL/IDENTITY
}
```

```csharp [SQLite]
new DmColumn("Id", typeof(int)) 
{ 
    IsNullable = false, 
    IsAutoIncrement = true // Uses INTEGER PRIMARY KEY
}
```
:::

## Provider Selection

### When to Choose SQL Server
- Enterprise applications requiring high availability
- Windows-centric environments
- Need for advanced features like partitioning, replication
- Integration with Microsoft ecosystem

### When to Choose MySQL/MariaDB
- Web applications with high read loads
- Open-source preference
- Need for master-slave replication
- Cost-sensitive projects

### When to Choose PostgreSQL
- Applications requiring complex queries
- Need for advanced data types (JSON, arrays, spatial)
- ACID compliance is critical
- Open-source with enterprise features

### When to Choose SQLite
- Desktop applications
- Mobile applications
- Development and testing
- Small to medium datasets
- Zero-configuration requirements

## Best Practices

1. **Use connection factories** for better connection management
2. **Always specify timeouts** for long-running DDL operations
3. **Test DDL operations** against all target providers
4. **Use transactions** where supported for consistency
5. **Handle provider-specific exceptions** appropriately

## Migration Between Providers

DapperMatic's model-first approach makes it easier to migrate between providers, but consider:

- **Data type compatibility** - some types don't have direct equivalents
- **Schema differences** - SQL Server schemas vs MySQL databases
- **Feature availability** - not all features are available on all providers
- **Performance characteristics** - query patterns may need optimization

## Getting Help

If you encounter provider-specific issues:

1. Check the [troubleshooting guide](/guide/troubleshooting)
2. Review the provider's documentation
3. File an issue on [GitHub](https://github.com/mjczone/MJCZone.DapperMatic/issues)