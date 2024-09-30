using System.Data;
using DapperMatic.Models;

namespace DapperMatic.Providers.PostgreSql;

public partial class PostgreSqlMethods : DatabaseMethodsBase, IDatabaseMethods
{
    private static string _defaultSchema = "public";

    public static void SetDefaultSchema(string schema)
    {
        _defaultSchema = schema;
    }

    protected override string DefaultSchema => _defaultSchema;

    protected override List<DataTypeMap> DataTypes =>
        DataTypeMapFactory.GetDefaultDbProviderDataTypeMap(DbProviderType.PostgreSql);

    internal PostgreSqlMethods() { }

    public override async Task<string> GetDatabaseVersionAsync(
        IDbConnection db,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await ExecuteScalarAsync<string>(db, $@"SELECT version()", transaction: tx)
                .ConfigureAwait(false) ?? "";
    }

    public override Type GetDotnetTypeFromSqlType(string sqlType)
    {
        return PostgreSqlSqlParser.GetDotnetTypeFromSqlType(sqlType);
    }

    protected override string GetSchemaQualifiedTableName(string schemaName, string tableName)
    {
        return string.IsNullOrWhiteSpace(schemaName) ? $"{tableName}" : $"{schemaName}.{tableName}";
    }
}
