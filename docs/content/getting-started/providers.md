# Providers

The following providers are currently supported. Unit tested versions are in parenthesis.

- [x] SQLite (v3)
- [x] MySQL (v5.7, 8.4)
- [x] MariaDB (v10.11)
- [x] PostgreSQL (v15, v16)
- [x] SQL Server (v2017, v2019, v2022)
- [ ] Oracle
- [ ] IBM DB2

## Register a custom provider

First override the `IDatabaseMethods`, and `IDatabaseMethodsFactory` interfaces:

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

## Extend an existing provider

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

See it in action with the [ProfiledPostgreSqlMethodsFactory](https://github.com/mjczone/MJCZone.DapperMatic/tests/MJCZone.DapperMatic.Tests/ProviderTests/PostgreSqlDatabaseMethodsTests.cs#L84) class. Similar factory classes exist for the other providers.

## Provider documentation links

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
