
# General methods

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

// Get a provider specific sql type for a specific .NET type 
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
