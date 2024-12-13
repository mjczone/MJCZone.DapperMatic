# Table methods

```cs
using var db = await connectionFactory.OpenConnectionAsync();
using var tx = db.BeginTransaction();

// EXISTS: Check to see if a database table exists
bool exists = await db.DoesTableExistAsync("app","app_employees", tx, cancellationToken).ConfigureAwait(false)

// CREATE: Create a database table
bool created = await db.CreateTableIfNotExistsAsync("app", /* DmTable */ table);
// or
    created = await db.CreateTableIfNotExistsAsync(
        "app",
        "app_employees",
        // DmColumn[] columns,
        columns,
        // DmPrimaryKeyConstraint? primaryKey = null,
        primaryKey,
        // DmCheckConstraint[]? checkConstraints = null,
        checkConstraints,
        // DmDefaultConstraint[]? defaultConstraints = null,
        defaultConstraints,
        // DmUniqueConstraint[]? uniqueConstraints = null,
        uniqueConstraints,
        // DmForeignKeyConstraint[]? foreignKeyConstraints = null,
        foreignKeyConstraints,
        // DmIndex[]? indexes = null,
        indexes,
        ...
    );

// GET: Retrieve table names
List<string> names = await db.GetTableNamesAsync("app", "app_*", ...);

// GET: Retrieve tables
List<DmTable> tables = await db.GetTablesAsync("app", "app_*", ...);

// GET: Retrieve single table
DmTable? table = await db.GetTableAsync("app", "app_employees", ...);

// DROP: Drop a database table
bool dropped = await db.DropTableIfExistsAsync("app", "app_employees", ...);

// RENAME: Rename a database table
bool renamed = await db.RenameTableIfExistsAsync("app", "app_employees", /* new name */ "app_staff", ...);

// TRUNCATE: Truncate a database table
bool truncated = await db.TruncateTableIfExistsAsync("app", "app_employees", ...);
```
