using System.Collections.Concurrent;

namespace DapperMatic.Providers;

public static class DataTypeMapFactory
{
    private static ConcurrentDictionary<
        DbProviderType,
        List<DataTypeMap>
    > _databaseTypeDataTypeMappings = new();

    public static void UpdateDefaultDbProviderDataTypeMap(
        DbProviderType dbProviderType,
        Func<List<DataTypeMap>, List<DataTypeMap>> updateFunc
    )
    {
        var dataTypeMap = GetDefaultDbProviderDataTypeMap(dbProviderType);
        var newDataTypeMap = updateFunc([.. dataTypeMap]);
        _databaseTypeDataTypeMappings.TryUpdate(dbProviderType, newDataTypeMap, dataTypeMap);
    }

    public static List<DataTypeMap> GetDefaultDbProviderDataTypeMap(DbProviderType databaseType)
    {
        return _databaseTypeDataTypeMappings.GetOrAdd(
            databaseType,
            dbt =>
            {
                return dbt switch
                {
                    DbProviderType.SqlServer => GetSqlServerDataTypeMap(),
                    DbProviderType.PostgreSql => GetPostgresqlDataTypeMap(),
                    DbProviderType.MySql => GetMySqlDataTypeMap(),
                    DbProviderType.Sqlite => GetSqliteDataTypeMap(),
                    _ => throw new NotSupportedException($"Database type {dbt} is not supported.")
                };
            }
        );
    }

    private static List<DataTypeMap> GetSqliteDataTypeMap()
    {
        var types = new List<DataTypeMap>
        {
            new DataTypeMap
            {
                DotnetType = typeof(string),
                SqlType = "TEXT",
                SqlTypeWithMaxLength = "TEXT",
                SqlTypeWithLength = "NVARCHAR({0})"
            },
            new DataTypeMap { DotnetType = typeof(Guid), SqlType = "TEXT" },
            new DataTypeMap { DotnetType = typeof(int), SqlType = "INTEGER" },
            new DataTypeMap { DotnetType = typeof(long), SqlType = "INTEGER" },
            new DataTypeMap { DotnetType = typeof(float), SqlType = "REAL" },
            new DataTypeMap { DotnetType = typeof(double), SqlType = "REAL" },
            new DataTypeMap
            {
                DotnetType = typeof(decimal),
                SqlType = "NUMERIC",
                SqlTypeWithPrecisionAndScale = "DECIMAL({0}, {1})"
            },
            new DataTypeMap { DotnetType = typeof(bool), SqlType = "INTEGER" },
            new DataTypeMap { DotnetType = typeof(DateTime), SqlType = "TEXT" },
            new DataTypeMap { DotnetType = typeof(DateTimeOffset), SqlType = "TEXT" },
            new DataTypeMap { DotnetType = typeof(byte[]), SqlType = "BLOB" },
            new DataTypeMap { DotnetType = typeof(Guid[]), SqlType = "TEXT" },
            new DataTypeMap { DotnetType = typeof(int[]), SqlType = "TEXT" },
            new DataTypeMap { DotnetType = typeof(long[]), SqlType = "TEXT" },
            new DataTypeMap { DotnetType = typeof(double[]), SqlType = "TEXT" },
            new DataTypeMap { DotnetType = typeof(decimal[]), SqlType = "TEXT" },
            new DataTypeMap { DotnetType = typeof(string[]), SqlType = "TEXT" },
            new DataTypeMap { DotnetType = typeof(Dictionary<string, string>), SqlType = "TEXT" },
            new DataTypeMap { DotnetType = typeof(Dictionary<string, object>), SqlType = "TEXT" }
        };

        return UnionNullableValueTypes(types);
    }

    private static List<DataTypeMap> GetMySqlDataTypeMap()
    {
        // get the type map for MySql
        var types = new List<DataTypeMap>
        {
            new DataTypeMap
            {
                DotnetType = typeof(string),
                SqlType = "VARCHAR(255)",
                SqlTypeWithLength = "VARCHAR({0})",
                SqlTypeWithMaxLength = "TEXT"
            },
            new DataTypeMap { DotnetType = typeof(Guid), SqlType = "CHAR(36)" },
            new DataTypeMap { DotnetType = typeof(int), SqlType = "INT" },
            new DataTypeMap { DotnetType = typeof(long), SqlType = "BIGINT" },
            new DataTypeMap { DotnetType = typeof(float), SqlType = "FLOAT" },
            new DataTypeMap { DotnetType = typeof(double), SqlType = "DOUBLE" },
            new DataTypeMap { DotnetType = typeof(decimal), SqlType = "DECIMAL" },
            new DataTypeMap { DotnetType = typeof(bool), SqlType = "TINYINT" },
            new DataTypeMap { DotnetType = typeof(DateTime), SqlType = "DATETIME" },
            new DataTypeMap { DotnetType = typeof(DateTimeOffset), SqlType = "DATETIME" },
            new DataTypeMap { DotnetType = typeof(byte[]), SqlType = "BLOB" },
            new DataTypeMap { DotnetType = typeof(Guid[]), SqlType = "TEXT" },
            new DataTypeMap { DotnetType = typeof(int[]), SqlType = "TEXT" },
            new DataTypeMap { DotnetType = typeof(long[]), SqlType = "TEXT" },
            new DataTypeMap { DotnetType = typeof(double[]), SqlType = "TEXT" },
            new DataTypeMap { DotnetType = typeof(decimal[]), SqlType = "TEXT" },
            new DataTypeMap { DotnetType = typeof(string[]), SqlType = "TEXT" },
            new DataTypeMap { DotnetType = typeof(Dictionary<string, string>), SqlType = "TEXT" },
            new DataTypeMap { DotnetType = typeof(Dictionary<string, object>), SqlType = "TEXT" }
        };

        return UnionNullableValueTypes(types);
    }

    private static List<DataTypeMap> GetPostgresqlDataTypeMap()
    {
        // get the type map for Postgresql
        var types = new List<DataTypeMap>
        {
            new DataTypeMap
            {
                DotnetType = typeof(string),
                SqlType = "CHARACTER VARYING",
                SqlTypeWithLength = "CHARACTER VARYING({0})",
                SqlTypeWithMaxLength = "TEXT"
            },
            new DataTypeMap { DotnetType = typeof(Guid), SqlType = "UUID" },
            new DataTypeMap { DotnetType = typeof(int), SqlType = "INTEGER" },
            new DataTypeMap { DotnetType = typeof(long), SqlType = "BIGINT" },
            new DataTypeMap { DotnetType = typeof(float), SqlType = "REAL" },
            new DataTypeMap { DotnetType = typeof(double), SqlType = "DOUBLE PRECISION" },
            new DataTypeMap { DotnetType = typeof(decimal), SqlType = "DECIMAL" },
            new DataTypeMap { DotnetType = typeof(bool), SqlType = "BOOLEAN" },
            new DataTypeMap { DotnetType = typeof(DateTime), SqlType = "TIMESTAMP" },
            new DataTypeMap
            {
                DotnetType = typeof(DateTimeOffset),
                SqlType = "TIMESTAMP WITH TIME ZONE"
            },
            new DataTypeMap { DotnetType = typeof(byte[]), SqlType = "BYTEA" },
            new DataTypeMap { DotnetType = typeof(Guid[]), SqlType = "UUID[]" },
            new DataTypeMap { DotnetType = typeof(int[]), SqlType = "INTEGER[]" },
            new DataTypeMap { DotnetType = typeof(long[]), SqlType = "BIGINT[]" },
            new DataTypeMap { DotnetType = typeof(double[]), SqlType = "DOUBLE PRECISION[]" },
            new DataTypeMap { DotnetType = typeof(decimal[]), SqlType = "DECIMAL[]" },
            new DataTypeMap { DotnetType = typeof(string[]), SqlType = "CHARACTER VARYING[]" },
            new DataTypeMap { DotnetType = typeof(Dictionary<string, string>), SqlType = "JSONB" },
            new DataTypeMap { DotnetType = typeof(Dictionary<string, object>), SqlType = "JSONB" }
        };

        return UnionNullableValueTypes(types);
    }

    private static List<DataTypeMap> GetSqlServerDataTypeMap()
    {
        // get the type map for SqlServer
        var types = new List<DataTypeMap>
        {
            new DataTypeMap
            {
                DotnetType = typeof(string),
                SqlType = "NVARCHAR",
                SqlTypeWithLength = "NVARCHAR({0})",
                SqlTypeWithMaxLength = "NVARCHAR(MAX)"
            },
            new DataTypeMap { DotnetType = typeof(Guid), SqlType = "UNIQUEIDENTIFIER" },
            new DataTypeMap { DotnetType = typeof(int), SqlType = "INT" },
            new DataTypeMap { DotnetType = typeof(long), SqlType = "BIGINT" },
            new DataTypeMap { DotnetType = typeof(float), SqlType = "REAL" },
            new DataTypeMap { DotnetType = typeof(double), SqlType = "FLOAT" },
            new DataTypeMap { DotnetType = typeof(decimal), SqlType = "DECIMAL" },
            new DataTypeMap { DotnetType = typeof(bool), SqlType = "BIT" },
            new DataTypeMap { DotnetType = typeof(DateTime), SqlType = "DATETIME2" },
            new DataTypeMap { DotnetType = typeof(DateTimeOffset), SqlType = "DATETIMEOFFSET" },
            new DataTypeMap { DotnetType = typeof(byte[]), SqlType = "VARBINARY" },
            new DataTypeMap { DotnetType = typeof(Guid[]), SqlType = "NVARCHAR(MAX)" },
            new DataTypeMap { DotnetType = typeof(int[]), SqlType = "NVARCHAR(MAX)" },
            new DataTypeMap { DotnetType = typeof(long[]), SqlType = "NVARCHAR(MAX)" },
            new DataTypeMap { DotnetType = typeof(double[]), SqlType = "NVARCHAR(MAX)" },
            new DataTypeMap { DotnetType = typeof(decimal[]), SqlType = "NVARCHAR(MAX)" },
            new DataTypeMap { DotnetType = typeof(string[]), SqlType = "NVARCHAR(MAX)" },
            new DataTypeMap
            {
                DotnetType = typeof(Dictionary<string, string>),
                SqlType = "NVARCHAR(MAX)"
            },
            new DataTypeMap
            {
                DotnetType = typeof(Dictionary<string, object>),
                SqlType = "NVARCHAR(MAX)"
            }
        };

        return UnionNullableValueTypes(types);
    }

    private static List<DataTypeMap> UnionNullableValueTypes(List<DataTypeMap> types)
    {
        // add nullable version of all the value types
        foreach (var type in types.ToArray())
        {
            if (type.DotnetType.IsValueType)
            {
                types.Add(
                    new DataTypeMap
                    {
                        DotnetType = typeof(Nullable<>).MakeGenericType(type.DotnetType),
                        SqlType = type.SqlType,
                        SqlTypeWithLength = type.SqlTypeWithLength,
                        SqlTypeWithMaxLength = type.SqlTypeWithMaxLength,
                        SqlTypeWithPrecisionAndScale = type.SqlTypeWithPrecisionAndScale
                    }
                );
            }
        }

        return types;
    }
}
