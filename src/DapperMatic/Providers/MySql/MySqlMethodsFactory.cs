using System.Data;
using DapperMatic.Interfaces;

namespace DapperMatic.Providers.MySql;

/// <summary>
/// Factory class for creating MySQL specific database methods.
/// </summary>
public class MySqlMethodsFactory : DatabaseMethodsFactoryBase
{
    /// <summary>
    /// Determines whether the specified database connection supports custom connection settings.
    /// </summary>
    /// <param name="db">The database connection.</param>
    /// <returns><c>true</c> if the connection supports custom settings; otherwise, <c>false</c>.</returns>
    public virtual bool SupportsConnectionCustom(IDbConnection db) => false;

    /// <summary>
    /// Determines whether the specified database connection is supported.
    /// </summary>
    /// <param name="db">The database connection.</param>
    /// <returns><c>true</c> if the connection is supported; otherwise, <c>false</c>.</returns>
    public override bool SupportsConnection(IDbConnection db) =>
        SupportsConnectionCustom(db)
        || (db.GetType().FullName ?? string.Empty).Contains(
            "mysql",
            StringComparison.OrdinalIgnoreCase
        );

    /// <summary>
    /// Creates the core database methods for MySQL.
    /// </summary>
    /// <returns>An instance of <see cref="IDatabaseMethods"/> for MySQL.</returns>
    protected override IDatabaseMethods CreateMethodsCore() => new MySqlMethods();
}
