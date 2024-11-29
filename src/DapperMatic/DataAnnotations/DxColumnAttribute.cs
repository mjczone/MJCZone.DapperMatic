using DapperMatic.Models;

namespace DapperMatic.DataAnnotations;

/// <summary>
/// Attribute to define a database column.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class DxColumnAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DxColumnAttribute"/> class representing a column in a database table.
    /// </summary>
    /// <param name="columnName">The name of the column.</param>
    /// <param name="providerDataType">The data type of the column as defined by the database provider. Can be null if not specified.</param>
    /// <param name="length">The maximum length (in characters) for string or binary data types. Can be null if not specified.</param>
    /// <param name="precision">The total number of digits for numeric data types. Can be null if not specified.</param>
    /// <param name="scale">The number of digits to the right of the decimal point for numeric data types. Can be null if not specified.</param>
    /// <param name="checkExpression">An optional check expression defining constraints on valid values for the column.</param>
    /// <param name="defaultExpression">An optional default expression defining the default value for the column when no explicit value is provided during insertion or update.</param>
    /// <param name="isNullable">
    ///     A boolean value indicating whether the column can contain null values. Defaults to true (nullable).
    /// </param>
    /// <param name="isPrimaryKey">A boolean value indicating whether the column is part of the table's primary key constraint. Defaults to false.</param>
    /// <param name="isAutoIncrement">
    ///     A boolean value indicating whether the column automatically increments its value for each new row inserted into the table without explicitly providing a value.
    ///     Defaults to false.
    /// </param>
    /// <param name="isUnique">A boolean value indicating whether the column must contain unique values. Defaults to false.</param>
    /// <param name="isIndexed">A boolean value indicating whether the column is indexed for faster lookup and sorting operations. Defaults to false.</param>
    /// <param name="isForeignKey">
    ///     A boolean value indicating whether the column participates in a foreign key constraint referencing another table.
    ///     Defaults to false.
    /// </param>
    /// <param name="referencedTableName">The name of the referenced table for foreign key columns. Can be null if not specified.</param>
    /// <param name="referencedColumnName">The name of the referenced column in the referenced table for foreign key columns. Can be null if not specified.</param>
    /// <param name="onDelete">The action to take when a referenced row is deleted. Can be null if not specified.</param>
    /// <param name="onUpdate">The action to take when a referenced row is updated. Can be null if not specified.</param>
    public DxColumnAttribute(
        string columnName,
        string? providerDataType = null,
        int? length = null,
        int? precision = null,
        int? scale = null,
        string? checkExpression = null,
        string? defaultExpression = null,
        bool isNullable = true,
        bool isPrimaryKey = false,
        bool isAutoIncrement = false,
        bool isUnique = false,
        bool isIndexed = false,
        bool isForeignKey = false,
        string? referencedTableName = null,
        string? referencedColumnName = null,
        DxForeignKeyAction? onDelete = null,
        DxForeignKeyAction? onUpdate = null
    )
    {
        ColumnName = columnName;
        ProviderDataType = providerDataType;
        Length = length;
        Precision = precision;
        Scale = scale;
        CheckExpression = checkExpression;
        DefaultExpression = defaultExpression;
        IsNullable = isNullable;
        IsPrimaryKey = isPrimaryKey;
        IsAutoIncrement = isAutoIncrement;
        IsUnique = isUnique;
        IsIndexed = isIndexed;
        IsForeignKey = isForeignKey;
        ReferencedTableName = referencedTableName;
        ReferencedColumnName = referencedColumnName;
        OnDelete = onDelete;
        OnUpdate = onUpdate;
    }

    /// <summary>
    /// Gets the column name.
    /// </summary>
    public string? ColumnName { get; }

    /// <summary>
    /// Gets the provider data type.
    /// Format of provider data types: {mysql:varchar(255),sqlserver:nvarchar(255)}.
    /// </summary>
    public string? ProviderDataType { get; }

    /// <summary>
    /// Gets the length of the column.
    /// </summary>
    public int? Length { get; }

    /// <summary>
    /// Gets the precision of the column.
    /// </summary>
    public int? Precision { get; }

    /// <summary>
    /// Gets the scale of the column.
    /// </summary>
    public int? Scale { get; }

    /// <summary>
    /// Gets the check expression for the column.
    /// </summary>
    public string? CheckExpression { get; }

    /// <summary>
    /// Gets the default expression for the column.
    /// </summary>
    public string? DefaultExpression { get; }

    /// <summary>
    /// Gets a value indicating whether the column is nullable.
    /// </summary>
    public bool IsNullable { get; }

    /// <summary>
    /// Gets a value indicating whether the column is a primary key.
    /// </summary>
    public bool IsPrimaryKey { get; }

    /// <summary>
    /// Gets a value indicating whether the column is auto-incremented.
    /// </summary>
    public bool IsAutoIncrement { get; }

    /// <summary>
    /// Gets a value indicating whether the column is unique.
    /// </summary>
    public bool IsUnique { get; }

    /// <summary>
    /// Gets a value indicating whether the column is indexed.
    /// </summary>
    public bool IsIndexed { get; }

    /// <summary>
    /// Gets a value indicating whether the column is a foreign key.
    /// </summary>
    public bool IsForeignKey { get; }

    /// <summary>
    /// Gets the referenced table name for the foreign key.
    /// </summary>
    public string? ReferencedTableName { get; }

    /// <summary>
    /// Gets the referenced column name for the foreign key.
    /// </summary>
    public string? ReferencedColumnName { get; }

    /// <summary>
    /// Gets the action to perform on delete.
    /// </summary>
    public DxForeignKeyAction? OnDelete { get; }

    /// <summary>
    /// Gets the action to perform on update.
    /// </summary>
    public DxForeignKeyAction? OnUpdate { get; }
}
