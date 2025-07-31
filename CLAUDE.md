# Claude's Notes

## Folders to Ignore

- `_delete/` - Should be ignored if encountered
- `__delete/` - Should be ignored if encountered
- `___delete/` - Should be ignored if encountered

These folders contain outdated implementations and are not part of the active codebase.

## Project Context

This is MJCZone.DapperMatic - a C# library providing IDbConnection extension methods for DDL operations across multiple database providers (SQL Server, MySQL/MariaDB, PostgreSQL, SQLite).

## Key Patterns

- Extension methods pattern via `DbConnectionExtensions_*.cs` files
- Provider pattern with database-specific implementations
- Model-first approach using `Dm*` prefixed models
- Real database integration tests for each provider

## Recent Improvements

- **Target Framework Simplification (2025-01-31)**: Simplified to target only .NET 8.0
  - Changed from `net8.0;net9.0` to `net8.0` only
  - Reduces maintenance burden while maintaining LTS support until November 2026
  - Broader compatibility (works with both .NET 8 and .NET 9 applications)
  - Future ASP.NET Core package will handle framework-specific web concerns

- **Auto-increment Detection (2025-01-31)**: Standardized auto-increment detection across all providers
  - Added `DatabaseMethodsBase.DetermineIsAutoIncrement()` method
  - Provider-specific overrides for metadata handling
  - Unified detection strategy covering SQL type names, provider metadata, and type descriptors
