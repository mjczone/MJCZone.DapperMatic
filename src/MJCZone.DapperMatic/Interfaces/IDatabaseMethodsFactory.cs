using System.Data;

namespace MJCZone.DapperMatic.Interfaces;

/// <summary>
/// Factory interface for creating database methods.
/// </summary>
public interface IDatabaseMethodsFactory
{
    /// <summary>
    /// Determines whether the factory supports the specified database connection.
    /// </summary>
    /// <param name="db">The database connection.</param>
    /// <returns><c>true</c> if the factory supports the specified database connection; otherwise, <c>false</c>.</returns>
    bool SupportsConnection(IDbConnection db);

    /// <summary>
    /// Gets the database methods for the specified database connection.
    /// </summary>
    /// <param name="db">The database connection.</param>
    /// <returns>An instance of <see cref="IDatabaseMethods"/> for the specified database connection.</returns>
    IDatabaseMethods GetMethods(IDbConnection db);
}
