namespace DapperMatic.Providers.Sqlite;

public sealed class SqliteProviderTypeMap : ProviderTypeMapBase<SqliteProviderTypeMap>
{
    internal static readonly Lazy<SqliteProviderTypeMap> Instance =
        new(() => new SqliteProviderTypeMap());

    #region Default Provider SQL Types

    private static readonly ProviderSqlType[] DefaultProviderSqlTypes =
    [
        new ProviderSqlType("integer", null, null, null, null, null, true, false, null, null, null),
        new ProviderSqlType(
            "int",
            "integer",
            null,
            null,
            null,
            null,
            false,
            false,
            null,
            null,
            null
        ),
        new ProviderSqlType("real", null, null, null, null, null, false, false, null, null, null),
        new ProviderSqlType(
            "float",
            "real",
            null,
            null,
            null,
            null,
            false,
            false,
            null,
            null,
            null
        ),
        new ProviderSqlType(
            "double",
            "real",
            null,
            null,
            null,
            null,
            false,
            false,
            null,
            null,
            null
        ),
        new ProviderSqlType(
            "numeric",
            null,
            null,
            "numeric({0})",
            "numeric({0},{1})",
            null,
            false,
            false,
            null,
            12,
            2
        ),
        new ProviderSqlType(
            "decimal",
            "numeric",
            null,
            "decimal({0})",
            "decimal({0},{1})",
            null,
            false,
            false,
            null,
            12,
            2
        ),
        new ProviderSqlType(
            "bool",
            "numeric",
            null,
            null,
            null,
            null,
            false,
            false,
            null,
            null,
            null
        ),
        new ProviderSqlType(
            "boolean",
            "numeric",
            null,
            null,
            null,
            null,
            false,
            false,
            null,
            null,
            null
        ),
        new ProviderSqlType(
            "datetime",
            "numeric",
            null,
            null,
            null,
            null,
            false,
            false,
            null,
            null,
            null
        ),
        new ProviderSqlType(
            "timestamp",
            "numeric",
            null,
            null,
            null,
            null,
            false,
            false,
            null,
            null,
            null
        ),
        new ProviderSqlType(
            "time",
            "numeric",
            null,
            null,
            null,
            null,
            false,
            false,
            null,
            null,
            null
        ),
        new ProviderSqlType(
            "date",
            "numeric",
            null,
            null,
            null,
            null,
            false,
            false,
            null,
            null,
            null
        ),
        new ProviderSqlType(
            "year",
            "numeric",
            null,
            null,
            null,
            null,
            false,
            false,
            null,
            null,
            null
        ),
        new ProviderSqlType("text", null, null, null, null, null, false, false, null, null, null),
        new ProviderSqlType(
            "char",
            "text",
            "char({0})",
            null,
            null,
            null,
            false,
            false,
            null,
            null,
            null
        ),
        new ProviderSqlType(
            "nchar",
            "text",
            "nchar({0})",
            null,
            null,
            null,
            false,
            false,
            null,
            null,
            null
        ),
        new ProviderSqlType(
            "varchar",
            "text",
            "varchar({0})",
            null,
            null,
            null,
            false,
            false,
            null,
            null,
            null
        ),
        new ProviderSqlType(
            "nvarchar",
            "text",
            "nvarchar({0})",
            null,
            null,
            null,
            false,
            false,
            null,
            null,
            null
        ),
        new ProviderSqlType(
            "varying character",
            "text",
            "varying character({0})",
            null,
            null,
            null,
            false,
            false,
            null,
            null,
            null
        ),
        new ProviderSqlType(
            "native character",
            "text",
            "native character({0})",
            null,
            null,
            null,
            false,
            false,
            null,
            null,
            null
        ),
        new ProviderSqlType("clob", "text", null, null, null, null, false, false, null, null, null),
        new ProviderSqlType("blob", null, null, null, null, null, false, false, null, null, null),
    ];

    private static readonly SqlTypeToDotnetTypeMap[] DefaultSqlTypeToDotnetTypeMap =
    [
        new SqlTypeToDotnetTypeMap(
            "integer",
            typeof(int),
            [
                typeof(byte),
                typeof(short),
                typeof(int),
                typeof(long),
                typeof(bool),
                typeof(float),
                typeof(double),
                typeof(decimal),
                typeof(DateTime),
                typeof(DateTimeOffset),
                typeof(TimeSpan),
                typeof(string)
            ]
        ),
        new SqlTypeToDotnetTypeMap(
            "int",
            typeof(int),
            [
                typeof(byte),
                typeof(short),
                typeof(int),
                typeof(long),
                typeof(bool),
                typeof(float),
                typeof(double),
                typeof(decimal),
                typeof(DateTime),
                typeof(DateTimeOffset),
                typeof(TimeSpan),
                typeof(string)
            ]
        ),
        new SqlTypeToDotnetTypeMap(
            "real",
            typeof(decimal),
            [
                typeof(byte),
                typeof(short),
                typeof(int),
                typeof(long),
                typeof(bool),
                typeof(float),
                typeof(double),
                typeof(decimal),
                typeof(DateTime),
                typeof(DateTimeOffset),
                typeof(TimeSpan),
                typeof(string)
            ]
        ),
        new SqlTypeToDotnetTypeMap(
            "float",
            typeof(decimal),
            [
                typeof(byte),
                typeof(short),
                typeof(int),
                typeof(long),
                typeof(bool),
                typeof(float),
                typeof(double),
                typeof(decimal)
            ]
        ),
        new SqlTypeToDotnetTypeMap(
            "double",
            typeof(decimal),
            [
                typeof(byte),
                typeof(short),
                typeof(int),
                typeof(long),
                typeof(bool),
                typeof(float),
                typeof(double),
                typeof(decimal)
            ]
        ),
        new SqlTypeToDotnetTypeMap(
            "numeric",
            typeof(decimal),
            [
                typeof(byte),
                typeof(short),
                typeof(int),
                typeof(long),
                typeof(bool),
                typeof(float),
                typeof(double),
                typeof(decimal)
            ]
        ),
        new SqlTypeToDotnetTypeMap(
            "decimal",
            typeof(decimal),
            [
                typeof(byte),
                typeof(short),
                typeof(int),
                typeof(long),
                typeof(bool),
                typeof(float),
                typeof(double),
                typeof(decimal)
            ]
        ),
        new SqlTypeToDotnetTypeMap("bool", typeof(bool), [typeof(bool)]),
        new SqlTypeToDotnetTypeMap("boolean", typeof(bool), [typeof(bool)]),
        new SqlTypeToDotnetTypeMap(
            "datetime",
            typeof(DateTime),
            [typeof(DateTime), typeof(DateTimeOffset), typeof(TimeSpan)]
        ),
        new SqlTypeToDotnetTypeMap(
            "timestamp",
            typeof(DateTimeOffset),
            [typeof(DateTime), typeof(DateTimeOffset), typeof(TimeSpan)]
        ),
        new SqlTypeToDotnetTypeMap(
            "time",
            typeof(DateTime),
            [typeof(DateTime), typeof(DateTimeOffset), typeof(TimeSpan)]
        ),
        new SqlTypeToDotnetTypeMap(
            "date",
            typeof(DateTime),
            [typeof(DateTime), typeof(DateTimeOffset), typeof(TimeSpan)]
        ),
        new SqlTypeToDotnetTypeMap(
            "year",
            typeof(DateTime),
            [typeof(DateTime), typeof(DateTimeOffset), typeof(TimeSpan)]
        ),
        new SqlTypeToDotnetTypeMap(
            "text",
            typeof(string),
            [
                typeof(byte),
                typeof(short),
                typeof(int),
                typeof(long),
                typeof(bool),
                typeof(float),
                typeof(double),
                typeof(decimal),
                typeof(DateTime),
                typeof(DateTimeOffset),
                typeof(TimeSpan),
                typeof(object),
                typeof(string),
                typeof(Guid),
                typeof(IDictionary<,>),
                typeof(Dictionary<,>),
                typeof(IEnumerable<>),
                typeof(ICollection<>),
                typeof(List<>),
                typeof(object[])
            ]
        ),
        new SqlTypeToDotnetTypeMap(
            "char",
            typeof(string),
            [
                typeof(byte),
                typeof(short),
                typeof(int),
                typeof(long),
                typeof(bool),
                typeof(float),
                typeof(double),
                typeof(decimal),
                typeof(DateTime),
                typeof(DateTimeOffset),
                typeof(TimeSpan),
                typeof(string),
                typeof(Guid)
            ]
        ),
        new SqlTypeToDotnetTypeMap(
            "nchar",
            typeof(string),
            [
                typeof(byte),
                typeof(short),
                typeof(int),
                typeof(long),
                typeof(bool),
                typeof(float),
                typeof(double),
                typeof(decimal),
                typeof(DateTime),
                typeof(DateTimeOffset),
                typeof(TimeSpan),
                typeof(string),
                typeof(Guid)
            ]
        ),
        new SqlTypeToDotnetTypeMap(
            "varchar",
            typeof(string),
            [
                typeof(byte),
                typeof(short),
                typeof(int),
                typeof(long),
                typeof(bool),
                typeof(float),
                typeof(double),
                typeof(decimal),
                typeof(DateTime),
                typeof(DateTimeOffset),
                typeof(TimeSpan),
                typeof(string),
                typeof(Guid),
                typeof(Dictionary<,>),
                typeof(IEnumerable<>),
                typeof(ICollection<>),
                typeof(List<>),
                typeof(object[])
            ]
        ),
        new SqlTypeToDotnetTypeMap(
            "nvarchar",
            typeof(string),
            [
                typeof(byte),
                typeof(short),
                typeof(int),
                typeof(long),
                typeof(bool),
                typeof(float),
                typeof(double),
                typeof(decimal),
                typeof(DateTime),
                typeof(DateTimeOffset),
                typeof(TimeSpan),
                typeof(string),
                typeof(Guid),
                typeof(Dictionary<,>),
                typeof(IEnumerable<>),
                typeof(ICollection<>),
                typeof(List<>),
                typeof(object[])
            ]
        ),
        new SqlTypeToDotnetTypeMap(
            "varying character",
            typeof(string),
            [
                typeof(byte),
                typeof(short),
                typeof(int),
                typeof(long),
                typeof(bool),
                typeof(float),
                typeof(double),
                typeof(decimal),
                typeof(DateTime),
                typeof(DateTimeOffset),
                typeof(TimeSpan),
                typeof(string),
                typeof(Guid),
                typeof(Dictionary<,>),
                typeof(IEnumerable<>),
                typeof(ICollection<>),
                typeof(List<>),
                typeof(object[])
            ]
        ),
        new SqlTypeToDotnetTypeMap(
            "native character",
            typeof(string),
            [
                typeof(byte),
                typeof(short),
                typeof(int),
                typeof(long),
                typeof(bool),
                typeof(float),
                typeof(double),
                typeof(decimal),
                typeof(DateTime),
                typeof(DateTimeOffset),
                typeof(TimeSpan),
                typeof(string),
                typeof(Guid),
                typeof(Dictionary<,>),
                typeof(IEnumerable<>),
                typeof(ICollection<>),
                typeof(List<>),
                typeof(object[])
            ]
        ),
        new SqlTypeToDotnetTypeMap(
            "clob",
            typeof(string),
            [
                typeof(byte),
                typeof(short),
                typeof(int),
                typeof(long),
                typeof(bool),
                typeof(float),
                typeof(double),
                typeof(decimal),
                typeof(DateTime),
                typeof(DateTimeOffset),
                typeof(TimeSpan),
                typeof(string),
                typeof(Guid),
                typeof(Dictionary<,>),
                typeof(IEnumerable<>),
                typeof(ICollection<>),
                typeof(List<>),
                typeof(object[])
            ]
        ),
        new SqlTypeToDotnetTypeMap("blob", typeof(byte[]), [typeof(byte[]), typeof(object)]),
    ];

    private static readonly DotnetTypeToSqlTypeMap[] DefaultDotnetToSqlTypeMap =
    [
        new DotnetTypeToSqlTypeMap(
            typeof(byte),
            "integer",
            [
                "integer",
                "int",
                "real",
                "float",
                "double",
                "numeric",
                "decimal",
                "text",
                "char",
                "nchar",
                "varchar",
                "nvarchar",
                "varying character",
                "native character",
                "clob"
            ]
        ),
        new DotnetTypeToSqlTypeMap(
            typeof(short),
            "integer",
            [
                "integer",
                "int",
                "real",
                "float",
                "double",
                "numeric",
                "decimal",
                "text",
                "char",
                "nchar",
                "varchar",
                "nvarchar",
                "varying character",
                "native character",
                "clob"
            ]
        ),
        new DotnetTypeToSqlTypeMap(
            typeof(int),
            "integer",
            [
                "integer",
                "real",
                "float",
                "double",
                "numeric",
                "decimal",
                "text",
                "char",
                "nchar",
                "varchar",
                "nvarchar",
                "varying character",
                "native character",
                "clob"
            ]
        ),
        new DotnetTypeToSqlTypeMap(
            typeof(long),
            "integer",
            [
                "integer",
                "int",
                "real",
                "float",
                "double",
                "numeric",
                "decimal",
                "text",
                "char",
                "nchar",
                "varchar",
                "nvarchar",
                "varying character",
                "native character",
                "clob"
            ]
        ),
        new DotnetTypeToSqlTypeMap(
            typeof(bool),
            "integer",
            [
                "integer",
                "int",
                "real",
                "float",
                "double",
                "numeric",
                "decimal",
                "text",
                "char",
                "nchar",
                "varchar",
                "nvarchar",
                "varying character",
                "native character",
                "clob"
            ]
        ),
        new DotnetTypeToSqlTypeMap(
            typeof(float),
            "real",
            [
                "real",
                "float",
                "double",
                "numeric",
                "decimal",
                "text",
                "char",
                "nchar",
                "varchar",
                "nvarchar",
                "varying character",
                "native character",
                "clob"
            ]
        ),
        new DotnetTypeToSqlTypeMap(
            typeof(double),
            "real",
            [
                "real",
                "float",
                "double",
                "numeric",
                "decimal",
                "text",
                "char",
                "nchar",
                "varchar",
                "nvarchar",
                "varying character",
                "native character",
                "clob"
            ]
        ),
        new DotnetTypeToSqlTypeMap(
            typeof(decimal),
            "real",
            [
                "real",
                "text",
                "char",
                "nchar",
                "varchar",
                "nvarchar",
                "varying character",
                "native character",
                "clob"
            ]
        ),
        new DotnetTypeToSqlTypeMap(
            typeof(DateTime),
            "text",
            [
                "text",
                "integer",
                "int",
                "real",
                "timestamp",
                "char",
                "nchar",
                "varchar",
                "nvarchar",
                "varying character",
                "native character",
                "clob"
            ]
        ),
        new DotnetTypeToSqlTypeMap(
            typeof(DateTimeOffset),
            "text",
            [
                "text",
                "integer",
                "int",
                "real",
                "datetime",
                "time",
                "date",
                "year",
                "char",
                "nchar",
                "varchar",
                "nvarchar",
                "varying character",
                "native character",
                "clob"
            ]
        ),
        new DotnetTypeToSqlTypeMap(
            typeof(TimeSpan),
            "text",
            [
                "text",
                "integer",
                "int",
                "real",
                "datetime",
                "timestamp",
                "time",
                "date",
                "year",
                "char",
                "nchar",
                "varchar",
                "nvarchar",
                "varying character",
                "native character",
                "clob"
            ]
        ),
        new DotnetTypeToSqlTypeMap(typeof(byte[]), "blob", ["blob",]),
        new DotnetTypeToSqlTypeMap(typeof(object), "text", ["text", "blob"]),
        new DotnetTypeToSqlTypeMap(typeof(string), "text", ["text",]),
        new DotnetTypeToSqlTypeMap(
            typeof(Guid),
            "text",
            [
                "text",
                "char",
                "nchar",
                "varchar",
                "nvarchar",
                "varying character",
                "native character",
                "clob"
            ]
        ),
        new DotnetTypeToSqlTypeMap(typeof(IDictionary<,>), "text", ["text",]),
        new DotnetTypeToSqlTypeMap(
            typeof(Dictionary<,>),
            "text",
            ["text", "varchar", "nvarchar", "varying character", "native character", "clob"]
        ),
        new DotnetTypeToSqlTypeMap(
            typeof(IEnumerable<>),
            "text",
            ["text", "varchar", "nvarchar", "varying character", "native character", "clob"]
        ),
        new DotnetTypeToSqlTypeMap(
            typeof(ICollection<>),
            "text",
            ["text", "varchar", "nvarchar", "varying character", "native character", "clob"]
        ),
        new DotnetTypeToSqlTypeMap(
            typeof(List<>),
            "text",
            ["text", "varchar", "nvarchar", "varying character", "native character", "clob"]
        ),
        new DotnetTypeToSqlTypeMap(
            typeof(object[]),
            "text",
            ["text", "varchar", "nvarchar", "varying character", "native character", "clob"]
        ),
    ];

    #endregion // Default Provider SQL Types

    internal SqliteProviderTypeMap()
        : base(DefaultProviderSqlTypes, DefaultDotnetToSqlTypeMap, DefaultSqlTypeToDotnetTypeMap)
    { }
}
