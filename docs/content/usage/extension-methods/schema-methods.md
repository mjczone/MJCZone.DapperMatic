# Schema methods

```cs
using var db = await connectionFactory.OpenConnectionAsync();
using var tx = db.BeginTransaction();

// Check to see if the database supports schemas
var supportsSchemas = db.SupportsSchemas();

// EXISTS: Check to see if a database schema exists
bool exists = await db.DoesSchemaExistAsync("app", tx, cancellationToken).ConfigureAwait(false)

// CREATE: Create a database schema
bool created = await db.CreateSchemaIfNotExistsAsync("app", ...);

// GET: Retrieve database schema names
List<string> names = await db.GetSchemaNamesAsync("*ap*", ...);

// DROP: Drop a database schema
bool dropped = await db.DropSchemaIfExistsAsync("app", ...)
```
