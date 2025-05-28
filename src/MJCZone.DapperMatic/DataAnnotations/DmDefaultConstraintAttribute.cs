namespace MJCZone.DapperMatic.DataAnnotations;

/// <summary>
/// Attribute to define a default constraint on a property.
/// </summary>
/// <example>
/// [DmDefaultConstraint("0")]
/// public int Age { get; set; }
/// </example>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true, Inherited = false)]
public sealed class DmDefaultConstraintAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DmDefaultConstraintAttribute"/> class.
    /// </summary>
    /// <param name="expression">The default value expression.</param>
    /// <param name="constraintName">The name of the constraint.</param>
    public DmDefaultConstraintAttribute(string expression, string? constraintName = null)
    {
        if (string.IsNullOrWhiteSpace(expression))
        {
            throw new ArgumentException("Expression is required", nameof(expression));
        }

        Expression = expression;
        ConstraintName = constraintName;
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
