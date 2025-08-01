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

- **Dapper Dependency Strategy (2025-08-01)**: Comprehensive analysis and refined dependency management approach
  - **Analysis Results**: Dapper usage is more complex than initially assessed - uses named tuples, complex type mapping, and dynamic queries extensively
  - **Decision**: Retain Dapper dependency with flexible versioning strategy to balance functionality vs. user flexibility
  - **Version Strategy**: Implemented version range `[2.1.35,3.0.0)` allowing users to reference different Dapper versions
  - **User Benefits**: Applications can use any compatible Dapper version (2.1.35-2.1.66+) without conflicts
  - **Learning**: Library's core value is schema management, not reimplementing data access - focus engineering effort accordingly
  - **Solution**: Version range prevents conflicts while maintaining API stability and feature access

- **SQL Injection Prevention (2025-08-01)**: Comprehensive security hardening to prevent SQL injection attacks
  - **Security Audit**: Identified critical vulnerabilities in view definitions, check constraints, and default expressions
  - **Implementation**: Created `SqlExpressionValidator` class with expression validation for all user-provided SQL code
  - **Protection Scope**: View definitions, check constraint expressions, default constraint expressions, and comment injection
  - **Validation Features**: Expression length limits, dangerous pattern detection, comment stripping, multi-statement prevention
  - **Security Testing**: Added 26 comprehensive security tests covering various attack vectors and edge cases
  - **Balance**: Maintains legitimate SQL functionality while blocking malicious injection attempts
  - **Coverage**: All database providers (SQL Server, MySQL, PostgreSQL, SQLite) protected consistently

## Code Style Guidelines

- Use trailing comma in multi-line initializers
- Code should not contain trailing whitespace

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

### Dapper Dependency Strategy (2025-08-01)

**Goal**: Balance dependency management with user flexibility and library functionality.

**Status**: ✅ **IMPLEMENTED** - Flexible dependency strategy with version range.

**Analysis Phase:**
1. **Initial Assessment**: Attempted to eliminate Dapper dependency entirely
2. **Complexity Discovery**: Library uses advanced Dapper features (named tuples, complex type mapping, null handling)
3. **Implementation Challenge**: Would require reimplementing significant Dapper functionality (~200+ lines)
4. **Risk Evaluation**: High risk of bugs in core data access with minimal benefit

**Strategic Decision:**
- **Retain Dependency**: Keep Dapper for robust, well-tested data access functionality
- **Add Flexibility**: Implement version range to prevent user conflicts
- **Focus Effort**: Concentrate on library's core value (schema management) vs. data access reimplementation

**Implementation Details:**
- **Version Range**: `[2.1.35,3.0.0)` allows compatible versions while preventing breaking changes
- **Compatibility**: Supports Dapper versions from 2.1.35 through 2.1.66+ 
- **User Benefits**: Applications can reference any compatible Dapper version without conflicts
- **Testing**: Verified build and test compatibility with version range

**Key Learnings:**
- Dependency elimination isn't always the best solution - balance functionality vs. complexity
- Version ranges provide excellent compromise between stability and flexibility
- Library's value proposition should drive architectural decisions
- User experience (no version conflicts) is as important as technical purity

**Future Considerations:**
- Monitor Dapper's major version releases for potential breaking changes
- Consider dependency injection pattern if ecosystem demands change

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