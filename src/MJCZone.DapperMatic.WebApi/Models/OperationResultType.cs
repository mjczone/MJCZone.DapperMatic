namespace MJCZone.DapperMatic.WebApi.Models;

/// <summary>
/// Represents the type of result that a query can return.
/// </summary>
public enum OperationResultType : int
{
    /// <summary>
    /// Indicates that the query does not return any result.
    /// </summary>
    Void = 0,

    /// <summary>
    /// Indicates that the query returns a single scalar value.
    /// </summary>
    Scalar = 1,

    /// <summary>
    /// Indicates that the query returns a single object.
    /// </summary>
    Object = 2,

    /// <summary>
    /// Indicates that the query returns a single result set.
    /// </summary>
    SingleResultset = 3,

    /// <summary>
    /// Indicates that the query returns multiple result sets.
    /// </summary>
    MultipleResultsets = 4,
}
