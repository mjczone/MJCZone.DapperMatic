namespace MJCZone.DapperMatic.DataAnnotations;

/// <summary>
/// Attribute to define a default constraint on a property.
/// </summary>
/// <example>
/// [DmDefaultConstraint("0")]
/// public int Age { get; set; }
/// ...
/// </example>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public sealed class DmDefaultConstraintAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DmDefaultConstraintAttribute"/> class with an expression.
    /// </summary>
    /// <param name="expression">The default value expression.</param>
    public DmDefaultConstraintAttribute(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            throw new ArgumentException("Expression is required", nameof(expression));
        }

        Expression = expression;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DmDefaultConstraintAttribute"/> class with a constraint name and expression.
    /// </summary>
    /// <param name="constraintName">The name of the constraint.</param>
    /// <param name="expression">The default value expression.</param>
    public DmDefaultConstraintAttribute(string constraintName, string expression)
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
    /// Gets the name of the constraint.
    /// </summary>
    public string? ConstraintName { get; }

    /// <summary>
    /// Gets the default value expression.
    /// </summary>
    public string Expression { get; }
}
