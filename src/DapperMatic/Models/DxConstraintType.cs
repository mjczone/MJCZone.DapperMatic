namespace DapperMatic.Models;

[Serializable]
public enum DxConstraintType
{
    PrimaryKey,
    ForeignKey,
    Unique,
    Check,
    Default
}