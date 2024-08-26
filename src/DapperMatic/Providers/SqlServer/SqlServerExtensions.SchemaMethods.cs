using System.Data;
using System.Data.Common;

namespace DapperMatic.Providers.SqlServer;

public partial class SqlServerExtensions : DatabaseExtensionsBase, IDatabaseExtensions
{
    public async Task<bool> SchemaExistsAsync(
        IDbConnection db,
        string schema,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return 0
            < await ExecuteScalarAsync<int>(
                    db,
                    "SELECT count(*) FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME = @schema",
                    new { schema },
                    transaction: tx
                )
                .ConfigureAwait(false);
    }

    public async Task<bool> CreateSchemaIfNotExistsAsync(
        IDbConnection db,
        string schema,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (await SchemaExistsAsync(db, schema, tx, cancellationToken).ConfigureAwait(false))
            return false;

        var schemaName = NormalizeName(schema);

        await ExecuteAsync(db, $"CREATE SCHEMA {schemaName}", transaction: tx)
            .ConfigureAwait(false);
        return true;
    }

    public async Task<IEnumerable<string>> GetSchemasAsync(
        IDbConnection db,
        string? filter = null,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            // get sql server schemas
            return await QueryAsync<string>(
                    db,
                    "SELECT DISTINCT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA ORDER BY SCHEMA_NAME",
                    transaction: tx
                )
                .ConfigureAwait(false);
        }
        else
        {
            var where = $"{ToAlphaNumericString(filter)}".Replace("*", "%");
            return await QueryAsync<string>(
                    db,
                    "SELECT DISTINCT SCHEMA_NAME FROM INFORMATION_SCHEMA.SCHEMATA WHERE SCHEMA_NAME LIKE @where ORDER BY SCHEMA_NAME",
                    new { where },
                    tx
                )
                .ConfigureAwait(false);
        }
    }

    public async Task<bool> DropSchemaIfExistsAsync(
        IDbConnection db,
        string schema,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (!await SchemaExistsAsync(db, schema, tx, cancellationToken).ConfigureAwait(false))
            return false;

        var schemaName = NormalizeName(schema);

        var innerTx =
            tx
            ?? await (db as DbConnection)!
                .BeginTransactionAsync(cancellationToken)
                .ConfigureAwait(false);
        try
        {
            // drop all objects in the schema (except tables, which will be handled separately)
            var dropAllRelatedTypesSqlStatement = await QueryAsync<string>(
                    db,
                    $@"
                SELECT CASE
                    WHEN type in ('C', 'D', 'F', 'UQ', 'PK') THEN
                        CONCAT('ALTER TABLE ', QUOTENAME(SCHEMA_NAME(o.schema_id))+'.'+QUOTENAME(o.[name]), ' DROP CONSTRAINT ', QUOTENAME(o.[name]))
                    WHEN type in ('SN') THEN
                        CONCAT('DROP SYNONYM ', QUOTENAME(SCHEMA_NAME(o.schema_id))+'.'+QUOTENAME(o.[name]))
                    WHEN type in ('SO') THEN
                        CONCAT('DROP SEQUENCE ', QUOTENAME(SCHEMA_NAME(o.schema_id))+'.'+QUOTENAME(o.[name]))
                    WHEN type in ('U') THEN
                        CONCAT('DROP TABLE ', QUOTENAME(SCHEMA_NAME(o.schema_id))+'.'+QUOTENAME(o.[name]))
                    WHEN type in ('V') THEN
                        CONCAT('DROP VIEW ', QUOTENAME(SCHEMA_NAME(o.schema_id))+'.'+QUOTENAME(o.[name]))
                    WHEN type in ('TR') THEN
                        CONCAT('DROP TRIGGER ', QUOTENAME(SCHEMA_NAME(o.schema_id))+'.'+QUOTENAME(o.[name]))
                    WHEN type in ('IF', 'TF', 'FN', 'FS', 'FT') THEN
                        CONCAT('DROP FUNCTION ', QUOTENAME(SCHEMA_NAME(o.schema_id))+'.'+QUOTENAME(o.[name]))
                    WHEN type in ('P', 'PC') THEN
                        CONCAT('DROP PROCEDURE ', QUOTENAME(SCHEMA_NAME(o.schema_id))+'.'+QUOTENAME(o.[name]))
                    END AS DropSqlStatement
                FROM sys.objects o
                WHERE o.schema_id = SCHEMA_ID('{schemaName}')
                AND 
                    type IN(
                        --constraints (check, default, foreign key, unique)
                        'C', 'D', 'F', 'UQ',
                        --primary keys
                        'PK',
                        --synonyms
                        'SN',
                        --sequences
                        'SO',
                        --user defined tables
                        'U',
                        --views
                        'V',
                        --triggers
                        'TR',
                        --functions (inline, table-valued, scalar, CLR scalar, CLR table-valued)
                        'IF', 'TF', 'FN', 'FS', 'FT',
                        --procedures (stored procedure, CLR stored procedure)
                        'P', 'PC'
                    )
                ORDER BY CASE
                    WHEN type in ('C', 'D', 'UQ') THEN 2
                    WHEN type in ('F') THEN 1
                    WHEN type in ('PK') THEN 19
                    WHEN type in ('SN') THEN 3
                    WHEN type in ('SO') THEN 4
                    WHEN type in ('U') THEN 20
                    WHEN type in ('V') THEN 18
                    WHEN type in ('TR') THEN 10
                    WHEN type in ('IF', 'TF', 'FN', 'FS', 'FT') THEN 9
                    WHEN type in ('P', 'PC') THEN 8
                    END            
            ",
                    transaction: innerTx
                )
                .ConfigureAwait(false);
            foreach (var dropSql in dropAllRelatedTypesSqlStatement)
            {
                await ExecuteAsync(db, dropSql, transaction: innerTx).ConfigureAwait(false);
            }

            // drop xml schema collection
            var dropXmlSchemaCollectionSqlStatements = await QueryAsync<string>(
                    db,
                    $@"SELECT 'DROP XML SCHEMA COLLECTION ' + QUOTENAME(SCHEMA_NAME(schema_id)) + '.' + QUOTENAME(name)
                    FROM sys.xml_schema_collections
                    WHERE schema_id = SCHEMA_ID('{schemaName}')",
                    transaction: innerTx
                )
                .ConfigureAwait(false);
            foreach (var dropSql in dropXmlSchemaCollectionSqlStatements)
            {
                await ExecuteAsync(db, dropSql, transaction: innerTx).ConfigureAwait(false);
            }

            // drop all custom types
            var dropCustomTypesSqlStatements = await QueryAsync<string>(
                    db,
                    $@"SELECT 'DROP TYPE ' +QUOTENAME(SCHEMA_NAME(schema_id))+'.'+QUOTENAME(name)
                    FROM sys.types
                    WHERE schema_id = SCHEMA_ID('{schemaName}')",
                    transaction: innerTx
                )
                .ConfigureAwait(false);
            foreach (var dropSql in dropCustomTypesSqlStatements)
            {
                await ExecuteAsync(db, dropSql, transaction: innerTx).ConfigureAwait(false);
            }

            // drop the schema itself
            await ExecuteAsync(db, $"DROP SCHEMA [{schemaName}]", transaction: innerTx)
                .ConfigureAwait(false);

            if (tx == null)
                innerTx.Commit();
        }
        catch
        {
            if (tx == null)
                innerTx.Rollback();
            throw;
        }
        finally
        {
            if (tx == null)
                innerTx.Dispose();
        }

        return true;
    }
}
