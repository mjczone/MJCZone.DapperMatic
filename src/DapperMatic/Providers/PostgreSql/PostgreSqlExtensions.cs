using System.Data;

namespace DapperMatic.Providers.PostgreSql;

public partial class PostgreSqlExtensions : DatabaseExtensionsBase, IDatabaseExtensions
{
    internal PostgreSqlExtensions() { }

    protected override string DefaultSchema => "public";

    protected override List<DataTypeMap> DataTypes =>
        DataTypeMapFactory.GetDefaultDatabaseTypeDataTypeMap(DatabaseTypes.PostgreSql);

    protected override string NormalizeName(string name)
    {
        return base.NormalizeName(name).ToLowerInvariant();
    }

    public async Task<string> GetDatabaseVersionAsync(
        IDbConnection db,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await ExecuteScalarAsync<string>(db, $@"SELECT version()", transaction: tx)
                .ConfigureAwait(false) ?? "";
    }
}
