# Unique Constraint constraint methods

```cs
using var db = await connectionFactory.OpenConnectionAsync();
using var tx = db.BeginTransaction();

var uniqueConstraintName = ProviderUtils.GenerateUniqueConstraintName("app_employees", "email");

// EXISTS: Check to see if a unique constraint exists
bool exists = await db.DoesUniqueConstraintExistAsync("app","app_employees", uniqueConstraintName, tx, cancellationToken).ConfigureAwait(false)

// EXISTS: Check to see if a unique constraint exists on a column
exists = await db.DoesUniqueConstraintExistOnColumnAsync("app","app_employees", "email", tx, cancellationToken).ConfigureAwait(false)

// CREATE: Create a unique constraint
bool created = await db.CreateUniqueConstraintIfNotExistsAsync("app", /* DmUniqueConstraint */ uniqueConstraint, ...);
// or
    created = await db.CreateUniqueConstraintIfNotExistsAsync(
        "app",
        "app_employees",
        // string uniqueConstraintName,
        uniqueConstraintName,
        // DmOrderedColumn[] columns,
        [ new DmOrderedColumn("email", DmColumnOrder.Descending) ],
        ...
    );

// GET: Retrieve unique constraint names
List<string> names = await db.GetUniqueConstraintNamesAsync("app", "app_employees", "uc_*", ...);

// GET: Retrieve uniqueConstraint names on column
names = await db.GetUniqueConstraintNamesOnColumnAsync("app", "app_employees", "email", ...);

// GET: Retrieve uniqueConstraints
List<DmUniqueConstraint> uniqueConstraints = await db.GetUniqueConstraintsAsync("app", "app_employees", "uc_*", ...);

// GET: Retrieve single unique constraint
DmUniqueConstraint? uniqueConstraint = await db.GetUniqueConstraintAsync("app", "app_employees", uniqueConstraintName, ...);

// GET: Retrieve single unique constraint on column
uniqueConstraint = await db.GetUniqueConstraintOnColumnAsync("app", "app_employees", "email", ...);

// DROP: Drop an unique constraint
bool dropped = await db.DropUniqueConstraintIfExistsAsync("app", "app_employees", uniqueConstraintName, ...);

// DROP: Drop unique constraints on column
dropped = await db.DropUniqueConstraintsOnColumnIfExistsAsync("app", "app_employees", "email", ...);
```
