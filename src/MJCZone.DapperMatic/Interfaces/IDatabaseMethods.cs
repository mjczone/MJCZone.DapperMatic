using System.Data;
using MJCZone.DapperMatic.Providers;

namespace MJCZone.DapperMatic.Interfaces;

/// <summary>
/// Defines methods for interacting with a database.
/// </summary>
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
    /// <summary>
    /// Gets the type of the database provider.
    /// </summary>
    DbProviderType ProviderType { get; }

    /// <summary>
    /// Gets the provider type map.
    /// </summary>
    IDbProviderTypeMap ProviderTypeMap { get; }

    /// <summary>
    /// Gets a value indicating whether the database supports schemas.
    /// </summary>
    bool SupportsSchemas { get; }

    /// <summary>
    /// Determines whether the database supports check constraints.
    /// </summary>
    /// <param name="db">The database connection.</param>
    /// <param name="tx">The database transaction.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating support for check constraints.</returns>
    Task<bool> SupportsCheckConstraintsAsync(
        IDbConnection db,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Determines whether the database supports ordered keys in constraints.
    /// </summary>
    /// <param name="db">The database connection.</param>
    /// <param name="tx">The database transaction.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating support for ordered keys in constraints.</returns>
    Task<bool> SupportsOrderedKeysInConstraintsAsync(
        IDbConnection db,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets the version of the database.
    /// </summary>
    /// <param name="db">The database connection.</param>
    /// <param name="tx">The database transaction.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the database version.</returns>
    Task<Version> GetDatabaseVersionAsync(
        IDbConnection db,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets the .NET type descriptor from the SQL type.
    /// </summary>
    /// <param name="sqlType">The SQL type.</param>
    /// <returns>The .NET type descriptor.</returns>
    DotnetTypeDescriptor GetDotnetTypeFromSqlType(string sqlType);

    /// <summary>
    /// Gets the SQL type from the .NET type descriptor.
    /// </summary>
    /// <param name="descriptor">The .NET type descriptor.</param>
    /// <returns>The SQL type.</returns>
    string GetSqlTypeFromDotnetType(DotnetTypeDescriptor descriptor);
}
