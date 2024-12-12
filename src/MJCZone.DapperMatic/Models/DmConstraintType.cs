namespace MJCZone.DapperMatic.Models;

/// <summary>
/// Specifies the type of a database constraint.
/// </summary>
[Serializable]
public enum DmConstraintType
{
    /// <summary>
    /// Represents a primary key constraint.
    /// </summary>
    PrimaryKey,

    /// <summary>
    /// Represents a foreign key constraint.
    /// </summary>
    ForeignKey,

    /// <summary>
    /// Represents a unique constraint.
    /// </summary>
    Unique,

    /// <summary>
    /// Represents a check constraint.
    /// </summary>
    Check,

    /// <summary>
    /// Represents a default constraint.
    /// </summary>
    Default,
}
