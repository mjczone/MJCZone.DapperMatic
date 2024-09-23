namespace DapperMatic.Models;

public abstract class DxConstraint
{
    protected DxConstraint(string constraintName)
    {
        ConstraintName = constraintName;
    }
    
    public abstract DxConstraintType ConstraintType { get; }

    public string ConstraintName { get; set; }
}
