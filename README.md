# MJCZone.DapperMatic

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![.github/workflows/build-and-test.yml](https://github.com/mjczone/MJCZone.DapperMatic/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/mjczone/MJCZone.DapperMatic/actions/workflows/build-and-test.yml)
[![.github/workflows/release.yml](https://github.com/mjczone/MJCZone.DapperMatic/actions/workflows/release.yml/badge.svg)](https://github.com/mjczone/MJCZone.DapperMatic/actions/workflows/release.yml)

**Model-first database schema management for .NET** - Create, modify, and manage database schemas using strongly-typed C# models across SQL Server, MySQL, PostgreSQL, and SQLite.

DapperMatic extends `IDbConnection` with intuitive extension methods for DDL (Data Definition Language) operations. Define your database schema in code and let DapperMatic handle the provider-specific SQL generation and execution.

## ✨ Key Features

- **🎯 Model-First Approach** - Define schemas using strongly-typed C# classes (`DmTable`, `DmColumn`, etc.)
- **📝 Data Annotations Support** - Use familiar `[Table]`, `[Key]` attributes or advanced `[DmColumn]` attributes
- **🔄 Cross-Database Support** - SQL Server, MySQL/MariaDB, PostgreSQL, SQLite with consistent API
- **🔍 Schema Reverse Engineering** - Extract complete database schemas including tables, views, constraints, and indexes
- **🛡️ SQL Injection Protected** - Comprehensive validation prevents malicious SQL injection attacks
- **⚡ Dapper Integration** - Built on top of Dapper for high-performance data access
- **🧪 Extensively Tested** - 500+ tests covering all providers and edge cases
- **📦 Zero Configuration** - Works out-of-the-box with your existing Dapper applications

## 🚀 Quick Start

### Installation

```bash
dotnet add package MJCZone.DapperMatic
```

### Basic Usage

```csharp
using MJCZone.DapperMatic;
using MJCZone.DapperMatic.Models;
using System.Data.SqlClient;

// Connect to your database (works with any IDbConnection)
using var connection = new SqlConnection("your-connection-string");
await connection.OpenAsync();

// Define a table model
var usersTable = new DmTable("dbo", "Users", new[]
{
    new DmColumn("Id", typeof(int), isPrimaryKey: true, isAutoIncrement: true),
    new DmColumn("Name", typeof(string), length: 100, isNullable: false),
    new DmColumn("Email", typeof(string), length: 255, isUnique: true),
    new DmColumn("CreatedAt", typeof(DateTime), defaultExpression: "GETDATE()")
});

// Create the table if it doesn't exist
bool created = await connection.CreateTableIfNotExistsAsync(usersTable);

// Check if table exists
bool exists = await connection.DoesTableExistAsync("dbo", "Users");

// Add a new column
await connection.CreateColumnIfNotExistsAsync("dbo", "Users", 
    new DmColumn("LastLoginAt", typeof(DateTime?), isNullable: true));

// Create an index
await connection.CreateIndexIfNotExistsAsync("dbo", "Users", "IX_Users_Email", 
    new[] { "Email" });
```

### Data Annotations Approach

DapperMatic supports both standard .NET data annotations and its own comprehensive attribute system:

```csharp
using MJCZone.DapperMatic.DataAnnotations;
using MJCZone.DapperMatic.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

// Using standard .NET attributes
[Table("Users", Schema = "dbo")]
public class User
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;
    
    [EmailAddress]
    public string Email { get; set; } = null!;
}

// Using DapperMatic-specific attributes for advanced features
[DmTable("dbo", "Products")]
[DmIndex("IX_Products_Name", new[] { nameof(Name) }, isUnique: true)]
public class Product
{
    [DmColumn(isPrimaryKey: true, isAutoIncrement: true)]
    public int Id { get; set; }
    
    [DmColumn("product_name", length: 255, isNullable: false)]
    public string Name { get; set; } = null!;
    
    [DmColumn("price", precision: 10, scale: 2, 
        checkExpression: "Price > 0", 
        defaultExpression: "0.00")]
    public decimal Price { get; set; }
    
    [DmColumn(isForeignKey: true, referencedTableName: "Categories", 
        referencedColumnName: "Id", onDelete: DmForeignKeyAction.Cascade)]
    public int CategoryId { get; set; }
    
    [DmColumn(defaultExpression: "GETDATE()")]
    public DateTime CreatedAt { get; set; }
}

// Generate table from class and create it
var productTable = DmTableFactory.GetTable<Product>();
await connection.CreateTableIfNotExistsAsync(productTable);

// Or use the type directly
var userTable = DmTableFactory.GetTable(typeof(User));
await connection.CreateTableIfNotExistsAsync(userTable);
```

### Schema Management

```csharp
// Create schema if it doesn't exist
await connection.CreateSchemaIfNotExistsAsync("app");

// Work with views
await connection.CreateViewIfNotExistsAsync("dbo", "ActiveUsers", 
    "SELECT * FROM Users WHERE LastLoginAt > DATEADD(day, -30, GETDATE())");

// Manage constraints
await connection.CreateCheckConstraintIfNotExistsAsync("dbo", "Users", "Email",
    "chk_email_format", "Email LIKE '%@%.%'");

await connection.CreateForeignKeyConstraintIfNotExistsAsync("dbo", "Orders", 
    "FK_Orders_Users", new[] { "UserId" }, "dbo", "Users", new[] { "Id" });
```

### Schema Reverse Engineering

DapperMatic provides comprehensive capabilities to extract and analyze existing database schemas. This is essential for tools that need to understand current database structure, perform schema comparisons, or generate code from existing databases.

```csharp
// Extract complete table definition with all metadata
var existingTable = await connection.GetTableAsync("dbo", "Users");
if (existingTable != null)
{
    Console.WriteLine($"Table: {existingTable.SchemaName}.{existingTable.TableName}");
    
    // Access all columns with detailed metadata
    foreach (var column in existingTable.Columns)
    {
        Console.WriteLine($"  {column.ColumnName}: {column.DotnetType}");
        Console.WriteLine($"    Nullable: {column.IsNullable}");
        Console.WriteLine($"    Primary Key: {column.IsPrimaryKey}");
        Console.WriteLine($"    Auto Increment: {column.IsAutoIncrement}");
        Console.WriteLine($"    Unique: {column.IsUnique}");
        
        if (column.IsForeignKey)
        {
            Console.WriteLine($"    References: {column.ReferencedTableName}.{column.ReferencedColumnName}");
        }
    }
    
    // Access constraints
    if (existingTable.PrimaryKeyConstraint != null)
    {
        var pkColumns = string.Join(", ", existingTable.PrimaryKeyConstraint.Columns.Select(c => c.ColumnName));
        Console.WriteLine($"  Primary Key: {pkColumns}");
    }
    
    // Access foreign key constraints
    foreach (var fk in existingTable.ForeignKeyConstraints)
    {
        Console.WriteLine($"  FK {fk.ConstraintName}: {string.Join(", ", fk.Columns.Select(c => c.ColumnName))} -> {fk.ReferencedSchemaName}.{fk.ReferencedTableName}");
    }
    
    // Access indexes
    foreach (var index in existingTable.Indexes)
    {
        var indexColumns = string.Join(", ", index.Columns.Select(c => $"{c.ColumnName} {(c.IsDescending ? "DESC" : "ASC")}"));
        Console.WriteLine($"  Index {index.IndexName}: {indexColumns} (Unique: {index.IsUnique})");
    }
}

// Get all tables in a schema
var allTables = await connection.GetTablesAsync("dbo");
Console.WriteLine($"Found {allTables.Count} tables in schema 'dbo'");

// Get table names only (lightweight operation)
var tableNames = await connection.GetTableNamesAsync("dbo");
foreach (var tableName in tableNames)
{
    Console.WriteLine($"Table: {tableName}");
}

// Extract view definitions
var salesView = await connection.GetViewAsync("dbo", "MonthlySales");
if (salesView != null)
{
    Console.WriteLine($"View Definition:\n{salesView.Definition}");
}

// Get all views
var allViews = await connection.GetViewsAsync("dbo");

// Extract individual column metadata
var emailColumn = await connection.GetColumnAsync("dbo", "Users", "Email");
if (emailColumn != null)
{
    Console.WriteLine($"Email column type: {emailColumn.DotnetType}");
    Console.WriteLine($"Max length: {emailColumn.Length}");
    Console.WriteLine($"Is unique: {emailColumn.IsUnique}");
}

// Get all columns for a table
var userColumns = await connection.GetColumnsAsync("dbo", "Users");

// Extract constraint information
var foreignKeys = await connection.GetForeignKeyConstraintsAsync("dbo", "Orders");
var checkConstraints = await connection.GetCheckConstraintsAsync("dbo", "Users");
var uniqueConstraints = await connection.GetUniqueConstraintsAsync("dbo", "Products");

// Get index information
var userIndexes = await connection.GetIndexesAsync("dbo", "Users");
var emailIndex = await connection.GetIndexAsync("dbo", "Users", "IX_Users_Email");

// List all schemas in the database
var schemas = await connection.GetSchemaNamesAsync();
Console.WriteLine($"Available schemas: {string.Join(", ", schemas)}");

// Generate C# class from existing table
var existingUsersTable = await connection.GetTableAsync("dbo", "Users");
if (existingUsersTable != null)
{
    // You can use the table definition to generate corresponding C# classes
    // or compare with your model definitions for schema validation
    var modelTable = DmTableFactory.GetTable<User>();
    // Compare existingUsersTable with modelTable for differences
}
```

## 🗄️ Supported Database Providers

| Provider | Versions Tested | Connection Type | Notes |
|----------|----------------|-----------------|-------|
| **SQL Server** | 2017, 2019, 2022 | `SqlConnection` | Full feature support |
| **MySQL** | 8.0, 8.1 | `MySqlConnection` | Includes MariaDB compatibility |
| **PostgreSQL** | 13, 14, 15, 16 | `NpgsqlConnection` | PostGIS extensions supported |
| **SQLite** | 3.x | `SQLiteConnection` | File-based and in-memory |

## 🎯 Core Capabilities

### Table Operations
- ✅ Create, drop, rename, and modify tables
- ✅ Check table existence and retrieve metadata
- ✅ Support for temporary tables

### Column Management
- ✅ Add, drop, rename, and modify columns
- ✅ Full data type mapping across providers
- ✅ Auto-increment and computed columns
- ✅ Default values and nullable constraints

### Constraint Support
- ✅ Primary keys (single and composite)
- ✅ Foreign keys with cascade options
- ✅ Unique constraints
- ✅ Check constraints with expression validation
- ✅ Default constraints

### Index Management
- ✅ Create and drop indexes
- ✅ Unique and composite indexes
- ✅ Provider-specific optimizations

### View Operations
- ✅ Create, drop, and manage views
- ✅ Secure view definition validation

### Schema Management
- ✅ Create and manage database schemas
- ✅ Cross-schema operations
- ✅ Provider compatibility handling

### Schema Reverse Engineering
- ✅ Extract complete table definitions with all metadata
- ✅ Retrieve columns, constraints, indexes, and relationships
- ✅ Analyze views and their SQL definitions
- ✅ Query existing schema structure and constraints
- ✅ Generate C# models from existing database tables

## 🔧 Advanced Features

### Type Mapping
DapperMatic automatically maps .NET types to appropriate SQL types for each database provider:

```csharp
// Automatic type mapping
new DmColumn("Price", typeof(decimal), precision: 10, scale: 2)
// → SQL Server: decimal(10,2)
// → MySQL: decimal(10,2) 
// → PostgreSQL: numeric(10,2)
// → SQLite: real

new DmColumn("Tags", typeof(string[]))  // PostgreSQL arrays
new DmColumn("Config", typeof(object))  // JSON columns where supported
```

### Provider Detection
```csharp
// Automatic provider detection
bool supportsSchemas = connection.SupportsSchemas();
var version = await connection.GetDatabaseVersionAsync();
```

### Security Features
- **SQL Injection Protection** - All user inputs are validated and sanitized
- **Expression Validation** - Check constraints and view definitions are secured
- **Identifier Normalization** - Table/column names are properly escaped

## 🆚 Comparison with Alternatives

| Feature | DapperMatic | Entity Framework Core | FluentMigrator |
|---------|-------------|----------------------|----------------|
| **Model-First** | ✅ Code-first schemas | ✅ Code-first entities | ❌ Migration-based |
| **Data Annotations** | ✅ Standard + Advanced | ✅ Standard annotations | ❌ Fluent API only |
| **Cross-Database** | ✅ 4 providers | ✅ Many providers | ✅ Many providers |
| **Performance** | ⚡ Dapper-based | 📊 ORM overhead | ⚡ Direct SQL |
| **Learning Curve** | 🟢 Minimal | 🟡 Moderate | 🟡 Moderate |
| **Schema Focus** | ✅ DDL operations | ❌ Entity operations | ✅ Migration scripts |
| **Runtime Schema Changes** | ✅ Dynamic | ❌ Requires migrations | ❌ Requires migrations |
| **Reverse Engineering** | ✅ Full schema extraction | ✅ Scaffold from database | ❌ No reverse engineering |
| **Size** | 📦 Lightweight | 📦 Full framework | 📦 Migration-focused |

### When to Choose DapperMatic

✅ **Perfect for:**
- Applications that need dynamic schema management
- Dapper-based applications requiring DDL operations
- Multi-tenant applications with varying schemas
- Tools that generate database schemas from models
- APIs that manage database structures programmatically
- Schema analysis and documentation tools
- Database migration and comparison utilities
- Code generators that create models from existing databases
- Developers who prefer data annotations over fluent APIs

❌ **Consider alternatives for:**
- Full ORM functionality (use Entity Framework Core)
- Complex migration workflows (use FluentMigrator)
- Applications that rarely modify schema structure

## 📚 Dependencies

This library requires **.NET 8.0 or later** and uses [Dapper](https://github.com/DapperLib/Dapper) for data access operations. We use a version range (`[2.1.35,3.0.0)`) to ensure compatibility with your application's Dapper version while maintaining functionality.

**Framework Requirements**: .NET 8.0+ (compatible with .NET 8 and .NET 9 applications)

**Supported Dapper Versions**: 2.1.35 through 2.1.66+ (any 2.1.x version)

**Version Conflict Resolution**: If your application uses a different Dapper 2.1.x version, NuGet will automatically resolve to your version, preventing conflicts.

## 📖 Documentation

- **Full Documentation**: [mjczone.github.io/MJCZone.DapperMatic](https://mjczone.github.io/MJCZone.DapperMatic/)
- **API Reference**: Complete method documentation with examples
- **Provider Guides**: Database-specific tips and best practices
- **Migration Examples**: Real-world usage scenarios

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guidelines](CONTRIBUTING.md) for details on:

- 🐛 Reporting bugs
- 💡 Suggesting features  
- 🔧 Submitting pull requests
- 🧪 Adding tests for new providers

## 🏗️ Project Status

- **Stability**: Production ready
- **Maintenance**: Actively maintained
- **Testing**: 500+ automated tests across all providers
- **Security**: SQL injection protected with comprehensive validation
- **Performance**: Optimized for high-throughput scenarios

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

---

⭐ **Star this repository** if you find DapperMatic helpful for your projects!