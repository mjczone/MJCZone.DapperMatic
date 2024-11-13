using System.Data;
using DapperMatic.Interfaces;

namespace DapperMatic.Providers.PostgreSql;

public class PostgreSqlMethodsFactory : DatabaseMethodsFactoryBase
{
    public virtual bool SupportsConnectionCustom(IDbConnection db) => false;

    public override bool SupportsConnection(IDbConnection db) =>
        SupportsConnectionCustom(db)
        || (db.GetType().FullName ?? "").Contains("pg", StringComparison.OrdinalIgnoreCase)
        || (db.GetType().FullName ?? "").Contains(
            "postgres",
            StringComparison.OrdinalIgnoreCase
        );

    protected override IDatabaseMethods CreateMethodsCore() => new PostgreSqlMethods();
}