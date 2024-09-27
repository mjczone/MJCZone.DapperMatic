namespace DapperMatic.DataAnnotations;

/// <summary>
/// Check Constraint Attribute
/// </summary>
/// <example>
/// [DxDefaultConstraint("0")]
/// public int Age { get; set; }
/// </example>
[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = true)]
public class DxDefaultConstraintAttribute : Attribute
{
    public DxDefaultConstraintAttribute(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            throw new ArgumentException("Expression cannot be null or empty", nameof(expression));

        Expression = expression;
    }

    public DxDefaultConstraintAttribute(string constraintName, string expression)
    {
        if (string.IsNullOrWhiteSpace(constraintName))
            throw new ArgumentException(
                "Constraint name cannot be null or empty",
                nameof(constraintName)
            );

        if (string.IsNullOrWhiteSpace(expression))
            throw new ArgumentException("Expression cannot be null or empty", nameof(expression));

        ConstraintName = constraintName;
        Expression = expression;
    }

    public string? ConstraintName { get; }
    public string Expression { get; }
}
