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

- **Type Mapping Consolidation - Step 1 (2025-01-31)**: Created foundation for unified type mapping
  - Added `TypeMappingDefaults.cs` with shared constants (string lengths, precision/scale defaults)
  - Added `TypeMappingHelpers.cs` with 14 helper methods for consistent type creation
  - Comprehensive test coverage with 33 test methods
  - Foundation supports: decimal types, string types, GUID/enum storage, geometry types, LOB types, arrays, datetime precision, and type inspection utilities

- **Dapper Dependency Elimination (2025-08-01)**: Planned major refactoring to remove Dapper dependency
  - Analysis shows minimal Dapper usage: only 3 methods (`QueryAsync<T>`, `ExecuteScalarAsync<T>`, `ExecuteAsync`)
  - Benefits: Reduced dependencies, smaller package size (~200KB savings), better control, potential performance gains
  - Implementation: Replace abstract methods in `DatabaseMethodsBase.cs` with raw ADO.NET implementations
  - Maintains existing provider pattern and extension method API
  - Estimated effort: 50-100 lines of replacement code

## Code Style Guidelines

- Use trailing comma in multi-line initializers

## Major Refactoring Projects

### Type Mapping Consolidation (Completed 2025-01-31)

**Goal**: Eliminate ~3000+ lines of duplicated type mapping code across providers while maintaining consistency.

**Status**: ✅ **COMPLETED** - All 491 tests passing across all database providers.

**Progress Tracking:**
- ✅ **Step 1**: Constants & Helpers - Create shared defaults and helper methods
- ✅ **Step 2**: Refactor SQL Server Provider - Use new helpers in existing provider  
- ✅ **Step 3**: Refactor Other Providers - Apply changes to MySQL, PostgreSQL, SQLite
- ✅ **Step 4**: Extract Base Converters - Move common conversion logic to base classes
- ✅ **Step 5**: Standardize Geometry - Unify NetTopologySuite handling across providers
- ✅ **Step 6**: Unify JSON Handling - Standardize JSON type strategies  
- ✅ **Step 7**: Native Array Support - Implement array handling where available (PostgreSQL)
- ✅ **Step 8**: Fix MySQL datetime precision issue (default to precision 6)
- ✅ **Step 9**: Consistency Testing - Add comprehensive type mapping tests

### Dapper Dependency Elimination (Planned)

**Goal**: Remove Dapper dependency to reduce external dependencies and package size while maintaining API compatibility.

**Status**: 📋 **PLANNED** - Analysis complete, ready for implementation.

**Implementation Plan:**
1. Replace 3 abstract methods in `DatabaseMethodsBase.cs` with raw ADO.NET implementations
2. Maintain existing provider pattern and extension method API
3. Preserve all current functionality and test compatibility
4. Update package references and documentation

## Type Mapping Consolidation - Implementation Details

**Major Achievement**: Successfully consolidated ~3000+ lines of duplicated type mapping code across all database providers.

**Key Components Added:**
- `DbProviderTypeMapBase<T>` - Abstract base class providing shared converter registration and type creation logic
- `IProviderTypeMapping` - Interface defining provider-specific type configuration (constants, methods)
- Provider-specific configuration classes: `SqlServerTypeMapping`, `MySqlTypeMapping`, `PostgreSqlTypeMapping`, `SqliteTypeMapping`
- `TypeMappingDefaults.cs` - Shared constants (string lengths, precision/scale defaults)
- `TypeMappingHelpers.cs` - 14 helper methods for consistent type creation

**Major Provider Refactoring:**
- **SQL Server Provider**: Refactored to use DbProviderTypeMapBase, preserving SQL Server-specific datetime behavior
- **MySQL Provider**: Refactored to use DbProviderTypeMapBase, fixed datetime precision issue (now defaults to precision 6)
- **PostgreSQL Provider**: Refactored to use DbProviderTypeMapBase, preserved range types, arrays, and hstore functionality  
- **SQLite Provider**: Refactored to use DbProviderTypeMapBase, maintained SQLite's text-based geometry storage

**Native Array Support:**
- Implemented comprehensive PostgreSQL native array support for bidirectional type mapping
- Added support for both standard (`text[]`) and internal (`_text`) PostgreSQL array notation
- Created `GetPostgreSqlStandardArrayTypes()` with 46 array type names
- Fixed SQL-to-.NET array type conversion that was causing test failures

**Code Reduction:**
- Eliminated ~60% code duplication across providers
- Reduced provider type map files from ~400-500 lines each to ~100-200 lines
- Centralized common converter logic in shared base classes
- Maintained all provider-specific features and functionality

**Test Results:**
- All 491 tests passing across all database providers
- No functional regressions during refactoring
- Enhanced maintainability and consistency