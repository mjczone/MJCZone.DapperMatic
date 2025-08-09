# Table Methods

Table methods allow you to create, modify, and query database tables.

## Check Table Existence

```csharp
using var db = await connectionFactory.OpenConnectionAsync();
using var tx = db.BeginTransaction();

// Check if a table exists
bool exists = await db.DoesTableExistAsync("app", "employees", tx, cancellationToken);
```

## Create Tables

### Using a DmTable Model

```csharp
var table = new DmTable("employees")
{
    Columns = new[]
    {
        new DmColumn("id", typeof(int)) { IsNullable = false, IsAutoIncrement = true },
        new DmColumn("name", typeof(string)) { MaxLength = 100, IsNullable = false },
        new DmColumn("email", typeof(string)) { MaxLength = 200, IsNullable = false },
        new DmColumn("hire_date", typeof(DateTime)) { IsNullable = false }
    },
    PrimaryKey = new DmPrimaryKeyConstraint("pk_employees", "id"),
    Indexes = new[]
    {
        new DmIndex("ix_employees_email", new[] { "email" }) { IsUnique = true }
    }
};

bool created = await db.CreateTableIfNotExistsAsync("app", table);
```

### Using Individual Components

```csharp
bool created = await db.CreateTableIfNotExistsAsync(
    schemaName: "app",
    tableName: "employees",
    columns: new[]
    {
        new DmColumn("id", typeof(int)) { IsNullable = false, IsAutoIncrement = true },
        new DmColumn("name", typeof(string)) { MaxLength = 100, IsNullable = false }
    },
    primaryKey: new DmPrimaryKeyConstraint("pk_employees", "id"),
    checkConstraints: new[]
    {
        new DmCheckConstraint("ck_employees_name_length", "LEN(name) > 0")
    },
    defaultConstraints: new[]
    {
        new DmDefaultConstraint("df_employees_hire_date", "hire_date", "GETDATE()")
    },
    uniqueConstraints: new[]
    {
        new DmUniqueConstraint("uq_employees_email", new[] { "email" })
    },
    foreignKeyConstraints: new[]
    {
        new DmForeignKeyConstraint("fk_employees_department", 
            new[] { "department_id" }, 
            "departments", 
            new[] { "id" })
    },
    indexes: new[]
    {
        new DmIndex("ix_employees_name", new[] { "name" })
    }
);
```

## Query Tables

### Get Table Names

```csharp
// Get all table names in a schema
List<string> tableNames = await db.GetTableNamesAsync("app");

// Get table names matching a pattern
List<string> employeeTables = await db.GetTableNamesAsync("app", "emp%");
```

### Get Table Details

```csharp
// Get all tables with full metadata
List<DmTable> tables = await db.GetTablesAsync("app");

// Get a specific table
DmTable? table = await db.GetTableAsync("app", "employees");
if (table != null)
{
    Console.WriteLine($"Table: {table.TableName}");
    Console.WriteLine($"Columns: {table.Columns.Count}");
    Console.WriteLine($"Has Primary Key: {table.PrimaryKey != null}");
}
```

## Modify Tables

### Drop Table

```csharp
bool dropped = await db.DropTableIfExistsAsync("app", "employees");
```

### Rename Table

```csharp
bool renamed = await db.RenameTableIfExistsAsync(
    schemaName: "app", 
    oldTableName: "employees", 
    newTableName: "staff"
);
```

### Truncate Table

```csharp
// Remove all rows from a table (faster than DELETE)
bool truncated = await db.TruncateTableIfExistsAsync("app", "employees");
```

## Best Practices

1. **Always use transactions** for DDL operations in production
2. **Check existence** before creating or dropping tables
3. **Use meaningful names** for constraints and indexes
4. **Consider provider differences** - some features may vary by database

## Provider Notes

::: warning SQL Server
SQL Server requires schema names for most operations. Use "dbo" if unsure.
:::

::: warning MySQL
MySQL doesn't support schemas in the same way. Use the database name or null.
:::

::: warning PostgreSQL
PostgreSQL is case-sensitive for quoted identifiers. Use lowercase names.
:::

::: warning SQLite
SQLite has limited ALTER TABLE support. Some operations may recreate the table.
:::