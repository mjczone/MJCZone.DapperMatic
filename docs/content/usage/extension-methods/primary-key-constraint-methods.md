# PrimaryKeyConstraint constraint methods

```cs
using var db = await connectionFactory.OpenConnectionAsync();
using var tx = db.BeginTransaction();

var primaryKeyConstraintName = 
    ProviderUtils.GeneratePrimaryKeyConstraintName("app_employees", "email");

// EXISTS: Check to see if a primary key constraint exists
bool exists = await db.DoesPrimaryKeyConstraintExistAsync("app","app_employees", 
    tx, cancellationToken).ConfigureAwait(false)

// CREATE: Create a primary key constraint
bool created = await db.CreatePrimaryKeyConstraintIfNotExistsAsync("app", 
    /* DmPrimaryKeyConstraint */ primaryKeyConstraint, ...);
// or
    created = await db.CreatePrimaryKeyConstraintIfNotExistsAsync(
        "app",
        "app_employees",
        // string primaryKeyConstraintName,
        primaryKeyConstraintName,
        // DmOrderedColumn[] columns,
        [ new DmOrderedColumn("email", DmColumnOrder.Descending) ],
        ...
    );

// GET: Retrieve single primary key constraint
DmPrimaryKeyConstraint? primaryKeyConstraint = 
    await db.GetPrimaryKeyConstraintAsync("app", "app_employees", ...);

// DROP: Drop a primary key constraint
bool dropped = 
    await db.DropPrimaryKeyConstraintIfExistsAsync("app", "app_employees", primaryKeyConstraintName, ...);
```
