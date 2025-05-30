namespace MJCZone.DapperMatic.Models;

/// <summary>
/// Specifies the order of a column in an index or constraint.
/// </summary>
[Serializable]
public enum DmColumnOrder
{
    /// <summary>
    /// Specifies that the column is sorted in ascending order.
    /// </summary>
    Ascending,

    /// <summary>
    /// Specifies that the column is sorted in descending order.
    /// </summary>
    Descending,
}
