using System.Data;
using DapperMatic.Interfaces;

namespace DapperMatic.Providers.MySql;

public class MySqlMethodsFactory : DatabaseMethodsFactoryBase
{
    public virtual bool SupportsConnectionCustom(IDbConnection db) => false;

    public override bool SupportsConnection(IDbConnection db) =>
        SupportsConnectionCustom(db)
        || (db.GetType().FullName ?? "").Contains(
            "mysql",
            StringComparison.OrdinalIgnoreCase
        );

    protected override IDatabaseMethods CreateMethodsCore() => new MySqlMethods();
}
