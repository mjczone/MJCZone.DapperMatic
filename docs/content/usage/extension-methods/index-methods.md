# Index methods

```cs
using var db = await connectionFactory.OpenConnectionAsync();
using var tx = db.BeginTransaction();

var indexName = ProviderUtils.GenerateIndexName("app_employees", "is_onboarded");

// EXISTS: Check to see if a index exists
bool exists = await db.DoesIndexExistAsync("app","app_employees", indexName, tx, cancellationToken).ConfigureAwait(false)

// EXISTS: Check to see if a index exists on a column
exists = await db.DoesIndexExistOnColumnAsync("app","app_employees", "is_onboarded", tx, cancellationToken).ConfigureAwait(false)

// CREATE: Create a index
bool created = await db.CreateIndexIfNotExistsAsync("app", /* DmIndex */ index);
// or
    created = await db.CreateIndexIfNotExistsAsync(
        "app",
        "app_employees",
        // string indexName,
        indexName,
        // DmOrderedColumn[] columns,
        [ new DmOrderedColumn("is_onboarded", DmColumnOrder.Descending) ],
        isUnique: false,
        ...
    );

// GET: Retrieve index names
List<string> names = await db.GetIndexNamesAsync("app", "app_employees", "ix_*", ...);

// GET: Retrieve index names on column
names = await db.GetIndexNamesOnColumnAsync("app", "app_employees", "is_onboarded", ...);

// GET: Retrieve indexes
List<DmIndex> indexes = await db.GetIndexesAsync("app", "app_employees", "ix_*", ...);

// GET: Retrieve single index
DmIndex? index = await db.GetIndexAsync("app", "app_employees", indexName, ...);

// GET: Retrieve single index on column
index = await db.GetIndexOnColumnAsync("app", "app_employees", "is_onboarded", ...);

// DROP: Drop an index
bool dropped = await db.DropIndexIfExistsAsync("app", "app_employees", indexName, ...);

// DROP: Drop indexes on column
dropped = await db.DropIndexesOnColumnIfExistsAsync("app", "app_employees", "is_onboarded", ...);
```
