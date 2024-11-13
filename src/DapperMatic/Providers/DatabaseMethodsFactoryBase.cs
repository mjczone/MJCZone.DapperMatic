using System.Collections.Concurrent;
using System.Data;
using DapperMatic.Interfaces;

namespace DapperMatic.Providers;

// Base implementation that other providers can inherit from
public abstract class DatabaseMethodsFactoryBase : IDatabaseMethodsFactory
{
    private readonly ConcurrentDictionary<Type, IDatabaseMethods> _methodsCache = new();

    public IDatabaseMethods GetMethods(IDbConnection db)
    {
        if (!SupportsConnection(db))
            throw new NotSupportedException(
                $"Connection type {db.GetType().FullName} is not supported by this factory."
            );

        return _methodsCache.GetOrAdd(db.GetType(), _ => CreateMethodsCore());
    }

    public abstract bool SupportsConnection(IDbConnection db);
    protected abstract IDatabaseMethods CreateMethodsCore();
}
