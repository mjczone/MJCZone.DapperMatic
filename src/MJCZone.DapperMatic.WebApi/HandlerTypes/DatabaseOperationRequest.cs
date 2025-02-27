namespace MJCZone.DapperMatic.WebApi.HandlerTypes;

/// <summary>
/// Represents a request for a database operation.
/// </summary>
public class DatabaseOperationRequest
{
    /// <summary>
    /// Converts the <see cref="DatabaseOperationRequest"/> to a <see cref="DatabaseOperation"/>.
    /// </summary>
    /// <returns>The <see cref="DatabaseOperation"/>.</returns>
    internal DatabaseOperation ToDatabaseOperation()
    {
        throw new NotImplementedException();
    }
}
