using System.Data;

namespace DapperMatic.Interfaces;

public interface IDatabaseMethodsFactory
{
    bool SupportsConnection(IDbConnection db);
    IDatabaseMethods GetMethods(IDbConnection db);
}