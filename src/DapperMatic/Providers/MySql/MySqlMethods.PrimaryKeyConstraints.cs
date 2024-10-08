using System.Data;

namespace DapperMatic.Providers.MySql;

public partial class MySqlMethods
{
    public override async Task<bool> DropPrimaryKeyConstraintIfExistsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        var primaryKeyConstraint = await GetPrimaryKeyConstraintAsync(
            db,
            schemaName,
            tableName,
            tx,
            cancellationToken
        );
        if (primaryKeyConstraint is null)
            return false;

        (schemaName, tableName, _) = NormalizeNames(schemaName, tableName);

        var schemaQualifiedTableName = GetSchemaQualifiedIdentifierName(schemaName, tableName);

        await ExecuteAsync(
                db,
                $@"ALTER TABLE {schemaQualifiedTableName} 
                    DROP PRIMARY KEY",
                tx: tx
            )
            .ConfigureAwait(false);

        return true;
    }
}
