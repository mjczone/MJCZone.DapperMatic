using System.Data;

namespace DapperMatic.Providers.MySql;

public partial class MySqlMethods
{
    public override async Task<bool> DropUniqueConstraintIfExistsAsync(
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
                await DoesUniqueConstraintExistAsync(
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

        var schemaQualifiedTableName = GetSchemaQualifiedTableName(schemaName, tableName);

        // in mysql <= 5.7, you can't drop a unique constraint by name, you have to drop the index
        await ExecuteAsync(
                db,
                $@"ALTER TABLE {schemaQualifiedTableName} 
                    DROP INDEX {constraintName}",
                transaction: tx
            )
            .ConfigureAwait(false);

        return true;
    }
}
