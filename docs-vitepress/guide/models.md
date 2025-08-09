# Models

DapperMatic uses a model-first approach with `Dm*` prefixed classes to define database schema objects. These models provide a strongly-typed way to create and manipulate database structures.

## Core Models

### DmTable

The `DmTable` class represents a database table with all its components.

```csharp
var table = new DmTable("Users")
{
    TableName = "Users",
    Columns = new[]
    {
        new DmColumn("Id", typeof(int)) { IsNullable = false, IsAutoIncrement = true },
        new DmColumn("Username", typeof(string)) { MaxLength = 50, IsNullable = false },
        new DmColumn("Email", typeof(string)) { MaxLength = 100, IsNullable = false },
        new DmColumn("IsActive", typeof(bool)) { IsNullable = false, DefaultValue = "1" },
        new DmColumn("CreatedAt", typeof(DateTime)) { IsNullable = false }
    },
    PrimaryKey = new DmPrimaryKeyConstraint("PK_Users", "Id"),
    Indexes = new[]
    {
        new DmIndex("IX_Users_Username", new[] { "Username" }) { IsUnique = true },
        new DmIndex("IX_Users_Email", new[] { "Email" }) { IsUnique = true }
    },
    CheckConstraints = new[]
    {
        new DmCheckConstraint("CK_Users_Username_Length", "LEN(Username) > 0")
    },
    DefaultConstraints = new[]
    {
        new DmDefaultConstraint("DF_Users_CreatedAt", "CreatedAt", "GETDATE()")
    }
};
```

**Properties:**
- `TableName` - Name of the table
- `Columns` - Array of column definitions
- `PrimaryKey` - Primary key constraint (optional)
- `ForeignKeys` - Foreign key constraints
- `CheckConstraints` - Check constraints
- `DefaultConstraints` - Default value constraints
- `UniqueConstraints` - Unique constraints
- `Indexes` - Table indexes

### DmColumn

Defines a table column with its data type and properties.

```csharp
// Auto-increment primary key
var idColumn = new DmColumn("Id", typeof(int))
{
    IsNullable = false,
    IsAutoIncrement = true
};

// String column with length
var nameColumn = new DmColumn("Name", typeof(string))
{
    MaxLength = 100,
    IsNullable = false
};

// Decimal column with precision/scale
var priceColumn = new DmColumn("Price", typeof(decimal))
{
    Precision = 10,
    Scale = 2,
    IsNullable = true
};

// Column with default value
var statusColumn = new DmColumn("Status", typeof(string))
{
    MaxLength = 20,
    IsNullable = false,
    DefaultValue = "'Active'"
};
```

**Key Properties:**
- `ColumnName` - Name of the column
- `DataType` - .NET type (automatically mapped to SQL type)
- `IsNullable` - Whether the column allows NULL values
- `IsAutoIncrement` - Whether the column auto-increments
- `MaxLength` - Maximum length for string/binary types
- `Precision` - Total digits for numeric types
- `Scale` - Decimal places for numeric types
- `DefaultValue` - Default value expression

### DmView

Represents a database view with its definition.

```csharp
var view = new DmView("ActiveUsers")
{
    ViewName = "ActiveUsers",
    Definition = @"
        SELECT 
            Id,
            Username,
            Email,
            CreatedAt
        FROM Users 
        WHERE IsActive = 1"
};
```

**Properties:**
- `ViewName` - Name of the view
- `Definition` - SQL definition of the view

## Constraint Models

### DmPrimaryKeyConstraint

Defines a primary key constraint on one or more columns.

```csharp
// Single column primary key
var singlePK = new DmPrimaryKeyConstraint("PK_Users", "Id");

// Composite primary key
var compositePK = new DmPrimaryKeyConstraint("PK_OrderItems", new[] { "OrderId", "ProductId" });
```

### DmForeignKeyConstraint

Defines relationships between tables.

```csharp
// Basic foreign key
var basicFK = new DmForeignKeyConstraint(
    constraintName: "FK_Orders_Users",
    columnNames: new[] { "UserId" },
    referencedTableName: "Users",
    referencedColumnNames: new[] { "Id" }
);

// Foreign key with cascade actions
var cascadeFK = new DmForeignKeyConstraint(
    constraintName: "FK_OrderItems_Orders",
    columnNames: new[] { "OrderId" },
    referencedTableName: "Orders",
    referencedColumnNames: new[] { "Id" }
)
{
    OnDelete = DmForeignKeyAction.Cascade,
    OnUpdate = DmForeignKeyAction.Restrict
};
```

**DmForeignKeyAction Options:**
- `NoAction` - No action (default)
- `Restrict` - Prevent the action
- `Cascade` - Cascade the action
- `SetNull` - Set referencing columns to NULL
- `SetDefault` - Set referencing columns to their default values

### DmUniqueConstraint

Ensures uniqueness across one or more columns.

```csharp
// Single column unique constraint
var emailUnique = new DmUniqueConstraint("UQ_Users_Email", new[] { "Email" });

// Multi-column unique constraint
var compositeUnique = new DmUniqueConstraint(
    "UQ_UserProfiles_UserId_ProfileType", 
    new[] { "UserId", "ProfileType" }
);
```

### DmCheckConstraint

Defines business rules at the database level.

```csharp
// Simple value check
var ageCheck = new DmCheckConstraint("CK_Users_Age", "Age >= 0 AND Age <= 150");

// Complex business rule
var emailCheck = new DmCheckConstraint(
    "CK_Users_Email_Format", 
    "Email LIKE '%@%.%' AND LEN(Email) > 5"
);

// Date range check
var dateCheck = new DmCheckConstraint(
    "CK_Orders_ValidDateRange", 
    "OrderDate >= '2020-01-01' AND OrderDate <= GETDATE()"
);
```

### DmDefaultConstraint

Provides default values for columns.

```csharp
// Current timestamp default
var createdAtDefault = new DmDefaultConstraint(
    "DF_Users_CreatedAt", 
    "CreatedAt", 
    "GETDATE()"
);

// GUID default
var idDefault = new DmDefaultConstraint(
    "DF_Sessions_Id", 
    "Id", 
    "NEWID()"
);

// Constant value default
var statusDefault = new DmDefaultConstraint(
    "DF_Users_Status", 
    "Status", 
    "'Pending'"
);
```

## Index Models

### DmIndex

Defines database indexes for performance optimization.

```csharp
// Simple index
var nameIndex = new DmIndex("IX_Users_LastName", new[] { "LastName" });

// Unique index
var emailIndex = new DmIndex("IX_Users_Email", new[] { "Email" })
{
    IsUnique = true
};

// Composite index with column ordering
var compositeIndex = new DmIndex("IX_Orders_Date_Status", new[]
{
    new DmOrderedColumn("OrderDate", DmColumnOrder.Descending),
    new DmOrderedColumn("Status", DmColumnOrder.Ascending)
});

// Filtered index (SQL Server)
var filteredIndex = new DmIndex("IX_Users_Active", new[] { "Username" })
{
    IsUnique = true,
    Filter = "IsActive = 1"  // Provider-specific feature
};
```

**Properties:**
- `IndexName` - Name of the index
- `Columns` - Columns included in the index
- `IsUnique` - Whether the index enforces uniqueness
- `Filter` - Filter expression (provider-specific)

### DmOrderedColumn

Specifies column ordering within indexes.

```csharp
// Ascending order (default)
var ascColumn = new DmOrderedColumn("CreatedAt", DmColumnOrder.Ascending);

// Descending order
var descColumn = new DmOrderedColumn("CreatedAt", DmColumnOrder.Descending);
```

## Factory Methods

### DmTableFactory

Generate table models from .NET classes using attributes.

```csharp
// Define a class with attributes
[Table("app_employees")]
public class Employee
{
    [Key]
    public int Id { get; set; }
    
    [MaxLength(100)]
    public string Name { get; set; }
    
    [MaxLength(200)]
    public string Email { get; set; }
    
    public DateTime HireDate { get; set; }
}

// Generate DmTable from class
DmTable table = DmTableFactory.GetTable(typeof(Employee));
```

### DmViewFactory

Generate view models from .NET classes.

```csharp
[View("vw_active_employees")]
public class ActiveEmployeeView
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime HireDate { get; set; }
}

// Generate DmView from class
DmView view = DmViewFactory.GetView(typeof(ActiveEmployeeView));
```

## Complete Example

Here's a comprehensive example showing how to create a complete table with all types of constraints:

```csharp
var ordersTable = new DmTable("Orders")
{
    Columns = new[]
    {
        new DmColumn("Id", typeof(int)) { IsNullable = false, IsAutoIncrement = true },
        new DmColumn("UserId", typeof(int)) { IsNullable = false },
        new DmColumn("OrderNumber", typeof(string)) { MaxLength = 50, IsNullable = false },
        new DmColumn("OrderDate", typeof(DateTime)) { IsNullable = false },
        new DmColumn("TotalAmount", typeof(decimal)) { Precision = 10, Scale = 2, IsNullable = false },
        new DmColumn("Status", typeof(string)) { MaxLength = 20, IsNullable = false },
        new DmColumn("CreatedAt", typeof(DateTime)) { IsNullable = false },
        new DmColumn("UpdatedAt", typeof(DateTime)) { IsNullable = true }
    },
    PrimaryKey = new DmPrimaryKeyConstraint("PK_Orders", "Id"),
    ForeignKeys = new[]
    {
        new DmForeignKeyConstraint("FK_Orders_Users", new[] { "UserId" }, "Users", new[] { "Id" })
        {
            OnDelete = DmForeignKeyAction.Restrict
        }
    },
    UniqueConstraints = new[]
    {
        new DmUniqueConstraint("UQ_Orders_OrderNumber", new[] { "OrderNumber" })
    },
    CheckConstraints = new[]
    {
        new DmCheckConstraint("CK_Orders_TotalAmount", "TotalAmount >= 0"),
        new DmCheckConstraint("CK_Orders_Status", "Status IN ('Pending', 'Processing', 'Shipped', 'Delivered', 'Cancelled')")
    },
    DefaultConstraints = new[]
    {
        new DmDefaultConstraint("DF_Orders_Status", "Status", "'Pending'"),
        new DmDefaultConstraint("DF_Orders_CreatedAt", "CreatedAt", "GETDATE()")
    },
    Indexes = new[]
    {
        new DmIndex("IX_Orders_UserId", new[] { "UserId" }),
        new DmIndex("IX_Orders_OrderDate", new[] { "OrderDate" }),
        new DmIndex("IX_Orders_Status_Date", new[]
        {
            new DmOrderedColumn("Status", DmColumnOrder.Ascending),
            new DmOrderedColumn("OrderDate", DmColumnOrder.Descending)
        })
    }
};

// Create the table
await connection.CreateTableIfNotExistsAsync("dbo", ordersTable);
```

## Best Practices

1. **Use meaningful names** for constraints and indexes
2. **Always specify nullability** explicitly
3. **Set appropriate string lengths** to avoid truncation
4. **Use check constraints** for business rules
5. **Create indexes** on foreign key columns
6. **Consider composite indexes** for common query patterns
7. **Use factory methods** when working with existing classes