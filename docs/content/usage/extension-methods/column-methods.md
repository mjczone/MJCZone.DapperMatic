# Column methods

```cs
using var db = await connectionFactory.OpenConnectionAsync();
using var tx = db.BeginTransaction();

// EXISTS: Check to see if a table column exists
bool exists = await db.DoesColumnExistAsync("app", "app_employees", "title", tx, cancellationToken).ConfigureAwait(false)

// CREATE: Create a table column
bool created = await db.CreateColumnIfNotExistsAsync("app", /* DmColumn */ column);
// or
    created = await db.CreateColumnIfNotExistsAsync(
        "app",
        "app_employees",
        // string columnName,
        "manager_id"
        // Type dotnetType,
        typeof(Guid),
        // optional parameters (the actual sql data type is derived from the dotnet type, but can be specified if desired)
        providerDataType: (string?) null,
        length: (int?) null,
        precision: (int?) null,
        scale: (int?) null,
        checkExpression: (string?)null,
        defaultExpression: (string?)null,
        isNullable: false /* default */,
        isPrimaryKey: false /* default */,
        isAutoIncrement: false /* default */,
        isUnique: false /* default */,
        isIndexed: false /* default */,
        isForeignKey: true,
        referencedTableName: (string?) "app_managers",
        referencedColumnName: (string?) "id",
        onDelete: (DmForeignKeyAction?) DmForeignKeyAction.Cascade,
        onUpdate: (DmForeignKeyAction?) DmForeignKeyAction.NoAction,
        ...
    );

// GET: Retrieve table column names
List<string> names = await db.GetColumnNamesAsync("app", "app_employees", "*title*", ...);

// GET: Retrieve table columns
List<DmTable> tables = await db.GetColumnsAsync("app", "app_employees", "*title*", ...);

// GET: Retrieve single table column
DmColumn? column = await db.GetColumnAsync("app", "app_employees", "title", ...);

// DROP: Drop a table column
bool dropped = await db.DropColumnIfExistsAsync("app", "app_employees", "title", ...);

// RENAME: Rename a table column
bool renamed = await db.RenameColumnIfExistsAsync("app", "app_employees", "title", /* new name */ "job_title", ...);
```
