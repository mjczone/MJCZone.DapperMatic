# DapperMatic

[![.github/workflows/release.yml](https://github.com/mjczone/DapperMatic/actions/workflows/release.yml/badge.svg)](https://github.com/mjczone/DapperMatic/actions/workflows/release.yml)

Additional extensions leveraging Dapper

## Features

### `IDbConnection` extension methods

The following table outlines the various extension methods available for `IDbConnection` instances. (WIP)

| Method Name                              | Description                                                                                       |
|------------------------------------------|---------------------------------------------------------------------------------------------------|

| **Database Methods** | |
| `GetDatabaseVersionAsync`                | Retrieves the version of the database.                                                            |
| **Schema Methods** | |
| `SupportsSchemasAsync`                   | Checks if the database supports schemas.                                                          |
| `DoesSchemaExistAsync`                   | Checks if a schema exists in the database.                                                        |
| `CreateSchemaIfNotExistsAsync`           | Creates a schema if it does not already exist in the database.                                    |
| `GetSchemaNamesAsync`                    | Retrieves the names of schemas in the database.                                                   |
| `DropSchemaIfExistsAsync`                | Drops a schema if it exists in the database.                                                      |
| `RenameSchemaIfExistsAsync`              | Renames a schema if it exists in the database.                                                    |
| **Table Methods**                        |                                                                                                   |
| `DoesTableExistAsync`                    | Checks if a table exists in the database.                                                         |
| `CreateTableIfNotExistsAsync`            | Creates a table if it does not already exist, with optional primary key column names, types, and lengths. |
| `GetTableNamesAsync`                     | Retrieves the names of tables in the database, optionally filtered by a table name filter.        |
| `GetTablesAsync`                         | Retrieves the tables in the database, optionally filtered by a table name filter.                 |
| `GetTableAsync`                          | Retrieves a table in the database.                                                                |
| `DropTableIfExistsAsync`                 | Drops a table if it exists in the database.                                                       |
| `RenameTableIfExistsAsync`               | Renames a table if it exists in the database.                                                     |
| **View Methods**                         |                                                                                                   |
| `GetViewNamesAsync`                      | Retrieves the names of views in the database, optionally filtered by a view name filter.          |
| `DropViewIfExistsAsync`                  | Drops a view if it exists in the database.                                                        |
| `RenameViewIfExistsAsync`                | Renames a view if it exists in the database.                                                      |
| `DoesViewExistAsync`                     | Checks if a view exists in the database.                                                          |
| **Column Methods**                       |                                                                                                   |
| `GetColumnNamesAsync`                    | Retrieves the names of columns in a specified table.                                              |
| `AddColumnAsync`                         | Adds a column to a specified table.                                                               |
| `DropColumnIfExistsAsync`                | Drops a column if it exists in a specified table.                                                 |
| `RenameColumnIfExistsAsync`              | Renames a column if it exists in a specified table.                                               |
| `DoesColumnExistAsync`                   | Checks if a column exists in a specified table.                                                   |
| **Index Methods**                        |                                                                                                   |
| `GetIndexNamesAsync`                     | Retrieves the names of indexes in a specified table.                                              |
| `CreateIndexIfNotExistsAsync`            | Creates an index if it does not already exist on a specified table.                               |
| `DropIndexIfExistsAsync`                 | Drops an index if it exists on a specified table.                                                 |
| `RenameIndexIfExistsAsync`               | Renames an index if it exists on a specified table.                                               |
| `DoesIndexExistAsync`                    | Checks if an index exists on a specified table.                                                   |
| **Foreign Key Constraint Methods**       |                                                                                                   |
| `GetForeignKeyNamesAsync`                | Retrieves the names of foreign keys in a specified table.                                         |
| `GetForeignKeyConstraintOnColumnAsync`   | Retrieves the foreign key constraint on a specified column.                                       |
| `CreateForeignKeyConstraintIfNotExistsAsync` | Creates a foreign key constraint if it does not already exist on a specified table.               |
| `DropForeignKeyConstraintIfExistsAsync`  | Drops a foreign key constraint if it exists on a specified table.                                 |
| `RenameForeignKeyConstraintIfExistsAsync`| Renames a foreign key constraint if it exists on a specified table.                               |
| `DoesForeignKeyConstraintExistAsync`     | Checks if a foreign key constraint exists on a specified table.                                   |
| **Primary Key Constraint Methods**       |                                                                                                   |
| `GetPrimaryKeyNamesAsync`                | Retrieves the names of primary keys in a specified table.                                         |
| `CreatePrimaryKeyConstraintIfNotExistsAsync` | Creates a primary key constraint if it does not already exist on a specified table.               |
| `DropPrimaryKeyConstraintIfExistsAsync`  | Drops a primary key constraint if it exists on a specified table.                                 |
| `RenamePrimaryKeyConstraintIfExistsAsync`| Renames a primary key constraint if it exists on a specified table.                               |
| `DoesPrimaryKeyConstraintExistAsync`     | Checks if a primary key constraint exists on a specified table.                                   |
| **Unique Constraint Methods**            |                                                                                                   |
| `GetUniqueConstraintNamesAsync`          | Retrieves the names of unique constraints in a specified table.                                   |
| `CreateUniqueConstraintIfNotExistsAsync` | Creates a unique constraint if it does not already exist on a specified table.                    |
| `DropUniqueConstraintIfExistsAsync`      | Drops a unique constraint if it exists on a specified table.                                      |
| `RenameUniqueConstraintIfExistsAsync`    | Renames a unique constraint if it exists on a specified table.                                    |
| `DoesUniqueConstraintExistAsync`         | Checks if a unique constraint exists on a specified table.                                        |
| **Check Constraint Methods**             |                                                                                                   |
| `GetCheckConstraintNamesAsync`           | Retrieves the names of check constraints in a specified table.                                    |
| `CreateCheckConstraintIfNotExistsAsync`  | Creates a check constraint if it does not already exist on a specified table.                     |
| `DropCheckConstraintIfExistsAsync`       | Drops a check constraint if it exists on a specified table.                                       |
| `RenameCheckConstraintIfExistsAsync`     | Renames a check constraint if it exists on a specified table.                                     |
| `DoesCheckConstraintExistAsync`          | Checks if a check constraint exists on a specified table.

## Implementation details

The extension methods and operation implementations are derived from the SQL documentation residing at the following links:

- MySQL 8.4: <https://dev.mysql.com/doc/refman/8.4/en/sql-data-definition-statements.html>
- MySQL 5.7: <https://dev.mysql.com/doc/refman/5.7/en/sql-data-definition-statements.html>
- PostgreSQL 16: <https://www.postgresql.org/docs/16/ddl.html>
- PostgreSQL 15: <https://www.postgresql.org/docs/15/ddl.html>
- PostgreSQL 14: <https://www.postgresql.org/docs/14/ddl.html>
- PostgreSQL 13: <https://www.postgresql.org/docs/13/ddl.html>
- SQLite (v3): <https://www.sqlite.org/lang.html>
- SQL Server 2022: <https://learn.microsoft.com/en-us/sql/t-sql/statements/statements?view=sql-server-ver16#data-definition-language>
- SQL Server 2019: <https://learn.microsoft.com/en-us/sql/t-sql/statements/statements?view=sql-server-ver15#data-definition-language>
- SQL Server 2017: <https://learn.microsoft.com/en-us/sql/t-sql/statements/statements?view=sql-server-2017#data-definition-language>

## Testing

The testing methodology consists of using the following very handy `Testcontainer` nuget library packages.

```xml
    <PackageReference Include="Testcontainers.MsSql" Version="3.9.0" />
    <PackageReference Include="Testcontainers.MySql" Version="3.9.0" />
    <PackageReference Include="Testcontainers.PostgreSql" Version="3.9.0" />
    <PackageReference Include="Testcontainers.Redis" Version="3.9.0" />
```

The exact same tests are run for each database provider, ensuring consistent behavior across all providers.

The tests leverage docker containers for each supported database version (created and disposed of automatically thanks to the `Testcontainers` libraries).
The local file system is used for SQLite.
