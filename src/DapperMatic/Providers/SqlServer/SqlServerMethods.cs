using System.Data;

namespace DapperMatic.Providers.SqlServer;

public partial class SqlServerMethods : DatabaseMethodsBase, IDatabaseMethods
{
    public override DbProviderType ProviderType => DbProviderType.SqlServer;

    private static string _defaultSchema = "dbo";

    public static void SetDefaultSchema(string schema)
    {
        _defaultSchema = schema;
    }

    protected override string DefaultSchema => _defaultSchema;

    internal SqlServerMethods() { }

    public override async Task<string> GetDatabaseVersionAsync(
        IDbConnection db,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        /*
            SELECT
            SERVERPROPERTY('Productversion') As [SQL Server Version],
            SERVERPROPERTY('Productlevel') As [SQL Server Build Level],
            SERVERPROPERTY('edition') As [SQL Server Edition]
         */
        return await ExecuteScalarAsync<string>(
                    db,
                    $@"SELECT SERVERPROPERTY('Productversion')",
                    transaction: tx
                )
                .ConfigureAwait(false) ?? "";
    }

    public override Type GetDotnetTypeFromSqlType(string sqlType)
    {
        return SqlServerSqlParser.GetDotnetTypeFromSqlType(sqlType);
    }

    protected override string GetSchemaQualifiedTableName(string schemaName, string tableName)
    {
        return string.IsNullOrWhiteSpace(schemaName)
            ? $"[{tableName}]"
            : $"[{schemaName}].[{tableName}]";
    }
}
