using System.Data;
using System.Reflection.Metadata.Ecma335;
using DapperMatic.Interfaces;

namespace DapperMatic.Providers.SqlServer;

public class SqlServerMethodsFactory : DatabaseMethodsFactoryBase
{
    public virtual bool SupportsConnectionCustom(IDbConnection db) => false;

    public override bool SupportsConnection(IDbConnection db)
    {
        var typeName = db.GetType().FullName;
        return SupportsConnectionCustom(db)
            || (typeName == "System.Data.SqlClient.SqlConnection")
            || (typeName == "System.Data.SqlServerCe.SqlCeConnection")
            || (typeName == "Microsoft.Data.SqlClient.SqlConnection");
    }

    protected override IDatabaseMethods CreateMethodsCore() => new SqlServerMethods();
}
