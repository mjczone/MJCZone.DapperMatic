using System.Data;

namespace DapperMatic.Providers.Sqlite;

public partial class SqliteExtensions : DatabaseExtensionsBase, IDatabaseExtensions
{
    protected override string DefaultSchema => "";

    protected override List<DataTypeMap> DataTypes =>
        DataTypeMapFactory.GetDefaultDatabaseTypeDataTypeMap(DatabaseTypes.Sqlite);

    internal SqliteExtensions() { }

    public async Task<string> GetDatabaseVersionAsync(
        IDbConnection db,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await ExecuteScalarAsync<string>(db, $@"select sqlite_version()", transaction: tx)
                .ConfigureAwait(false) ?? "";
    }
}
