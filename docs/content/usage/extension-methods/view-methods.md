# View methods

```cs
using var db = await connectionFactory.OpenConnectionAsync();
using var tx = db.BeginTransaction();

var viewName = "vw_employees_not_yet_onboarded";

// EXISTS: Check to see if a view exists
bool exists = await db.DoesViewExistAsync("app", viewName, tx, cancellationToken).ConfigureAwait(false)

// CREATE: Create a view
bool created = await db.CreateViewIfNotExistsAsync("app", /* DmView */ view, ...);
// or
    created = await db.CreateViewIfNotExistsAsync(
        "app",
        // string viewName,
        viewName,
        // string viewDefinition,
        "SELECT * FROM app_employees WHERE is_onboarded = 0",
        ...
    );

// UPDATE: Update a view
bool updated = await db.CreateViewIfNotExistsAsync(
    "app",
    // string viewName,
    viewName,
    // string viewDefinition,
    "SELECT * FROM app_employees WHERE is_onboarded = 0 and employment_date_ended is null",
    ...
);

// GET: Retrieve view names
List<string> viewNames = await db.GetViewNames("app", "vw_*", ...);

// GET: Retrieve single view
List<DmView> views = await db.GetViewsAsync("app", "vw_*", ...);

// GET: Retrieve single view
DmView? view = await db.GetViewAsync("app", viewName, ...);

// DROP: Drop a view
bool dropped = await db.DropViewIfExistsAsync("app", viewName, ...);

// RENAME: Rename a view
bool dropped = await db.RenameViewIfExistsAsync(
    "app",
    // string viewName,
    "vw_employees_not_yet_onboarded",
    // string newViewName,
    "vw_current_employees_not_yet_onboarded",
    ...
);
```
