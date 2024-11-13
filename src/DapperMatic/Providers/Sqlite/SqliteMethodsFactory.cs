using System.Data;
using DapperMatic.Interfaces;

namespace DapperMatic.Providers.Sqlite;

public class SqliteMethodsFactory : DatabaseMethodsFactoryBase
{
    public virtual bool SupportsConnectionCustom(IDbConnection db) => false;

    public override bool SupportsConnection(IDbConnection db) =>
        SupportsConnectionCustom(db)
        || (db.GetType().FullName ?? "").Contains(
            "sqlite",
            StringComparison.OrdinalIgnoreCase
        );

    protected override IDatabaseMethods CreateMethodsCore() => new SqliteMethods();
}
