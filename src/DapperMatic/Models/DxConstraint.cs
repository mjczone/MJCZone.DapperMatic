namespace DapperMatic.Models;

/// <summary>
/// Represents a constraint on a table.
/// </summary>
public abstract class DxConstraint
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DxConstraint"/> class.
    /// </summary>
    /// <param name="constraintName">The name of the constraint.</param>
    protected DxConstraint(string constraintName)
    {
        ConstraintName = constraintName;
    }

    /// <summary>
    /// Gets the type of the constraint.
    /// </summary>
    public abstract DxConstraintType ConstraintType { get; }

    /// <summary>
    /// Gets or sets the name of the constraint.
    /// </summary>
    public string ConstraintName { get; set; }
}
