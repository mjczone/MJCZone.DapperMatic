# MJCZone.DapperMatic

[![.github/workflows/release.yml](https://github.com/mjczone/MJCZone.DapperMatic/actions/workflows/release.yml/badge.svg)](https://github.com/mjczone/MJCZone.DapperMatic/actions/workflows/release.yml)

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)

Additional extensions leveraging Dapper

- [MJCZone.DapperMatic](#dappermatic)
  - [Method Providers](#method-providers)
    - [Built-in Providers](#built-in-providers)
    - [Custom Providers](#custom-providers)
  - [Models](#models)
    - [Model related factory methods](#model-related-factory-methods)
  - [`IDbConnection` CRUD extension methods](#idbconnection-crud-extension-methods)
    - [General methods](#general-methods)
    - [Schema methods](#schema-methods)
    - [Table methods](#table-methods)
    - [Column methods](#column-methods)
    - [Check constraint methods](#check-constraint-methods)
    - [Default constraint methods](#default-constraint-methods)
    - [Foreign Key constraint methods](#foreign-key-constraint-methods)
    - [UniqueConstraint constraint methods](#uniqueconstraint-constraint-methods)
    - [Index constraint methods](#index-constraint-methods)
    - [PrimaryKeyConstraint constraint methods](#primarykeyconstraint-constraint-methods)
    - [View methods](#view-methods)
  - [Testing](#testing)
  - [Reference](#reference)
    - [Provider documentation links](#provider-documentation-links)
    - [Future plans (t.b.d.)](#future-plans-tbd)

## Method Providers

### Built-in Providers

Unit tests against versions in parenthesis.

- [x] SQLite (v3)
- [x] MySQL (v5.7, 8.4)
- [x] MariaDB (v10.11)
- [x] PostgreSQL (v15, v16)
- [x] SQL Server (v2017, v2019, v2022)
- [ ] Oracle
- [ ] IBM DB2

### Custom Providers

To register a custom provider, first override the `IDatabaseMethods`, and `IDatabaseMethodsFactory` interfaces:

```csharp
namespace PestControl.Foundry;

public class CentipedeDbConnection: System.Data.Common.DbConnection
{
    // ...
}

public class CentipedeDbMethods: MJCZone.DapperMatic.Interfaces.IDatabaseMethods
{
    // ...
}

public class CentipedeDbMethodsFactory : MJCZone.DapperMatic.Providers.DatabaseMethodsFactoryBase
{
    public override bool SupportsConnection(IDbConnection db)
        => connection.GetType().Name == nameof(CentipedeDbConnection);

    protected override IDatabaseMethods CreateMethodsCore()
        => new CentipedeDbMethods();
}
```

Then register the provider:

```csharp
DatabaseMethodsProvider.RegisterFactory("CentipedeDb", new PestControl.Foundry.CentipedeDbMethodsFactory());
```

### Extending an existing Provider Factory

You may want to use a library that wraps an existing `IDbConnection` (e.g., ProfiledDbConnection with MiniProfiler). In that case, you can simply extend
a built-in factory and register your new factory implementation with MJCZone.DapperMatic.

Your factory class would like like this.

```csharp
public class ProfiledPostgreSqlMethodsFactory: PostgreSqlMethodsFactory
{
    public override bool SupportsConnectionCustom(IDbConnection db)
    {
        return (db is ProfiledDbConnection pdc) ? base.SupportsConnectionCustom(pdc.InnerConnection): false;
    }
}
```

Then register the factory as follows.

```csharp
DatabaseMethodsProvider.RegisterFactory(
    "ProfiledDbConnection.PostgreSql", new ProfiledPostgreSqlMethodsFactory());
```

The test suite uses this method to profile the database and output sql exception details
to the unit testing logs.

See it in action with the [ProfiledPostgreSqlMethodsFactory](./tests/MJCZone.DapperMatic.Tests/ProviderTests/PostgreSqlDatabaseMethodsTests.cs#L84) class. This factory class demonstrates using a custom `IDbConnection` type `DbQueryLogging.LoggedDbConnection` from the `DbQueryLogging` package. Similar factory classes exist for the other providers.

## Models

- [DmCheckConstraint](src/MJCZone.DapperMatic/Models/DmCheckConstraint.cs)
- [DmColumn](src/MJCZone.DapperMatic/Models/DmColumn.cs)
- [DmColumnOrder](src/MJCZone.DapperMatic/Models/DmColumnOrder.cs)
- [DmConstraint](src/MJCZone.DapperMatic/Models/DmConstraint.cs)
- [DmConstraintType](src/MJCZone.DapperMatic/Models/DmConstraintType.cs)
- [DmDefaultConstraint](src/MJCZone.DapperMatic/Models/DmDefaultConstraint.cs)
- [DmForeignKeyAction](src/MJCZone.DapperMatic/Models/DmForeignKeyAction.cs)
- [DmForeignKeyConstraint](src/MJCZone.DapperMatic/Models/DmForeignKeyConstraint.cs)
- [DmIndex](src/MJCZone.DapperMatic/Models/DmIndex.cs)
- [DmOrderedColumn](src/MJCZone.DapperMatic/Models/DmOrderedColumn.cs)
- [DmPrimaryKeyConstraint](src/MJCZone.DapperMatic/Models/DmPrimaryKeyConstraint.cs)
- [DmTable](src/MJCZone.DapperMatic/Models/DmTable.cs)
- [DmUniqueConstraint](src/MJCZone.DapperMatic/Models/DmUniqueConstraint.cs)
- [DmView](src/MJCZone.DapperMatic/Models/DmView.cs)

### Model related factory methods

- [DmTableFactory](src/MJCZone.DapperMatic/Models/DmTableFactory.cs)

```cs
DmTable table = DmTableFactory.GetTable(typeof(app_employees))
```

- [DmViewFactory](src/MJCZone.DapperMatic/Models/DmViewFactory.cs)

```cs
DmView view = DmViewFactory.GetView(typeof(vw_onboarded_employees))
```

## `IDbConnection` CRUD extension methods

All methods are async and support an optional transaction (recommended), and cancellation token.

### About `Schemas`

The schema name parameter is nullable in all methods, as many database providers don't support schemas (e.g., SQLite and MySql). If a database supports schemas, and the schema name passed in is `null` or an empty string, then a default schema name is used for that database provider.

The following default schema names apply:

- SqLite: "" (empty string)
- MySql: "" (empty string)
- PostgreSql: "public"
- SqlServer: "dbo"

### General methods

```cs
using var db = await connectionFactory.OpenConnectionAsync();
using var tx = db.BeginTransaction();

// Get the version of the database (e.g., 3.46.1 for a SQLite database)
Version version = await db.GetDatabaseVersionAsync(tx, cancellationToken).ConfigureAwait(false)

// Get a .NET type descriptor for a provider specific sql type
DbProviderDotnetTypeDescriptor descriptor = db.GetDotnetTypeFromSqlType("nvarchar(255)");
// descriptor.AutoIncrement -> False
// descriptor.DotnetType -> typeof (String)
// descriptor.Length -> 255
// descriptor.Precision -> null
// descriptor.Scale -> null
// descriptor.Unicode -> True

// Get a .NET type descriptor for a provider specific sql type
string sqlType = db.GetSqlTypeFromDotnetType(new DbProviderDotnetTypeDescriptor(typeof(string), 47, unicode: true));
// sqlType => nvarchar(47)

// Get the mapped .NET type matching a specific provider sql data type (e.g., varchar(255), decimal(15,4))
var (/* Type */ dotnetType, /* int? */ length, /* int? */ precision, /* int? */ scale) = db.GetDotnetTypeFromSqlType(string sqlType);

// Normalize a database name identifier to some idiomatic standard, namely alpha numeric with underscores and without spaces
var normalizedName = db.NormalizeName(name);

// Get the last sql executed inside MJCZone.DapperMatic
var lastSql = db.GetLastSql();
(string sql, object? parameters) lastSqlWithParams = db.GetLastSqlWithParms();
```

### Schema methods

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

### Table methods

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

// TRUNCATE: Drop a database table
bool truncated = await db.TruncateTableIfExistsAsync("app", "app_employees", ...);
```

### Column methods

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

### Check constraint methods

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

### Default constraint methods

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

### Foreign Key constraint methods

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

### UniqueConstraint constraint methods

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

### Index constraint methods

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

### PrimaryKeyConstraint constraint methods

```cs
using var db = await connectionFactory.OpenConnectionAsync();
using var tx = db.BeginTransaction();

var primaryKeyConstraintName = ProviderUtils.GeneratePrimaryKeyConstraintName("app_employees", "email");

// EXISTS: Check to see if a primary key constraint exists
bool exists = await db.DoesPrimaryKeyConstraintExistAsync("app","app_employees", tx, cancellationToken).ConfigureAwait(false)

// CREATE: Create a primary key constraint
bool created = await db.CreatePrimaryKeyConstraintIfNotExistsAsync("app", /* DmPrimaryKeyConstraint */ primaryKeyConstraint, ...);
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
DmPrimaryKeyConstraint? primaryKeyConstraint = await db.GetPrimaryKeyConstraintAsync("app", "app_employees", ...);

// DROP: Drop a primary key constraint
bool dropped = await db.DropPrimaryKeyConstraintIfExistsAsync("app", "app_employees", primaryKeyConstraintName, ...);
```

### View methods

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

## Testing

The testing methodology consists of using the following very handy `Testcontainers.*` nuget library packages.
Tests are executed on Linux, and can be run on WSL during development.

```xml
    <PackageReference Include="Testcontainers.MsSql" Version="3.9.0" />
    <PackageReference Include="Testcontainers.MySql" Version="3.9.0" />
    <PackageReference Include="Testcontainers.PostgreSql" Version="3.9.0" />
    <PackageReference Include="Testcontainers.Redis" Version="3.9.0" />
```

The exact same tests are run for each database provider, ensuring consistent behavior across all providers.

The tests leverage docker containers for each supported database version (created and disposed of automatically at runtime).
The local file system is used for SQLite.

## Reference

### Provider documentation links

The extension methods and operation implementations are derived from the SQL documentation residing at the following links:

- MySQL
  - MySQL 8.4: <https://dev.mysql.com/doc/refman/8.4/en/sql-data-definition-statements.html>
  - MySQL 5.7: <https://dev.mysql.com/doc/refman/5.7/en/sql-data-definition-statements.html>
- MariaDB
  - MariaDB 10.11: <https://mariadb.com/kb/en/data-definition/>
- PostgreSQL
  - PostgreSQL 16: <https://www.postgresql.org/docs/16/ddl.html>
  - PostgreSQL 15: <https://www.postgresql.org/docs/15/ddl.html>
- SQLite
  - SQLite (v3): <https://www.sqlite.org/lang.html>
- SQL Server
  - SQL Server 2022: <https://learn.microsoft.com/en-us/sql/t-sql/statements/statements?view=sql-server-ver16#data-definition-language>
  - SQL Server 2019: <https://learn.microsoft.com/en-us/sql/t-sql/statements/statements?view=sql-server-ver15#data-definition-language>
  - SQL Server 2017: <https://learn.microsoft.com/en-us/sql/t-sql/statements/statements?view=sql-server-2017#data-definition-language>

### Future plans (t.b.d.)

- Add per-method provider `Option` parameters for greater coverage of provider-specific capabilities and nuances.
- Improve on some of the convention-based design decisions, and support a fluent configuration syntax for handling types and data type mappings.
- Add database CRUD methods
- Add account management CRUD methods (for users and roles)
