namespace DapperMatic.Providers.MySql;

public sealed class MySqlProviderTypeMap : ProviderTypeMapBase<MySqlProviderTypeMap>
{
    internal static readonly Lazy<MySqlProviderTypeMap> Instance =
        new(() => new MySqlProviderTypeMap());

    #region Default Provider SQL Types

    private static readonly ProviderSqlType[] DefaultProviderSqlTypes =
    [
        new ProviderSqlType(
            "tinyint",
            null,
            null,
            "tinyint({0})",
            null,
            null,
            true,
            false,
            null,
            4,
            null
        ),
        new ProviderSqlType(
            "smallint",
            null,
            null,
            "smallint({0})",
            null,
            null,
            true,
            false,
            null,
            5,
            null
        ),
        new ProviderSqlType(
            "integer",
            null,
            null,
            "integer({0})",
            null,
            null,
            true,
            false,
            null,
            11,
            null
        ),
        new ProviderSqlType(
            "int",
            "integer",
            null,
            "int({0})",
            null,
            null,
            true,
            false,
            null,
            11,
            null
        ),
        new ProviderSqlType(
            "mediumint",
            null,
            null,
            "mediumint({0})",
            null,
            null,
            true,
            false,
            null,
            7,
            null
        ),
        new ProviderSqlType(
            "bigint",
            null,
            null,
            "bigint({0})",
            null,
            null,
            true,
            false,
            null,
            19,
            null
        ),
        new ProviderSqlType(
            "serial",
            "bigint",
            null,
            null,
            null,
            null,
            false,
            true,
            null,
            null,
            null
        ),
        new ProviderSqlType(
            "decimal",
            null,
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
            "dec",
            "decimal",
            null,
            "dec({0})",
            "dec({0},{1})",
            null,
            false,
            false,
            null,
            12,
            2
        ),
        new ProviderSqlType(
            "fixed",
            "decimal",
            null,
            "fixed({0})",
            "fixed({0},{1})",
            null,
            false,
            false,
            null,
            12,
            2
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
            "float",
            "double precision",
            null,
            "float({0})",
            "float({0},{1})",
            null,
            false,
            false,
            null,
            12,
            2
        ),
        new ProviderSqlType(
            "real",
            "double precision",
            null,
            "real({0})",
            "real({0},{1})",
            null,
            false,
            false,
            null,
            12,
            2
        ),
        new ProviderSqlType(
            "double precision",
            null,
            null,
            "double precision({0})",
            "double precision({0},{1})",
            null,
            false,
            false,
            null,
            12,
            2
        ),
        new ProviderSqlType(
            "double",
            "double precision",
            null,
            "double({0})",
            "double({0},{1})",
            null,
            false,
            false,
            null,
            12,
            2
        ),
        new ProviderSqlType("bit", null, null, "bit({0})", null, null, false, false, null, 1, null),
        new ProviderSqlType(
            "bool",
            "tinyint(1)",
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
            "tinyint(1)",
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
            null,
            null,
            "datetime({0})",
            null,
            null,
            false,
            false,
            null,
            6,
            null
        ),
        new ProviderSqlType(
            "timestamp",
            null,
            null,
            "timestamp({0})",
            null,
            null,
            false,
            false,
            null,
            6,
            null
        ),
        new ProviderSqlType(
            "time",
            null,
            null,
            "time({0})",
            null,
            null,
            false,
            false,
            null,
            6,
            null
        ),
        new ProviderSqlType("date", null, null, null, null, null, false, false, null, null, null),
        new ProviderSqlType("year", null, null, null, null, null, false, false, null, null, null),
        new ProviderSqlType(
            "char",
            null,
            "char({0})",
            null,
            null,
            "char(255)",
            false,
            false,
            64,
            null,
            null
        ),
        new ProviderSqlType(
            "varchar",
            null,
            "varchar({0})",
            null,
            null,
            "varchar(8000)",
            false,
            false,
            255,
            null,
            null
        ),
        new ProviderSqlType(
            "tinytext",
            null,
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
            "mediumtext",
            null,
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
            "longtext",
            null,
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
        new ProviderSqlType("enum", null, null, null, null, null, false, false, null, null, null),
        new ProviderSqlType("set", null, null, null, null, null, false, false, null, null, null),
        new ProviderSqlType(
            "binary",
            null,
            "binary({0})",
            null,
            null,
            "binary(255)",
            false,
            false,
            64,
            null,
            null
        ),
        new ProviderSqlType(
            "varbinary",
            null,
            "varbinary({0})",
            null,
            null,
            "varbinary(8000)",
            false,
            false,
            4000,
            null,
            null
        ),
        new ProviderSqlType(
            "tinyblob",
            null,
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
        new ProviderSqlType("blob", null, null, null, null, null, false, false, null, null, null),
        new ProviderSqlType(
            "mediumblob",
            null,
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
            "longblob",
            null,
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
            "geometry",
            null,
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
        new ProviderSqlType("point", null, null, null, null, null, false, false, null, null, null),
        new ProviderSqlType(
            "linestring",
            null,
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
            "polygon",
            null,
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
            "multipoint",
            null,
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
            "multilinestring",
            null,
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
            "multipolygon",
            null,
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
            "geomcollection",
            null,
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
            "geometrycollection",
            "geomcollection",
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
        new ProviderSqlType("json", null, null, null, null, null, false, false, null, null, null),
    ];

    private static readonly SqlTypeToDotnetTypeMap[] DefaultSqlTypeToDotnetTypeMap =
    [
        new SqlTypeToDotnetTypeMap(
            "tinyint",
            typeof(byte),
            [
                typeof(byte),
                typeof(short),
                typeof(int),
                typeof(long),
                typeof(bool),
                typeof(float),
                typeof(double),
                typeof(decimal),
                typeof(string)
            ]
        ),
        new SqlTypeToDotnetTypeMap(
            "smallint",
            typeof(short),
            [
                typeof(byte),
                typeof(short),
                typeof(int),
                typeof(long),
                typeof(bool),
                typeof(float),
                typeof(double),
                typeof(decimal),
                typeof(string)
            ]
        ),
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
                typeof(string)
            ]
        ),
        new SqlTypeToDotnetTypeMap(
            "mediumint",
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
                typeof(string)
            ]
        ),
        new SqlTypeToDotnetTypeMap(
            "bigint",
            typeof(long),
            [
                typeof(byte),
                typeof(short),
                typeof(int),
                typeof(long),
                typeof(bool),
                typeof(float),
                typeof(double),
                typeof(decimal),
                typeof(string)
            ]
        ),
        new SqlTypeToDotnetTypeMap(
            "serial",
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
                typeof(string)
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
                typeof(decimal),
                typeof(string)
            ]
        ),
        new SqlTypeToDotnetTypeMap(
            "dec",
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
                typeof(string)
            ]
        ),
        new SqlTypeToDotnetTypeMap(
            "fixed",
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
                typeof(string)
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
                typeof(decimal),
                typeof(string)
            ]
        ),
        new SqlTypeToDotnetTypeMap(
            "float",
            typeof(float),
            [
                typeof(byte),
                typeof(short),
                typeof(int),
                typeof(long),
                typeof(bool),
                typeof(float),
                typeof(double),
                typeof(decimal),
                typeof(string)
            ]
        ),
        new SqlTypeToDotnetTypeMap(
            "real",
            typeof(float),
            [
                typeof(byte),
                typeof(short),
                typeof(int),
                typeof(long),
                typeof(bool),
                typeof(float),
                typeof(double),
                typeof(decimal),
                typeof(string)
            ]
        ),
        new SqlTypeToDotnetTypeMap(
            "double precision",
            typeof(double),
            [
                typeof(byte),
                typeof(short),
                typeof(int),
                typeof(long),
                typeof(bool),
                typeof(float),
                typeof(double),
                typeof(decimal),
                typeof(string)
            ]
        ),
        new SqlTypeToDotnetTypeMap(
            "double",
            typeof(double),
            [
                typeof(byte),
                typeof(short),
                typeof(int),
                typeof(long),
                typeof(bool),
                typeof(float),
                typeof(double),
                typeof(decimal),
                typeof(string)
            ]
        ),
        new SqlTypeToDotnetTypeMap(
            "bit",
            typeof(byte),
            [typeof(byte), typeof(short), typeof(int), typeof(long), typeof(bool), typeof(string)]
        ),
        new SqlTypeToDotnetTypeMap(
            "bool",
            typeof(bool),
            [typeof(byte), typeof(short), typeof(int), typeof(long), typeof(bool), typeof(string)]
        ),
        new SqlTypeToDotnetTypeMap(
            "boolean",
            typeof(bool),
            [typeof(byte), typeof(short), typeof(int), typeof(long), typeof(bool), typeof(string)]
        ),
        new SqlTypeToDotnetTypeMap(
            "datetime",
            typeof(DateTime),
            [typeof(DateTime), typeof(DateTimeOffset), typeof(string)]
        ),
        new SqlTypeToDotnetTypeMap(
            "timestamp",
            typeof(DateTimeOffset),
            [typeof(DateTime), typeof(DateTimeOffset), typeof(string)]
        ),
        new SqlTypeToDotnetTypeMap(
            "time",
            typeof(TimeSpan),
            [typeof(DateTime), typeof(DateTimeOffset), typeof(TimeSpan), typeof(string)]
        ),
        new SqlTypeToDotnetTypeMap(
            "date",
            typeof(DateTime),
            [typeof(DateTime), typeof(DateTimeOffset), typeof(string)]
        ),
        new SqlTypeToDotnetTypeMap(
            "year",
            typeof(DateTime),
            [
                typeof(short),
                typeof(int),
                typeof(long),
                typeof(DateTime),
                typeof(DateTimeOffset),
                typeof(TimeSpan),
                typeof(string)
            ]
        ),
        new SqlTypeToDotnetTypeMap("char", typeof(string), [typeof(string), typeof(Guid)]),
        new SqlTypeToDotnetTypeMap(
            "varchar",
            typeof(string),
            [
                typeof(DateTime),
                typeof(DateTimeOffset),
                typeof(TimeSpan),
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
        new SqlTypeToDotnetTypeMap("tinytext", typeof(string), [typeof(string)]),
        new SqlTypeToDotnetTypeMap(
            "text",
            typeof(string),
            [
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
            "mediumtext",
            typeof(string),
            [
                typeof(DateTime),
                typeof(DateTimeOffset),
                typeof(TimeSpan),
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
            "longtext",
            typeof(string),
            [
                typeof(DateTime),
                typeof(DateTimeOffset),
                typeof(TimeSpan),
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
        new SqlTypeToDotnetTypeMap("enum", typeof(string), [typeof(string)]),
        new SqlTypeToDotnetTypeMap("set", typeof(string), [typeof(string)]),
        new SqlTypeToDotnetTypeMap("binary", typeof(byte[]), [typeof(byte[])]),
        new SqlTypeToDotnetTypeMap("varbinary", typeof(byte[]), [typeof(byte[])]),
        new SqlTypeToDotnetTypeMap("tinyblob", typeof(byte[]), [typeof(byte[])]),
        new SqlTypeToDotnetTypeMap("blob", typeof(byte[]), [typeof(byte[])]),
        new SqlTypeToDotnetTypeMap("mediumblob", typeof(byte[]), [typeof(byte[])]),
        new SqlTypeToDotnetTypeMap("longblob", typeof(byte[]), [typeof(byte[])]),
        new SqlTypeToDotnetTypeMap("geometry", typeof(string), [typeof(object), typeof(string)]),
        new SqlTypeToDotnetTypeMap("point", typeof(string), [typeof(object), typeof(string)]),
        new SqlTypeToDotnetTypeMap("linestring", typeof(string), [typeof(object), typeof(string)]),
        new SqlTypeToDotnetTypeMap("polygon", typeof(string), [typeof(object), typeof(string)]),
        new SqlTypeToDotnetTypeMap("multipoint", typeof(string), [typeof(object), typeof(string)]),
        new SqlTypeToDotnetTypeMap(
            "multilinestring",
            typeof(string),
            [typeof(object), typeof(string)]
        ),
        new SqlTypeToDotnetTypeMap(
            "multipolygon",
            typeof(string),
            [typeof(object), typeof(string)]
        ),
        new SqlTypeToDotnetTypeMap(
            "geomcollection",
            typeof(string),
            [typeof(object), typeof(string)]
        ),
        new SqlTypeToDotnetTypeMap(
            "geometrycollection",
            typeof(string),
            [typeof(object), typeof(string)]
        ),
        new SqlTypeToDotnetTypeMap(
            "json",
            typeof(string),
            [
                typeof(string),
                typeof(IDictionary<,>),
                typeof(Dictionary<,>),
                typeof(IEnumerable<>),
                typeof(ICollection<>),
                typeof(List<>),
                typeof(object[])
            ]
        ),
    ];

    private static readonly DotnetTypeToSqlTypeMap[] DefaultDotnetToSqlTypeMap =
    [
        new DotnetTypeToSqlTypeMap(
            typeof(byte),
            "tinyint",
            [
                "tinyint",
                "smallint",
                "integer",
                "int",
                "mediumint",
                "bigint",
                "decimal",
                "dec",
                "fixed",
                "numeric",
                "float",
                "real",
                "double precision",
                "double",
                "bool",
                "boolean"
            ]
        ),
        new DotnetTypeToSqlTypeMap(
            typeof(short),
            "smallint",
            [
                "smallint",
                "integer",
                "int",
                "mediumint",
                "bigint",
                "decimal",
                "dec",
                "fixed",
                "numeric",
                "float",
                "real",
                "double precision",
                "double"
            ]
        ),
        new DotnetTypeToSqlTypeMap(
            typeof(int),
            "integer",
            [
                "integer",
                "bigint",
                "decimal",
                "dec",
                "fixed",
                "numeric",
                "float",
                "real",
                "double precision",
                "double"
            ]
        ),
        new DotnetTypeToSqlTypeMap(
            typeof(long),
            "bigint",
            [
                "bigint",
                "decimal",
                "dec",
                "fixed",
                "numeric",
                "float",
                "real",
                "double precision",
                "double"
            ]
        ),
        new DotnetTypeToSqlTypeMap(
            typeof(bool),
            "tinyint",
            [
                "tinyint",
                "smallint",
                "integer",
                "int",
                "mediumint",
                "bigint",
                "serial",
                "decimal",
                "dec",
                "fixed",
                "numeric",
                "float",
                "real",
                "double precision",
                "double",
                "bit"
            ]
        ),
        new DotnetTypeToSqlTypeMap(
            typeof(float),
            "float",
            ["float", "decimal", "dec", "fixed", "numeric", "double precision", "double"]
        ),
        new DotnetTypeToSqlTypeMap(
            typeof(double),
            "double precision",
            ["double precision", "decimal", "dec", "fixed", "numeric", "float", "real"]
        ),
        new DotnetTypeToSqlTypeMap(
            typeof(decimal),
            "decimal",
            ["decimal", "float", "real", "double precision", "double"]
        ),
        new DotnetTypeToSqlTypeMap(
            typeof(DateTime),
            "datetime",
            ["datetime", "timestamp", "varchar", "text", "mediumtext", "longtext"]
        ),
        new DotnetTypeToSqlTypeMap(
            typeof(DateTimeOffset),
            "timestamp",
            ["timestamp", "datetime", "varchar", "text", "mediumtext", "longtext"]
        ),
        new DotnetTypeToSqlTypeMap(
            typeof(TimeSpan),
            "time",
            ["time", "varchar", "text", "mediumtext", "longtext"]
        ),
        new DotnetTypeToSqlTypeMap(typeof(byte[]), "varbinary", ["varbinary",]),
        new DotnetTypeToSqlTypeMap(typeof(object), "text", ["text",]),
        new DotnetTypeToSqlTypeMap(typeof(string), "varchar", ["varchar",]),
        new DotnetTypeToSqlTypeMap(
            typeof(Guid),
            "varchar",
            ["varchar", "char", "text", "mediumtext", "longtext"]
        ),
        new DotnetTypeToSqlTypeMap(
            typeof(IDictionary<,>),
            "text",
            ["text", "varchar", "mediumtext", "longtext", "json"]
        ),
        new DotnetTypeToSqlTypeMap(
            typeof(Dictionary<,>),
            "text",
            ["text", "varchar", "mediumtext", "longtext", "json"]
        ),
        new DotnetTypeToSqlTypeMap(
            typeof(IEnumerable<>),
            "text",
            ["text", "varchar", "mediumtext", "longtext", "json"]
        ),
        new DotnetTypeToSqlTypeMap(
            typeof(ICollection<>),
            "text",
            ["text", "varchar", "mediumtext", "longtext", "json"]
        ),
        new DotnetTypeToSqlTypeMap(
            typeof(List<>),
            "text",
            ["text", "varchar", "mediumtext", "longtext", "json"]
        ),
        new DotnetTypeToSqlTypeMap(
            typeof(object[]),
            "text",
            ["text", "varchar", "mediumtext", "longtext", "json"]
        ),
    ];

    #endregion // Default Provider SQL Types

    internal MySqlProviderTypeMap()
        : base(DefaultProviderSqlTypes, DefaultDotnetToSqlTypeMap, DefaultSqlTypeToDotnetTypeMap)
    { }
}
