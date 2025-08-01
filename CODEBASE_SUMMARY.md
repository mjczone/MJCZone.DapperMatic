# MJCZone.DapperMatic Codebase Summary

## Repository Goal
A C# library providing IDbConnection extension methods for DDL (Data Definition Language) operations, leveraging Dapper to provide consistent database schema management across multiple database providers. A key design goal is automatic dialect discovery - users don't need to specify the database type as the framework auto-detects it from the IDbConnection type and applies the appropriate SQL dialect automatically.

## Source (src folder)

### Core Architecture
- **Extension Methods Pattern**: All functionality exposed as IDbConnection extension methods via partial classes (`DbConnectionExtensions_*.cs`)
- **Provider Pattern**: Supports SQL Server, MySQL/MariaDB, PostgreSQL, and SQLite through provider-specific implementations
- **Model-First Approach**: Uses `Dm*` prefix models (DmTable, DmColumn, etc.) to represent database objects abstractly

### Key Components
1. **Models** (`Models/`): POCOs representing database objects (tables, columns, constraints, indexes, views)
2. **Providers** (`Providers/`): Database-specific implementations inheriting from `DatabaseMethodsBase`
3. **Data Annotations** (`DataAnnotations/`): Attributes for decorating C# classes to define schema
4. **Type Converters** (`Converters/`): Bidirectional conversion between .NET types and SQL types

### Provider Strategy
- Factory pattern (`DatabaseMethodsProvider`) automatically detects database type from connection
- Each provider implements database-specific SQL generation while maintaining consistent API
- Type mapping handled per-provider via `IDbProviderTypeMap` implementations

## Documentation (docs folder)

### Structure
- Vue.js-based documentation site with markdown content
- Comprehensive method documentation for each extension category
- Getting started guides covering installation, configuration, models, and providers

### Coverage
- Extension methods documented by category (tables, columns, constraints, indexes, views, schemas)
- Each method category has dedicated documentation page
- Minimal README.md relies on external documentation site

## Tests (tests folder)

### Testing Approach
- **xUnit** test framework with `ITestOutputHelper` for logging
- **Database Fixtures**: Separate fixture classes for each provider managing test database connections
- **Inheritance Pattern**: Provider-specific test classes inherit from `DatabaseMethodsTests` base class
- **Comprehensive Coverage**: Tests for all DDL operations across all supported providers

### Test Organization
- Base test methods in `DatabaseMethodsTests.*.cs` files (organized by feature)
- Provider-specific test classes override when needed for provider-specific behavior
- Real database connections used for integration testing
- Test data includes complex scenarios (foreign keys, constraints, indexes)

### Key Testing Features
- Schema isolation for parallel test execution
- Type mapping validation across providers
- Constraint and relationship testing
- View creation and management testing