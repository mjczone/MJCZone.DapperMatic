using System.Data;
using MJCZone.DapperMatic.Interfaces;

namespace MJCZone.DapperMatic.Providers.Sqlite;

/// <summary>
///  Provides SQLite specific database methods.
/// </summary>
public class SqliteMethodsFactory : DatabaseMethodsFactoryBase
{
    /// <summary>
    ///  Initializes a new instance of the <see cref="SqliteMethodsFactory"/> class.
    /// </summary>
    /// <param name="db">Database connection.</param>
    /// <returns>true/false.</returns>
    public virtual bool SupportsConnectionCustom(IDbConnection db) => false;

    /// <inheritdoc/>
    public override bool SupportsConnection(IDbConnection db) =>
        SupportsConnectionCustom(db)
        || (db.GetType().FullName ?? string.Empty).Contains(
            "sqlite",
            StringComparison.OrdinalIgnoreCase
        );

    /// <inheritdoc/>
    protected override IDatabaseMethods CreateMethodsCore() => new SqliteMethods();
}
