# Check constraint methods

```cs
using var db = await connectionFactory.OpenConnectionAsync();
using var tx = db.BeginTransaction();

var constraintName = ProviderUtils.GenerateCheckConstraintName("app_employees", "age");

// EXISTS: Check to see if a check constraint exists
bool exists = await db.DoesCheckConstraintExistAsync("app","app_employees", constraintName, tx, cancellationToken).ConfigureAwait(false)

// EXISTS: Check to see if a check constraint exists on a column
exists = await db.DoesCheckConstraintExistOnColumnAsync("app","app_employees", "age", tx, cancellationToken).ConfigureAwait(false)

// CREATE: Create a check constraint
bool created = await db.CreateCheckConstraintIfNotExistsAsync("app", /* DmCheckConstraint */ checkConstraint);
// or
    created = await db.CreateCheckConstraintIfNotExistsAsync(
        "app",
        "app_employees",
        // string? columnName,
        "age",
        // string constraintName,
        constraintName,
        // string expression,
        "age > 21",
        ...
    );

// GET: Retrieve check constraints
List<string> names = await db.GetCheckConstraintNamesAsync("app", "app_employees", "ck_*", ...);

string name = await db.GetCheckConstraintNameOnColumnAsync("app", "app_employees", "age", ...);

// GET: Retrieve check constraints
List<DmCheckConstraint> checkConstraints = await db.GetCheckConstraintsAsync("app", "app_employees", "ck_*", ...);

// GET: Retrieve single check constraint
DmCheckConstraint? checkConstraint = await db.GetCheckConstraintAsync("app", "app_employees", constraintName, ...);

// GET: Retrieve single check constraint on column
checkConstraint = await db.GetCheckConstraintOnColumnAsync("app", "app_employees", "age", ...);

// DROP: Drop a check constraint
bool dropped = await db.DropCheckConstraintIfExistsAsync("app", "app_employees", constraintName, ...);

// DROP: Drop a check constraint on column
dropped = await db.DropCheckConstraintOnColumnIfExistsAsync("app", "app_employees", "age", ...);
```
