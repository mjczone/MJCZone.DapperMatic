using System.Data;

namespace DapperMatic.Providers.SqlServer;

public partial class SqlServerExtensions : DatabaseExtensionsBase, IDatabaseExtensions
{
    protected override string DefaultSchema => "dbo";

    protected override List<DataTypeMap> DataTypes =>
        DataTypeMapFactory.GetDefaultDatabaseTypeDataTypeMap(DatabaseTypes.SqlServer);

    internal SqlServerExtensions() { }

    public async Task<string> GetDatabaseVersionAsync(
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
}
