using System.Data;

namespace DapperMatic.Providers.MySql;

public partial class MySqlExtensions : DatabaseExtensionsBase, IDatabaseExtensions
{
    protected override string DefaultSchema => "";

    protected override List<DataTypeMap> DataTypes =>
        DataTypeMapFactory.GetDefaultDatabaseTypeDataTypeMap(DatabaseTypes.MySql);

    internal MySqlExtensions() { }

    public async Task<string> GetDatabaseVersionAsync(
        IDbConnection db,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return await ExecuteScalarAsync<string>(db, $@"SELECT VERSION()", transaction: tx)
                .ConfigureAwait(false) ?? "";
    }
}
