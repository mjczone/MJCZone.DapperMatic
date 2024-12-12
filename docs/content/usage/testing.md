# Testing

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
