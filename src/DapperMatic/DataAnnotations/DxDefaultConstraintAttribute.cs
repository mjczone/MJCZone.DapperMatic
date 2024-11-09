namespace DapperMatic.DataAnnotations;

/// <summary>
/// Check Constraint Attribute
/// </summary>
/// <example>
/// [DxDefaultConstraint("0")]
/// public int Age { get; set; }
/// </example>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class DxDefaultConstraintAttribute : Attribute
{
    public DxDefaultConstraintAttribute(string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            throw new ArgumentException("Expression is required", nameof(expression));

        Expression = expression;
    }

    public DxDefaultConstraintAttribute(string constraintName, string expression)
    {
        if (string.IsNullOrWhiteSpace(constraintName))
            throw new ArgumentException("Constraint name is required", nameof(constraintName));

        if (string.IsNullOrWhiteSpace(expression))
            throw new ArgumentException("Expression is required", nameof(expression));

        ConstraintName = constraintName;
        Expression = expression;
    }

    public string? ConstraintName { get; }
    public string Expression { get; }
}
