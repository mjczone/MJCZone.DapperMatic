using MJCZone.DapperMatic.Models;

namespace MJCZone.DapperMatic.DataAnnotations;

/// <summary>
/// Attribute to define foreign key constraints on a class or property.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = true)]
public sealed class DmForeignKeyConstraintAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DmForeignKeyConstraintAttribute"/> class.
    /// Constructor for single-column foreign key constraint.
    /// </summary>
    /// <param name="sourceColumnName">The name of the source column in the foreign key constraint.</param>
    /// <param name="referencedType">The type of the referenced entity in the foreign key constraint.</param>
    /// <param name="referencedColumnName">The name of the referenced column in the foreign key constraint. Optional.</param>
    /// <param name="constraintName">The name of the foreign key constraint. Optional.</param>
    /// <param name="onDelete">The action to take when a referenced row is deleted. Default is <see cref="DmForeignKeyAction.NoAction"/>.</param>
    /// <param name="onUpdate">The action to take when a referenced row is updated. Default is <see cref="DmForeignKeyAction.NoAction"/>.</param>
    public DmForeignKeyConstraintAttribute(
        string sourceColumnName,
        Type referencedType,
        string? referencedColumnName = null,
        string? constraintName = null,
        DmForeignKeyAction onDelete = DmForeignKeyAction.NoAction,
        DmForeignKeyAction onUpdate = DmForeignKeyAction.NoAction
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
    /// Initializes a new instance of the <see cref="DmForeignKeyConstraintAttribute"/> class.
    /// Constructor for multi-column foreign key constraint (composite keys).
    /// </summary>
    /// <param name="sourceColumnNames">The names of the source columns in the foreign key constraint.</param>
    /// <param name="referencedType">The type of the referenced entity in the foreign key constraint.</param>
    /// <param name="referencedColumnNames">The names of the referenced columns in the foreign key constraint. Optional.</param>
    /// <param name="constraintName">The name of the foreign key constraint. Optional.</param>
    /// <param name="onDelete">The action to take when a referenced row is deleted. Default is <see cref="DmForeignKeyAction.NoAction"/>.</param>
    /// <param name="onUpdate">The action to take when a referenced row is updated. Default is <see cref="DmForeignKeyAction.NoAction"/>.</param>
    public DmForeignKeyConstraintAttribute(
        string[] sourceColumnNames,
        Type referencedType,
        string[]? referencedColumnNames = null,
        string? constraintName = null,
        DmForeignKeyAction onDelete = DmForeignKeyAction.NoAction,
        DmForeignKeyAction onUpdate = DmForeignKeyAction.NoAction
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
    /// Initializes a new instance of the <see cref="DmForeignKeyConstraintAttribute"/> class.
    /// Constructor for single-column foreign key constraint.
    /// </summary>
    /// <param name="sourceColumnName">The name of the source column in the foreign key constraint.</param>
    /// <param name="referencedTableName">The name of the referenced table in the foreign key constraint.</param>
    /// <param name="referencedColumnName">The name of the referenced column in the foreign key constraint.</param>
    /// <param name="constraintName">The name of the foreign key constraint. Optional.</param>
    /// <param name="onDelete">The action to take when a referenced row is deleted. Default is <see cref="DmForeignKeyAction.NoAction"/>.</param>
    /// <param name="onUpdate">The action to take when a referenced row is updated. Default is <see cref="DmForeignKeyAction.NoAction"/>.</param>
    public DmForeignKeyConstraintAttribute(
        string sourceColumnName,
        string referencedTableName,
        string referencedColumnName,
        string? constraintName = null,
        DmForeignKeyAction onDelete = DmForeignKeyAction.NoAction,
        DmForeignKeyAction onUpdate = DmForeignKeyAction.NoAction
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
    /// Initializes a new instance of the <see cref="DmForeignKeyConstraintAttribute"/> class.
    /// Constructor for multi-column foreign key constraint (composite keys).
    /// </summary>
    /// <param name="sourceColumnNames">The names of the source columns in the foreign key constraint.</param>
    /// <param name="referencedTableName">The name of the referenced table in the foreign key constraint.</param>
    /// <param name="referencedColumnNames">The names of the referenced columns in the foreign key constraint.</param>
    /// <param name="constraintName">The name of the foreign key constraint. Optional.</param>
    /// <param name="onDelete">The action to take when a referenced row is deleted. Default is <see cref="DmForeignKeyAction.NoAction"/>.</param>
    /// <param name="onUpdate">The action to take when a referenced row is updated. Default is <see cref="DmForeignKeyAction.NoAction"/>.</param>
    public DmForeignKeyConstraintAttribute(
        string[] sourceColumnNames,
        string referencedTableName,
        string[] referencedColumnNames,
        string? constraintName = null,
        DmForeignKeyAction onDelete = DmForeignKeyAction.NoAction,
        DmForeignKeyAction onUpdate = DmForeignKeyAction.NoAction
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
    /// Initializes a new instance of the <see cref="DmForeignKeyConstraintAttribute"/> class.
    /// Constructor for single-column foreign key constraint.
    /// </summary>
    /// <param name="referencedType">The type of the referenced entity in the foreign key constraint.</param>
    /// <param name="referencedColumnName">The name of the referenced column in the foreign key constraint. Optional.</param>
    /// <param name="constraintName">The name of the foreign key constraint. Optional.</param>
    /// <param name="onDelete">The action to take when a referenced row is deleted. Default is <see cref="DmForeignKeyAction.NoAction"/>.</param>
    /// <param name="onUpdate">The action to take when a referenced row is updated. Default is <see cref="DmForeignKeyAction.NoAction"/>.</param>
    public DmForeignKeyConstraintAttribute(
        Type referencedType,
        string? referencedColumnName = null,
        string? constraintName = null,
        DmForeignKeyAction onDelete = DmForeignKeyAction.NoAction,
        DmForeignKeyAction onUpdate = DmForeignKeyAction.NoAction
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
    /// Initializes a new instance of the <see cref="DmForeignKeyConstraintAttribute"/> class.
    /// Constructor for single-column foreign key constraint.
    /// </summary>
    /// <param name="referencedTableName">The name of the referenced table in the foreign key constraint.</param>
    /// <param name="referencedColumnName">The name of the referenced column in the foreign key constraint. Optional.</param>
    /// <param name="constraintName">The name of the foreign key constraint. Optional.</param>
    /// <param name="onDelete">The action to take when a referenced row is deleted. Default is <see cref="DmForeignKeyAction.NoAction"/>.</param>
    /// <param name="onUpdate">The action to take when a referenced row is updated. Default is <see cref="DmForeignKeyAction.NoAction"/>.</param>
    public DmForeignKeyConstraintAttribute(
        string referencedTableName,
        string? referencedColumnName = null,
        string? constraintName = null,
        DmForeignKeyAction onDelete = DmForeignKeyAction.NoAction,
        DmForeignKeyAction onUpdate = DmForeignKeyAction.NoAction
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

    /// <summary>
    /// Gets the name of the foreign key constraint.
    /// </summary>
    public string? ConstraintName { get; }

    /// <summary>
    /// Gets the names of the source columns in the foreign key constraint.
    /// </summary>
    public string[]? SourceColumnNames { get; }

    /// <summary>
    /// Gets the type of the referenced entity in the foreign key constraint.
    /// </summary>
    public Type? ReferencedType { get; }

    /// <summary>
    /// Gets the name of the referenced table in the foreign key constraint.
    /// </summary>
    public string? ReferencedTableName { get; }

    /// <summary>
    /// Gets the names of the referenced columns in the foreign key constraint.
    /// </summary>
    public string[]? ReferencedColumnNames { get; }

    /// <summary>
    /// Gets the action to take when a referenced row is deleted.
    /// </summary>
    public DmForeignKeyAction? OnDelete { get; }

    /// <summary>
    /// Gets the action to take when a referenced row is updated.
    /// </summary>
    public DmForeignKeyAction? OnUpdate { get; }
}
