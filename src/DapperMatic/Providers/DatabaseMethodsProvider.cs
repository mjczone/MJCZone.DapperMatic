using System.Collections.Concurrent;
using System.Data;
using DapperMatic.Interfaces;

namespace DapperMatic.Providers
{
    public static class DatabaseMethodsProvider
    {
        private static readonly ConcurrentDictionary<
            DbProviderType,
            IDatabaseMethodsFactory
        > NativeFactories =
            new()
            {
                [DbProviderType.Sqlite] = new Sqlite.SqliteMethodsFactory(),
                [DbProviderType.SqlServer] = new SqlServer.SqlServerMethodsFactory(),
                [DbProviderType.MySql] = new MySql.MySqlMethodsFactory(),
                [DbProviderType.PostgreSql] = new PostgreSql.PostgreSqlMethodsFactory()
            };

        private static readonly ConcurrentDictionary<
            string,
            IDatabaseMethodsFactory
        > CustomFactories = new();

        public static void RegisterFactory(string name, IDatabaseMethodsFactory factory)
        {
            CustomFactories.TryAdd(name.ToLower(), factory);
        }

        public static void RegisterFactory(
            DbProviderType providerType,
            IDatabaseMethodsFactory factory
        )
        {
            if (providerType == DbProviderType.Other)
            {
                RegisterFactory(Guid.NewGuid().ToString(), factory);
                return;
            }

            NativeFactories.AddOrUpdate(providerType, factory, (_, _) => factory);
        }

        public static IDatabaseMethods GetMethods(IDbConnection db)
        {
            foreach (var factory in CustomFactories.Values)
            {
                if (factory.SupportsConnection(db))
                    return factory.GetMethods(db);
            }

            foreach (var factory in NativeFactories.Values)
            {
                if (factory.SupportsConnection(db))
                    return factory.GetMethods(db);
            }

            throw new NotSupportedException(
                $"No factory found for connection type {db.GetType().FullName}"
            );
        }
    }
}
