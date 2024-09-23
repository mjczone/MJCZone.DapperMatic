using DapperMatic.Models;

namespace DapperMatic.DataAnnotations;

[AttributeUsage(
    AttributeTargets.Property | AttributeTargets.Class,
    Inherited = false,
    AllowMultiple = true
)]
public class DxForeignKeyConstraintAttribute : Attribute
{
    /// <summary>
    /// Use this on a class to define a foreign key constraint. Constructor for single-column foreign key constraint
    /// </summary>
    public DxForeignKeyConstraintAttribute(
        string sourceColumnName,
        Type referencedType,
        string? referencedColumnName = null,
        string? constraintName = null,
        DxForeignKeyAction onDelete = DxForeignKeyAction.NoAction,
        DxForeignKeyAction onUpdate = DxForeignKeyAction.NoAction
    )
    {
        ConstraintName = constraintName;
        SourceColumnNames = [sourceColumnName];
        ReferencedType = referencedType;
        ReferencedColumnNames = string.IsNullOrWhiteSpace(referencedColumnName)
            ? null
            : [referencedColumnName];
        OnDelete = onDelete;
        OnUpdate = onUpdate;
    }

    /// <summary>
    /// Use this on a class to define a foreign key constraint. Constructor for multi-column foreign key constraint (composite keys)
    /// </summary>
    public DxForeignKeyConstraintAttribute(
        string[] sourceColumnNames,
        Type referencedType,
        string[]? referencedColumnNames = null,
        string? constraintName = null,
        DxForeignKeyAction onDelete = DxForeignKeyAction.NoAction,
        DxForeignKeyAction onUpdate = DxForeignKeyAction.NoAction
    )
    {
        ConstraintName = constraintName;
        SourceColumnNames = sourceColumnNames;
        ReferencedType = referencedType;
        ReferencedColumnNames =
            referencedColumnNames == null || referencedColumnNames.Length == 0
                ? null
                : referencedColumnNames;
        OnDelete = onDelete;
        OnUpdate = onUpdate;
    }

    /// <summary>
    /// Use this on a class to define a foreign key constraint. Constructor for single-column foreign key constraint
    /// </summary>
    public DxForeignKeyConstraintAttribute(
        string sourceColumnName,
        string referencedTableName,
        string referencedColumnName,
        string? constraintName = null,
        DxForeignKeyAction onDelete = DxForeignKeyAction.NoAction,
        DxForeignKeyAction onUpdate = DxForeignKeyAction.NoAction
    )
    {
        ConstraintName = constraintName;
        SourceColumnNames = [sourceColumnName];
        ReferencedTableName = referencedTableName;
        ReferencedColumnNames = [referencedColumnName];
        OnDelete = onDelete;
        OnUpdate = onUpdate;
    }

    /// <summary>
    /// Use this on a class to define a foreign key constraint. Constructor for multi-column foreign key constraint (composite keys)
    /// </summary>
    public DxForeignKeyConstraintAttribute(
        string[] sourceColumnNames,
        string referencedTableName,
        string[] referencedColumnNames,
        string? constraintName = null,
        DxForeignKeyAction onDelete = DxForeignKeyAction.NoAction,
        DxForeignKeyAction onUpdate = DxForeignKeyAction.NoAction
    )
    {
        ConstraintName = constraintName;
        SourceColumnNames = sourceColumnNames;
        ReferencedTableName = referencedTableName;
        ReferencedColumnNames = referencedColumnNames;
        OnDelete = onDelete;
        OnUpdate = onUpdate;
    }

    /// <summary>
    /// Use this on a property to define a foreign key constraint
    /// </summary>
    public DxForeignKeyConstraintAttribute(
        Type referencedType,
        string? referencedColumnName = null,
        string? constraintName = null,
        DxForeignKeyAction onDelete = DxForeignKeyAction.NoAction,
        DxForeignKeyAction onUpdate = DxForeignKeyAction.NoAction
    )
    {
        ConstraintName = constraintName;
        ReferencedType = referencedType;
        ReferencedColumnNames = string.IsNullOrWhiteSpace(referencedColumnName)
            ? null
            : [referencedColumnName];
        OnDelete = onDelete;
        OnUpdate = onUpdate;
    }

    /// <summary>
    /// Use this on a property to define a foreign key constraint
    /// </summary>
    public DxForeignKeyConstraintAttribute(
        string referencedTableName,
        string? referencedColumnName = null,
        string? constraintName = null,
        DxForeignKeyAction onDelete = DxForeignKeyAction.NoAction,
        DxForeignKeyAction onUpdate = DxForeignKeyAction.NoAction
    )
    {
        ConstraintName = constraintName;
        ReferencedTableName = referencedTableName;
        ReferencedColumnNames = string.IsNullOrWhiteSpace(referencedColumnName)
            ? null
            : [referencedColumnName];
        OnDelete = onDelete;
        OnUpdate = onUpdate;
    }

    public string? ConstraintName { get; }
    public string[]? SourceColumnNames { get; }
    public Type? ReferencedType { get; }
    public string? ReferencedTableName { get; }
    public string[]? ReferencedColumnNames { get; }
    public DxForeignKeyAction? OnDelete { get; }
    public DxForeignKeyAction? OnUpdate { get; }
}
