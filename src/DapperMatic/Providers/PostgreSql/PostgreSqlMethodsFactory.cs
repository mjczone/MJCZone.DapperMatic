using System.Data;
using DapperMatic.Interfaces;

namespace DapperMatic.Providers.PostgreSql;

/// <summary>
/// Factory class for creating PostgreSQL specific database methods.
/// </summary>
public class PostgreSqlMethodsFactory : DatabaseMethodsFactoryBase
{
    /// <summary>
    /// Determines whether the specified database connection supports custom PostgreSQL connection.
    /// </summary>
    /// <param name="db">The database connection.</param>
    /// <returns><c>true</c> if the connection supports custom PostgreSQL connection; otherwise, <c>false</c>.</returns>
    public virtual bool SupportsConnectionCustom(IDbConnection db) => false;

    /// <summary>
    /// Determines whether the specified database connection is a PostgreSQL connection.
    /// </summary>
    /// <param name="db">The database connection.</param>
    /// <returns><c>true</c> if the connection is a PostgreSQL connection; otherwise, <c>false</c>.</returns>
    public override bool SupportsConnection(IDbConnection db) =>
        SupportsConnectionCustom(db)
        || (db.GetType().FullName ?? string.Empty).Contains(
            "pg",
            StringComparison.OrdinalIgnoreCase
        )
        || (db.GetType().FullName ?? string.Empty).Contains(
            "postgres",
            StringComparison.OrdinalIgnoreCase
        );

    /// <summary>
    /// Creates the core PostgreSQL database methods.
    /// </summary>
    /// <returns>The PostgreSQL database methods.</returns>
    protected override IDatabaseMethods CreateMethodsCore() => new PostgreSqlMethods();
}
