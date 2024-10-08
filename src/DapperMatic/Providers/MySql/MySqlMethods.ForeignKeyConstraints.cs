using System.Data;

namespace DapperMatic.Providers.MySql;

public partial class MySqlMethods
{
    public override async Task<bool> DropForeignKeyConstraintIfExistsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string constraintName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (
            !(
                await DoesForeignKeyConstraintExistAsync(
                        db,
                        schemaName,
                        tableName,
                        constraintName,
                        tx,
                        cancellationToken
                    )
                    .ConfigureAwait(false)
            )
        )
            return false;

        (schemaName, tableName, constraintName) = NormalizeNames(
            schemaName,
            tableName,
            constraintName
        );

        var schemaQualifiedTableName = GetSchemaQualifiedIdentifierName(schemaName, tableName);

        await ExecuteAsync(
                db,
                $@"ALTER TABLE {schemaQualifiedTableName} 
                    DROP FOREIGN KEY {constraintName}",
                tx: tx
            )
            .ConfigureAwait(false);

        return true;
    }
}
