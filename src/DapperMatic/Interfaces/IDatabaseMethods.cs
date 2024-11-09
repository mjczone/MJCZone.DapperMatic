using System.Data;
using DapperMatic.Providers;

namespace DapperMatic.Interfaces;

public interface IDatabaseMethods
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
    IDbProviderTypeMap ProviderTypeMap { get; }

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

    DbProviderDotnetTypeDescriptor GetDotnetTypeFromSqlType(string sqlType);
    string GetSqlTypeFromDotnetType(DbProviderDotnetTypeDescriptor descriptor);

    string NormalizeName(string name);
}
