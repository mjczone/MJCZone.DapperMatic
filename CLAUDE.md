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

## Code Style Guidelines

- Use trailing comma in multi-line initializers

## Type Mapping Consolidation Plan

**Goal**: Eliminate ~3000+ lines of duplicated type mapping code across providers while maintaining consistency.

### Progress Tracking
- ✅ **Step 1**: Constants & Helpers - Create shared defaults and helper methods
- ✅ **Step 2**: Refactor SQL Server Provider - Use new helpers in existing provider  
- ✅ **Step 3**: Refactor Other Providers - Apply changes to MySQL, PostgreSQL, SQLite
- ✅ **Step 5**: Standardize Geometry - Unify NetTopologySuite handling across providers
- ✅ **Step 6**: Unify JSON Handling - Standardize JSON type strategies  
- ⏳ **Step 7**: Native Array Support - Implement array handling where available (PostgreSQL)
- ⏳ **Step 8**: Extract Base Converters - Move common conversion logic to base classes
- ⏳ **Step 9**: Consistency Testing - Add comprehensive type mapping tests

### Current Todo List
1. ✅ Analyze current type mapping inconsistencies in detail
2. ✅ Design unified type mapping strategy  
3. ✅ Create base type mapping consolidation framework
4. ✅ Analyze provider implementations for additional helper method needs
5. ✅ Refactor SQL Server provider to use TypeMappingHelpers
6. ✅ Refactor MySQL provider to use TypeMappingHelpers
7. ✅ Refactor PostgreSQL provider to use TypeMappingHelpers
8. ✅ Refactor SQLite provider to use TypeMappingHelpers
9. ✅ Standardize geometry type handling across providers
10. ✅ Unify JSON type handling strategies  
11. ⏳ Implement native array support where available
12. ⏳ Extract shared conversion logic to base classes
13. ⏳ Add tests for type mapping consistency

**Next Session**: Begin Step 7 - Implement native array support where available (PostgreSQL)

### Step 2 Completed (2025-01-31)
Successfully refactored SQL Server provider to use TypeMappingHelpers:
- Replaced 13 converter methods with helper calls
- Reduced code duplication in numeric, string, datetime, binary, and geometry converters
- Used `CreateSimpleType()`, `CreateDecimalType()`, `CreateStringType()`, `CreateBinaryType()`, `CreateJsonType()`, `CreateGeometryType()`, `CreateLobType()`, and `CreateEnumStringType()`
- Improved geometry handling using `GetAssemblyQualifiedShortName()` helper
- All tests pass - no functional changes, only code consolidation

### Step 3 Completed (2025-01-31)
Successfully refactored MySQL, PostgreSQL, and SQLite providers to use TypeMappingHelpers:

**MySQL Provider Refactoring:**
- Refactored all converter methods to use TypeMappingHelpers
- Standardized decimal type handling with `CreateDecimalType()`
- Used `CreateGuidStringType()` for consistent GUID storage as fixed-length strings
- Applied `CreateLobType()` for text/blob types and `CreateJsonType()` for native JSON support
- Enhanced geometry handling with specific MySQL geometry types using `CreateGeometryType()`
- Improved enum handling with `CreateEnumStringType()`

**PostgreSQL Provider Refactoring:**
- Refactored text, datetime, enumerable, and geometry converters
- Applied `CreateStringType()` and `CreateLobType()` for text handling with proper max length support
- Used `CreateSimpleType()` for datetime types (timestamp, timestamptz, time, date)
- Enhanced HStore support for string dictionaries vs JSONB for other collections
- Standardized geometry handling for both NetTopologySuite and PostgreSQL-specific types
- Used `GetAssemblyQualifiedShortName()` helper for consistent type identification

**SQLite Provider Refactoring:**
- Completed refactoring of numeric, text, datetime, and geometry converters
- Applied `CreateDecimalType()` for decimal types with proper precision/scale
- Used `CreateStringType()` and `CreateLobType()` for text types, handling SQLite's max length limitations
- Standardized datetime types with `CreateSimpleType()`
- Enhanced geometry handling for NetTopologySuite types stored as text (WKT format)

**Results:**
- All 426 tests pass across all providers
- Significant code reduction and standardization achieved
- Consistent type creation patterns now used across all four database providers
- Improved maintainability and reduced duplication