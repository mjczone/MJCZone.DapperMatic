namespace MJCZone.DapperMatic.DataAnnotations;

/// <summary>
/// Check Constraint Attribute.
/// </summary>
/// <example>
/// [DmCheckConstraint("Age > 18")]
/// public int Age { get; set; }
/// ...
/// </example>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
public sealed class DmCheckConstraintAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DmCheckConstraintAttribute"/> class with the specified expression.
    /// </summary>
    /// <param name="expression">The check constraint expression.</param>
    /// <exception cref="ArgumentException">Thrown when the expression is null or whitespace.</exception>
    public DmCheckConstraintAttribute(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            throw new ArgumentException("Expression is required", nameof(expression));
        }

        Expression = expression;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DmCheckConstraintAttribute"/> class with the specified constraint name and expression.
    /// </summary>
    /// <param name="constraintName">The name of the check constraint.</param>
    /// <param name="expression">The check constraint expression.</param>
    /// <exception cref="ArgumentException">Thrown when the constraint name or expression is null or whitespace.</exception>
    public DmCheckConstraintAttribute(string constraintName, string expression)
    {
        if (string.IsNullOrWhiteSpace(constraintName))
        {
            throw new ArgumentException("Constraint name is required", nameof(constraintName));
        }

        if (string.IsNullOrWhiteSpace(expression))
        {
            throw new ArgumentException("Expression is required", nameof(expression));
        }

        ConstraintName = constraintName;
        Expression = expression;
    }

    /// <summary>
    /// Gets the name of the check constraint.
    /// </summary>
    public string? ConstraintName { get; }

    /// <summary>
    /// Gets the check constraint expression.
    /// </summary>
    public string Expression { get; }
}
