# Usage

## `IDbConnection` CRUD extension methods

All methods are async and support an optional transaction (recommended), and cancellation token.

## About `Schemas`

The schema name parameter is nullable in all methods, as many database providers don't support schemas (e.g., SQLite and MySql). If a database supports schemas, and the schema name passed in is `null` or an empty string, then a default schema name is used for that database provider.

The following default schema names apply:

- SqLite: "" (empty string)
- MySql: "" (empty string)
- PostgreSql: "public"
- SqlServer: "dbo"

## Explore

- [Extension Methods](#/usage/extension-methods)
