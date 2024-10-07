using System.Data;

namespace DapperMatic;

public partial interface IDatabaseMethods
    : IDatabaseTableMethods,
        IDatabaseColumnMethods,
        IDatabaseIndexMethods,
        IDatabaseCheckConstraintMethods,
        IDatabaseDefaultConstraintMethods,
        IDatabasePrimaryKeyConstraintMethods,
        IDatabaseUniqueConstraintMethods,
        IDatabaseForeignKeyConstraintMethods,
        IDatabaseSchemaMethods,
        IDatabaseViewMethods
{
    DbProviderType ProviderType { get; }

    bool SupportsSchemas { get; }

    Task<bool> SupportsCheckConstraintsAsync(
        IDbConnection db,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );
    Task<bool> SupportsOrderedKeysInConstraintsAsync(
        IDbConnection db,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );

    string GetLastSql(IDbConnection db);
    (string sql, object? parameters) GetLastSqlWithParams(IDbConnection db);
    Task<Version> GetDatabaseVersionAsync(
        IDbConnection db,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );
    Type GetDotnetTypeFromSqlType(string sqlType);
    string GetSqlTypeFromDotnetType(Type type, int? length, int? precision, int? scale);
    string NormalizeName(string name);
}
