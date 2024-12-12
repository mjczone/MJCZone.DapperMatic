# Default constraint methods

```cs
using var db = await connectionFactory.OpenConnectionAsync();
using var tx = db.BeginTransaction();

var constraintName = ProviderUtils.GenerateDefaultConstraintName("app_employees", "age");

// EXISTS: Check to see if a default constraint exists
bool exists = await db.DoesDefaultConstraintExistAsync("app","app_employees", constraintName, tx, cancellationToken).ConfigureAwait(false)

// EXISTS: Check to see if a default constraint exists on a column
exists = await db.DoesDefaultConstraintExistOnColumnAsync("app","app_employees", "age", tx, cancellationToken).ConfigureAwait(false)

// CREATE: Create a default constraint
bool created = await db.CreateDefaultConstraintIfNotExistsAsync("app", /* DmDefaultConstraint */ defaultConstraint);
// or
    created = await db.CreateDefaultConstraintIfNotExistsAsync(
        "app",
        "app_employees",
        // string? columnName,
        "age",
        // string constraintName,
        constraintName,
        // string expression,
        "-1",
        ...
    );

// GET: Retrieve default constraints
List<string> names = await db.GetDefaultConstraintNamesAsync("app", "app_employees", "df_*", ...);

string name = await db.GetDefaultConstraintNameOnColumnAsync("app", "app_employees", "age", ...);

// GET: Retrieve default constraints
List<DmDefaultConstraint> defaultConstraints = await db.GetDefaultConstraintsAsync("app", "app_employees", "df*", ...);

// GET: Retrieve single default constraint
DmDefaultConstraint? defaultConstraint = await db.GetDefaultConstraintAsync("app", "app_employees", constraintName, ...);

// GET: Retrieve single default constraint on column
defaultConstraint = await db.GetDefaultConstraintOnColumnAsync("app", "app_employees", "age", ...);

// DROP: Drop a default constraint
bool dropped = await db.DropDefaultConstraintIfExistsAsync("app", "app_employees", constraintName, ...);

// DROP: Drop a default constraint on column
dropped = await db.DropDefaultConstraintOnColumnIfExistsAsync("app", "app_employees", "age", ...);
```
