// Purpose: Provides a type map for SQLite data types.
namespace DapperMatic.Providers.Sqlite;

public class SqliteProviderTypeMap : ProviderTypeMapBase<SqliteProviderTypeMap>
{
    public SqliteProviderTypeMap()
    {
        foreach (var providerDataType in GetDefaultProviderDataTypes())
        {
            RegisterProviderDataType(providerDataType);
        }
    }

    // see: https://www.sqlite.org/datatype3.html
    // covers the following SQLite data types:
    // - TEXT
    //   - CHARACTER
    //   - CHAR
    //   - VARCHAR
    //   - VARYING CHARACTER
    //   - NCHAR
    //   - NATIVE CHARACTER
    //   - NVARCHAR
    //   - CLOB
    // - INTEGER
    //   - INT
    //   - TINYINT
    //   - SMALLINT
    //   - MEDIUMINT
    //   - BIGINT
    //   - UNSIGNED BIG INT
    //   - INT2
    //   - INT8
    //   - BOOLEAN
    // - REAL
    //   - DOUBLE
    //   - DOUBLE PRECISION
    //   - FLOAT
    // - NUMERIC
    //   - DECIMAL
    //   - NUMERIC
    // - BLOB
    // - (DATE/TIME) category
    //   - DATETIME
    //   - DATE
    public override ProviderDataType[] GetDefaultProviderDataTypes()
    {
        Type[] allIntegerAffinityTypes =
        [
            typeof(bool),
            typeof(int),
            typeof(long),
            typeof(short),
            typeof(byte)
        ];
        Type[] allRealAffinityTypes =
        [
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(int),
            typeof(long),
            typeof(short),
        ];
        Type[] allBlobAffinityTypes = [typeof(byte[]), typeof(object)];
        Type[] allTextAffinityTypes =
        [
            .. CommonTypes,
            .. CommonDictionaryTypes,
            .. CommonEnumerableTypes,
            typeof(object),
        ];
        return
        [
            // TEXT AFFINITY TYPES
            new ProviderDataType("TEXT", typeof(string), allTextAffinityTypes),
            new ProviderDataType(
                "CHARACTER",
                typeof(string),
                allTextAffinityTypes,
                "CHARACTER({0})"
            ),
            new ProviderDataType("CHAR", typeof(string), allTextAffinityTypes, "CHAR({0})"),
            new ProviderDataType("VARCHAR", typeof(string), allTextAffinityTypes, "VARCHAR({0})"),
            new ProviderDataType(
                "VARYING CHARACTER",
                typeof(string),
                allTextAffinityTypes,
                "VARYING CHARACTER({0})"
            ),
            new ProviderDataType("NCHAR", typeof(string), allTextAffinityTypes, "NCHAR({0})"),
            new ProviderDataType(
                "NATIVE CHARACTER",
                typeof(string),
                allTextAffinityTypes,
                "NATIVE CHARACTER({0})"
            ),
            new ProviderDataType("NVARCHAR", typeof(string), allTextAffinityTypes, "NVARCHAR({0})"),
            new ProviderDataType("CLOB", typeof(string), allTextAffinityTypes),
            // INTEGER AFFINITY TYPES
            new ProviderDataType("INTEGER", typeof(int), allIntegerAffinityTypes),
            new ProviderDataType("INT", typeof(int), allIntegerAffinityTypes),
            new ProviderDataType("TINYINT", typeof(short), allIntegerAffinityTypes),
            new ProviderDataType("SMALLINT", typeof(short), allIntegerAffinityTypes),
            new ProviderDataType("MEDIUMINT", typeof(int), allIntegerAffinityTypes),
            new ProviderDataType("BIGINT", typeof(long), allIntegerAffinityTypes),
            new ProviderDataType("UNSIGNED BIG INT", typeof(int), allIntegerAffinityTypes),
            new ProviderDataType("INT2", typeof(int), allIntegerAffinityTypes),
            new ProviderDataType("INT8", typeof(int), allIntegerAffinityTypes),
            new ProviderDataType("BOOLEAN", typeof(bool), allIntegerAffinityTypes),
            // REAL AFFINITY TYPES
            new ProviderDataType("REAL", typeof(double), allRealAffinityTypes),
            new ProviderDataType("DOUBLE", typeof(double), allRealAffinityTypes),
            new ProviderDataType("DOUBLE PRECISION", typeof(double), allRealAffinityTypes),
            new ProviderDataType("FLOAT", typeof(double), allRealAffinityTypes),
            new ProviderDataType(
                "DECIMAL",
                typeof(decimal),
                allRealAffinityTypes,
                null,
                "DECIMAL({0})",
                "DECIMAL({0}, {1})"
            ),
            new ProviderDataType(
                "NUMERIC",
                typeof(decimal),
                allRealAffinityTypes,
                null,
                "NUMERIC({0})",
                "NUMERIC({0}, {1})"
            ),
            new ProviderDataType(
                "DATETIME",
                typeof(DateTime),
                [typeof(DateTime), typeof(int), typeof(long)]
            ),
            new ProviderDataType(
                "DATE",
                typeof(DateTime),
                [typeof(DateTime), typeof(int), typeof(long)]
            ),
            // BINARY TYPES
            new ProviderDataType("BLOB", typeof(byte[]), [typeof(byte[]), typeof(object)]),
        ];
    }
}
