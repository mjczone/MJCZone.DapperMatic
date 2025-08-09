# Installation

## Prerequisites

- .NET 8.0 or later
- A supported database:
  - SQL Server 2019+
  - MySQL 8.0+ / MariaDB 10.5+
  - PostgreSQL 12+
  - SQLite 3.35+

## Install via NuGet

Add the DapperMatic package to your project:

::: code-group
```bash [.NET CLI]
dotnet add package MJCZone.DapperMatic
```

```xml [PackageReference]
<PackageReference Include="MJCZone.DapperMatic" Version="0.1.*" />
```

```powershell [Package Manager]
Install-Package MJCZone.DapperMatic
```
:::

## Database Provider Packages

You'll also need the appropriate database provider package:

::: code-group
```bash [SQL Server]
dotnet add package Microsoft.Data.SqlClient
```

```bash [MySQL]
dotnet add package MySqlConnector
```

```bash [PostgreSQL]
dotnet add package Npgsql
```

```bash [SQLite]
dotnet add package Microsoft.Data.Sqlite
```
:::

## Verify Installation

Create a simple test to verify everything is working:

```csharp
using MJCZone.DapperMatic;
using Microsoft.Data.SqlClient;

var connectionString = "your-connection-string";
using var connection = new SqlConnection(connectionString);

// Get database version
var version = await connection.GetDatabaseVersionAsync();
Console.WriteLine($"Connected to: {version}");
```

## Next Steps

Now that you have DapperMatic installed, check out:
- [Providers](./providers) to learn about database-specific features
- [Extension Methods](/guide/extension-methods/) to explore available operations