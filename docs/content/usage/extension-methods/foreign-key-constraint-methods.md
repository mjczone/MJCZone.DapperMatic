# Foreign Key constraint methods

```cs
using var db = await connectionFactory.OpenConnectionAsync();
using var tx = db.BeginTransaction();

var constraintName = ProviderUtils.GenerateForeignKeyConstraintName("app_employees", "manager_id", "app_managers", "id");

// EXISTS: Check to see if a foreign key exists
bool exists = await db.DoesForeignKeyConstraintExistAsync("app","app_employees", constraintName, tx, cancellationToken).ConfigureAwait(false)

// EXISTS: Check to see if a foreign key exists on a column
exists = await db.DoesForeignKeyConstraintExistOnColumnAsync("app","app_employees", "manager_id", tx, cancellationToken).ConfigureAwait(false)

// CREATE: Create a foreign key
bool created = await db.CreateForeignKeyConstraintIfNotExistsAsync("app", /* DmForeignKeyConstraint */ foreignKeyConstraint);
// or
    created = await db.CreateForeignKeyConstraintIfNotExistsAsync(
        "app",
        "app_employees",
        // string constraintName,
        constraintName,
        // DmOrderedColumn[] sourceColumns,
        [ new DmOrderedColumn("manager_id") ]
        // string referencedTableName,
        "app_managers",
        // DmOrderedColumn[] referencedColumns,
        [ new DmOrderedColumn("id") ],
        onDelete: DmForeignKeyAction.Cascade,
        onUpdate: DmForeignKeyAction.NoAction,
        ...
    );

// GET: Retrieve foreign key names
List<string> names = await db.GetForeignKeyConstraintNamesAsync("app", "app_employees", "fk_*", ...);

// GET: Retrieve foreign key name on column
string name = await db.GetForeignKeyConstraintNameOnColumnAsync("app", "app_employees", "manager_id", ...);

// GET: Retrieve foreign keys
List<DmForeignKeyConstraint> foreignKeyConstraints = await db.GetForeignKeyConstraintsAsync("app", "app_employees", "fk_*", ...);

// GET: Retrieve single foreign key
DmForeignKeyConstraint? foreignKeyConstraint = await db.GetForeignKeyConstraintAsync("app", "app_employees", constraintName, ...);

// GET: Retrieve single foreign key on column
foreignKeyConstraint = await db.GetForeignKeyConstraintOnColumnAsync("app", "app_employees", "manager_id", ...);

// DROP: Drop a foreign key
bool dropped = await db.DropForeignKeyConstraintIfExistsAsync("app", "app_employees", constraintName, ...);

// DROP: Drop a foreign key on column
dropped = await db.DropForeignKeyConstraintOnColumnIfExistsAsync("app", "app_employees", "age", ...);
```
