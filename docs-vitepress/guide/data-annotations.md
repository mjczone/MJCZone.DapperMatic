# Data Annotations

DapperMatic provides comprehensive data annotation attributes that allow you to define database schema directly on your C# classes and properties. These attributes work with the factory methods to automatically generate `DmTable` and `DmView` objects from your annotated classes.

## Overview

Data annotations provide a declarative way to define your database schema using attributes. This approach offers several benefits:

- **Code-first development** - Define schema alongside your domain models
- **Type safety** - Compile-time validation of your schema definitions
- **Maintainability** - Schema and code stay in sync
- **IntelliSense support** - Full IDE support with parameter hints

## Table and View Annotations

### DmTableAttribute

Defines the table name and schema for a class.

```csharp
[DmTable("dbo", "Users")]
public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
}

// Generate DmTable from the annotated class
DmTable table = DmTableFactory.GetTable(typeof(User));
```

**Parameters:**
- `schemaName` (optional) - The database schema name
- `tableName` (optional) - The table name (defaults to class name)

**Usage Patterns:**
```csharp
// Schema and table name specified
[DmTable("dbo", "app_users")]
public class User { }

// Only schema specified (uses class name as table)
[DmTable("dbo")]
public class Users { }

// Only table name specified (no schema)
[DmTable(tableName: "users")]
public class User { }

// Use class name for table, no schema
[DmTable]
public class Users { }
```

### DmViewAttribute

Defines a database view with its SQL definition.

```csharp
[DmView(@"
    SELECT 
        u.Id,
        u.Username,
        u.Email,
        u.CreatedAt
    FROM {0}.Users u 
    WHERE u.IsActive = 1", 
    schemaName: "dbo", 
    viewName: "ActiveUsers")]
public class ActiveUserView
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
}

// Generate DmView from the annotated class
DmView view = DmViewFactory.GetView(typeof(ActiveUserView));
```

**Parameters:**
- `definition` (required) - SQL definition for the view (use `{0}` for schema placeholder)
- `schemaName` (optional) - The database schema name
- `viewName` (optional) - The view name (defaults to class name)

## Column Annotations

### DmColumnAttribute

The most comprehensive attribute for defining column properties.

```csharp
public class Product
{
    [DmColumn("product_id", isAutoIncrement: true, isPrimaryKey: true)]
    public int Id { get; set; }

    [DmColumn("product_name", length: 100, isNullable: false)]
    public string Name { get; set; }

    [DmColumn("description", length: 500, isNullable: true)]
    public string? Description { get; set; }

    [DmColumn("price", precision: 10, scale: 2, isNullable: false)]
    public decimal Price { get; set; }

    [DmColumn("category_id", isForeignKey: true, 
              referencedTableName: "Categories", 
              referencedColumnName: "Id")]
    public int CategoryId { get; set; }

    [DmColumn("created_at", defaultExpression: "GETDATE()", isNullable: false)]
    public DateTime CreatedAt { get; set; }

    [DmColumn("is_active", defaultExpression: "1", isNullable: false)]
    public bool IsActive { get; set; }
}
```

**Key Parameters:**
- `columnName` - Database column name
- `providerDataType` - Specific database type (e.g., "nvarchar(100)")
- `length` - Maximum length for strings/binary data
- `precision` - Total digits for numeric types
- `scale` - Decimal places for numeric types
- `isNullable` - Whether column allows NULL
- `isPrimaryKey` - Whether column is part of primary key
- `isAutoIncrement` - Whether column auto-increments
- `isUnique` - Whether column has unique constraint
- `isIndexed` - Whether to create an index
- `checkExpression` - Check constraint expression
- `defaultExpression` - Default value expression
- `isForeignKey` - Whether column is a foreign key
- `referencedTableName` - Referenced table for foreign keys
- `referencedColumnName` - Referenced column for foreign keys
- `onDelete` / `onUpdate` - Foreign key actions

### Provider-Specific Data Types

Use the `providerDataType` parameter to specify database-specific types:

```csharp
public class Document
{
    // Different types for different providers
    [DmColumn("content", 
              providerDataType: "{sqlserver:nvarchar(max),mysql:longtext,postgresql:text,sqlite:text}")]
    public string Content { get; set; }

    // JSON column support
    [DmColumn("metadata", 
              providerDataType: "{sqlserver:nvarchar(max),mysql:json,postgresql:jsonb,sqlite:text}")]
    public string Metadata { get; set; }

    // UUID/GUID handling
    [DmColumn("document_id", 
              providerDataType: "{sqlserver:uniqueidentifier,mysql:char(36),postgresql:uuid,sqlite:text}")]
    public Guid DocumentId { get; set; }
}
```

### DmIgnoreAttribute

Excludes properties from database mapping.

```csharp
public class User
{
    public int Id { get; set; }
    
    [DmColumn("username")]
    public string Username { get; set; }
    
    // This property won't be included in the database table
    [DmIgnore]
    public string FullName => $"{FirstName} {LastName}";
    
    [DmIgnore]
    public List<Order> Orders { get; set; } = new();
}
```

## Constraint Annotations

### DmPrimaryKeyConstraintAttribute

Defines primary key constraints at the class or property level.

```csharp
// Single column primary key on property
public class User
{
    [DmPrimaryKeyConstraint(constraintName: "PK_Users")]
    public int Id { get; set; }
}

// Composite primary key on class
[DmPrimaryKeyConstraint(new[] { "UserId", "RoleId" }, "PK_UserRoles")]
public class UserRole
{
    public int UserId { get; set; }
    public int RoleId { get; set; }
}
```

### DmForeignKeyConstraintAttribute

Defines foreign key relationships.

```csharp
// Foreign key on property with type reference
public class Order
{
    [DmForeignKeyConstraint(
        referencedType: typeof(User),
        referencedColumnNames: new[] { "Id" },
        constraintName: "FK_Orders_Users",
        onDelete: DmForeignKeyAction.Cascade)]
    public int UserId { get; set; }
}

// Foreign key on class with explicit table name
[DmForeignKeyConstraint(
    new[] { "CategoryId" }, 
    referencedTableName: "Categories",
    referencedColumnNames: new[] { "Id" },
    constraintName: "FK_Products_Categories")]
public class Product
{
    public int CategoryId { get; set; }
}
```

**Foreign Key Actions:**
- `NoAction` - No action (default)
- `Restrict` - Prevent the action
- `Cascade` - Cascade the action
- `SetNull` - Set to NULL
- `SetDefault` - Set to default value

### DmUniqueConstraintAttribute

Creates unique constraints on single or multiple columns.

```csharp
// Single column unique constraint
public class User
{
    [DmUniqueConstraint(constraintName: "UQ_Users_Email")]
    public string Email { get; set; }
}

// Multi-column unique constraint on class
[DmUniqueConstraint(new[] { "FirstName", "LastName", "DateOfBirth" }, "UQ_Users_Natural")]
public class User
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
}
```

### DmCheckConstraintAttribute

Defines business rule constraints.

```csharp
public class Product
{
    [DmCheckConstraint("CK_Products_Price_Positive", "Price > 0")]
    public decimal Price { get; set; }
}

// Class-level check constraint
[DmCheckConstraint("CK_Users_Age_Valid", "Age >= 0 AND Age <= 150")]
public class User
{
    public int Age { get; set; }
}
```

### DmDefaultConstraintAttribute

Provides default values for columns.

```csharp
public class User
{
    [DmDefaultConstraint("DF_Users_CreatedAt", "GETDATE()")]
    public DateTime CreatedAt { get; set; }

    [DmDefaultConstraint("DF_Users_IsActive", "1")]
    public bool IsActive { get; set; }

    [DmDefaultConstraint("DF_Users_Id", "NEWID()")]
    public Guid Id { get; set; }
}
```

## Index Annotations

### DmIndexAttribute

Creates database indexes for performance optimization.

```csharp
public class User
{
    // Simple index on property
    [DmIndex(indexName: "IX_Users_Email")]
    public string Email { get; set; }

    // Unique index on property
    [DmIndex(isUnique: true, indexName: "IX_Users_Username")]
    public string Username { get; set; }
}

// Composite index on class
[DmIndex(columnNames: new[] { "LastName", "FirstName" }, indexName: "IX_Users_Name")]
[DmIndex(isUnique: true, columnNames: new[] { "Email" }, indexName: "IX_Users_Email_Unique")]
public class User
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
}
```

## Complete Example

Here's a comprehensive example showing multiple annotations on a single class:

```csharp
[DmTable("dbo", "Orders")]
[DmIndex(columnNames: new[] { "UserId", "OrderDate" }, indexName: "IX_Orders_User_Date")]
[DmCheckConstraint("CK_Orders_TotalAmount_Positive", "TotalAmount > 0")]
[DmForeignKeyConstraint(
    new[] { "UserId" }, 
    typeof(User), 
    new[] { "Id" },
    "FK_Orders_Users",
    DmForeignKeyAction.Restrict)]
public class Order
{
    [DmColumn("order_id", isAutoIncrement: true, isPrimaryKey: true)]
    public int Id { get; set; }

    [DmColumn("user_id", isNullable: false, isIndexed: true)]
    public int UserId { get; set; }

    [DmColumn("order_number", length: 50, isNullable: false)]
    [DmUniqueConstraint(constraintName: "UQ_Orders_OrderNumber")]
    public string OrderNumber { get; set; }

    [DmColumn("order_date", isNullable: false, isIndexed: true)]
    [DmDefaultConstraint("DF_Orders_OrderDate", "GETDATE()")]
    public DateTime OrderDate { get; set; }

    [DmColumn("total_amount", precision: 10, scale: 2, isNullable: false)]
    public decimal TotalAmount { get; set; }

    [DmColumn("status", length: 20, isNullable: false)]
    [DmDefaultConstraint("DF_Orders_Status", "'Pending'")]
    [DmCheckConstraint("CK_Orders_Status_Valid", 
                       "Status IN ('Pending', 'Processing', 'Shipped', 'Delivered', 'Cancelled')")]
    public string Status { get; set; }

    [DmColumn("notes", length: 1000, isNullable: true)]
    public string? Notes { get; set; }

    // Navigation properties ignored
    [DmIgnore]
    public User User { get; set; }

    [DmIgnore]
    public List<OrderItem> Items { get; set; } = new();
}

// Usage
DmTable orderTable = DmTableFactory.GetTable(typeof(Order));
await connection.CreateTableIfNotExistsAsync("dbo", orderTable);
```

## Best Practices

1. **Use meaningful constraint names** - Include table name and purpose
2. **Specify nullability explicitly** - Don't rely on defaults
3. **Group related annotations** - Put class-level constraints together
4. **Use type references** - Prefer `typeof(User)` over string table names for foreign keys
5. **Document complex expressions** - Add comments for complex check constraints
6. **Consider provider differences** - Use provider-specific types when needed
7. **Validate at compile time** - Attributes provide compile-time validation
8. **Use consistent naming** - Follow consistent patterns for constraint names

## Migration from Models

You can gradually migrate from manual `DmTable` creation to data annotations:

```csharp
// Before: Manual model creation
var table = new DmTable("Users")
{
    Columns = new[] { /* ... */ },
    PrimaryKey = new DmPrimaryKeyConstraint("PK_Users", "Id")
};

// After: Data annotations + factory
[DmTable("dbo", "Users")]
public class User
{
    [DmColumn("id", isPrimaryKey: true, isAutoIncrement: true)]
    public int Id { get; set; }
}

DmTable table = DmTableFactory.GetTable(typeof(User));
```

Data annotations provide a powerful, type-safe way to define your database schema directly in your C# code, making your applications more maintainable and reducing the gap between your domain models and database structure.