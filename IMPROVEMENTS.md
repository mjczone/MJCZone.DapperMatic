# MJCZone.DapperMatic Improvement Suggestions

## Documentation & Community Building

### Critical Documentation Needs

- **README.md Enhancement**: Current README is too minimal - needs quickstart examples, feature highlights, and comparison with alternatives
- **Code Examples**: Add comprehensive examples showing common scenarios (migrations, schema diffing, model-first development)
- **API Reference**: Generate and publish API documentation from XML comments
- **Migration Guide**: Document migration paths from raw SQL or other schema management tools

### Community Engagement

- **NuGet Package Metadata**: Add package icon, tags, and detailed description for better discoverability
- **Benchmarks**: Performance comparisons with raw ADO.NET and other schema management libraries
- **Sample Projects**: Create sample applications demonstrating real-world usage patterns
- **Contributing Guidelines**: Add CONTRIBUTING.md with development setup and PR guidelines

## Code Quality & Architecture

### API Design Improvements

- **Fluent API**: Consider adding fluent builder pattern for complex schema definitions
- **Async-Only Option**: Some sync methods exist - consider async-only API for consistency
- **Batch Operations**: Add methods for bulk schema operations with transaction support
- **Schema Diff Engine**: Implement schema comparison and migration generation capabilities

### Code Organization

- **Provider Abstraction**: Extract common SQL patterns to reduce code duplication across providers
- **Extension Method Organization**: Consider splitting extension methods into feature-specific namespaces
- **Model Validation**: Add validation attributes and methods to ensure model integrity before execution
- **Error Messages**: Enhance error messages with provider-specific troubleshooting hints

### Technical Debt

- **✅ Consolidate Auto-increment Detection**: ~~Create unified auto-increment detection strategy across providers~~ **COMPLETED** - Standardized auto-increment detection implemented across all providers
- **✅ Consolidate Type Mapping**: ~~Create unified type mapping strategy across providers~~ **COMPLETED** - Successfully consolidated ~3000+ lines of duplicated type mapping code using DbProviderTypeMapBase<T> and TypeMappingHelpers  
- **✅ Dapper Dependency Strategy**: ~~Replace minimal Dapper usage (3 methods) with raw ADO.NET to reduce dependencies and package size~~ **IMPLEMENTED** - Analyzed complexity, retained Dapper with flexible version range `[2.1.35,3.0.0)` to prevent user version conflicts while maintaining functionality
- **✅ SQL Injection Prevention**: ~~Audit all SQL generation for proper parameterization~~ **COMPLETED** - Implemented comprehensive SQL injection protection with SqlExpressionValidator for view definitions, check constraints, and default expressions. Added 26 security tests covering attack vectors and edge cases.

## Feature Enhancements

### Core Features

- **Schema Versioning**: Add built-in support for tracking and managing schema versions
- **Reverse Engineering**: Generate C# models from existing database schemas
- **Change Tracking**: Track and log all DDL operations for audit purposes
- **Rollback Support**: Implement undo/rollback capabilities for DDL operations

### Provider Support

- **Oracle Support**: Add Oracle database provider implementation
- **MongoDB Support**: Consider NoSQL provider support for schema-like operations
- **Cloud Database Features**: Support for cloud-specific features (Azure SQL, Amazon RDS)
- **Provider Feature Matrix**: Document which features are supported by each provider

### Advanced Scenarios

- **Computed Columns**: Add support for computed/generated columns
- **Partitioning**: Support table and index partitioning where available
- **Temporal Tables**: Add support for temporal/history tables
- **Full-Text Indexes**: Implement full-text index support across providers
- **Stored Procedures**: Consider adding stored procedure/function management
