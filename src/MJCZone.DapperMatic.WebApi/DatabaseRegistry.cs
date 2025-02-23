using Dapper;
using MJCZone.DapperMatic.Models;
using MJCZone.DapperMatic.WebApi.Tables;

namespace MJCZone.DapperMatic.WebApi;

/// <summary>
/// Represents a registry for managing database entries.
/// </summary>
public class DatabaseRegistry : IDatabaseRegistry
{
    private readonly IDatabaseRegistryConnectionFactory _registryConnectionFactory;
    private static readonly char[] Separator = [',', ';'];

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseRegistry"/> class.
    /// </summary>
    /// <param name="connectionFactory">The connection factory for creating database connections.</param>
    public DatabaseRegistry(IDatabaseRegistryConnectionFactory connectionFactory)
    {
        _registryConnectionFactory = connectionFactory;
    }

    /// <summary>
    /// Initializes the database registry.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await _registryConnectionFactory
            .OpenConnectionAsync(cancellationToken)
            .ConfigureAwait(false);

        await connection
            .CreateTablesIfNotExistsAsync(
                [DmTableFactory.GetTable(typeof(web_databases))],
                cancellationToken: cancellationToken
            )
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Adds a new database entry asynchronously.
    /// </summary>
    /// <param name="database">The database entry to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an updated version of the database that was saved.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="database"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="database"/> has an invalid connection string or name.</exception>
    public async Task<DatabaseEntry> AddDatabaseAsync(
        DatabaseEntry database,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(database);

        // a database name is required
        if (string.IsNullOrWhiteSpace(database.Name))
        {
            throw new ArgumentException("Database name cannot be null or empty.");
        }

        if (string.IsNullOrWhiteSpace(database.ConnectionStringName))
        {
            throw new ArgumentException("Connection string name cannot be null or empty.");
        }

        if (database.ProviderType == null || database.ProviderType == DbProviderType.Other)
        {
            throw new ArgumentException(
                "Database provider type is either missing or not supported."
            );
        }

        var newDatabase = new DatabaseEntry
        {
            Id = database.Id == Guid.Empty ? Guid.NewGuid() : database.Id,
            TenantIdentifier = string.IsNullOrWhiteSpace(database.TenantIdentifier)
                ? null
                : database.TenantIdentifier.Trim(),
            Name = database.Name.Trim(),
            Description = string.IsNullOrWhiteSpace(database.Description)
                ? null
                : database.Description.Trim(),
            Slug = string.IsNullOrWhiteSpace(database.Slug) ? null : database.Slug,
            ProviderType = database.ProviderType!,
            ConnectionStringName = string.IsNullOrWhiteSpace(database.ConnectionStringName)
                ? $"CS_{Guid.NewGuid():N}"
                : database.ConnectionStringName.Trim(),
            ExecutionRoles = database.ExecutionRoles ?? [],
            ManagementRoles = database.ManagementRoles ?? [],
            IsActive = database.IsActive.GetValueOrDefault(true),
            CreatedDate = database.CreatedDate.GetValueOrDefault(DateTime.UtcNow),
            ModifiedDate = database.ModifiedDate.GetValueOrDefault(DateTime.UtcNow),
            CreatedBy = database.CreatedBy?.Trim(),
            ModifiedBy = database.ModifiedBy?.Trim(),
        };

        // save the database entry
        using var connection = await _registryConnectionFactory
            .OpenConnectionAsync(cancellationToken)
            .ConfigureAwait(false);

        // if the database id is already set, we need to make sure it doesn't already exist
        if (database.Id != Guid.Empty)
        {
            var count = await connection
                .ExecuteScalarAsync<int>(
                    $"SELECT count(id) FROM {nameof(web_databases)} WHERE id = @Id"
                        + (
                            string.IsNullOrWhiteSpace(database.TenantIdentifier)
                                ? " AND tenant_identifier IS NULL"
                                : " AND tenant_identifier = @TenantIdentifier"
                        ),
                    new { newDatabase.Id, newDatabase.TenantIdentifier }
                )
                .ConfigureAwait(false);

            if (count > 0)
            {
                throw new ArgumentException(
                    $"A database with the id {newDatabase.Id} already exists."
                );
            }
        }

        // the name must be unique
        var nameCount = await connection
            .ExecuteScalarAsync<int>(
                $"SELECT count(id) FROM {nameof(web_databases)} WHERE name = @Name"
                    + (
                        string.IsNullOrWhiteSpace(database.TenantIdentifier)
                            ? " AND tenant_identifier IS NULL"
                            : " AND tenant_identifier = @TenantIdentifier"
                    ),
                new { newDatabase.Name, newDatabase.TenantIdentifier }
            )
            .ConfigureAwait(false);

        if (nameCount > 0)
        {
            throw new ArgumentException(
                $"A database with the name {newDatabase.Name} already exists."
            );
        }

        // the slug must also be unique if set
        if (!string.IsNullOrWhiteSpace(newDatabase.Slug))
        {
            var count = await connection
                .ExecuteScalarAsync<int>(
                    $"SELECT count(id) FROM {nameof(web_databases)} WHERE slug = @Slug"
                        + (
                            string.IsNullOrWhiteSpace(database.TenantIdentifier)
                                ? " AND tenant_identifier IS NULL"
                                : " AND tenant_identifier = @TenantIdentifier"
                        ),
                    new { newDatabase.Slug, newDatabase.TenantIdentifier }
                )
                .ConfigureAwait(false);

            if (count > 0)
            {
                throw new ArgumentException(
                    $"A database with the slug {newDatabase.Slug} already exists."
                );
            }
        }

        // the DatabaseEntry maps to a web_databases table, so we need to
        // write universal sql to insert the database entry and map to
        // the web_databases table
        var sql =
            $@"
            INSERT INTO {nameof(web_databases)} (
                id,
                tenant_identifier,
                name,
                slug,
                description,
                provider_type,
                connection_string_name,
                execution_roles,
                management_roles,
                is_active,
                created_at,
                updated_at,
                created_by,
                updated_by
            ) VALUES (
                @Id,
                @TenantIdentifier,
                @Name,
                @Slug,
                @Description,
                @ProviderType,
                @ConnectionStringName,
                @ExecutionRoles,
                @ManagementRoles,
                @IsActive,
                @CreatedDate,
                @ModifiedDate,
                @CreatedBy,
                @ModifiedBy
            )";

        await connection
            .ExecuteAsync(
                sql,
                new
                {
                    newDatabase.Id,
                    newDatabase.TenantIdentifier,
                    newDatabase.Name,
                    newDatabase.Slug,
                    newDatabase.Description,
                    ProviderType = newDatabase.ProviderType!.ToString()!.ToLowerInvariant(),
                    newDatabase.ConnectionStringName,
                    ExecutionRoles = string.Join(';', newDatabase.ExecutionRoles ?? []),
                    ManagementRoles = string.Join(';', newDatabase.ManagementRoles ?? []),
                    newDatabase.IsActive,
                    newDatabase.CreatedDate,
                    newDatabase.ModifiedDate,
                    newDatabase.CreatedBy,
                    newDatabase.ModifiedBy,
                }
            )
            .ConfigureAwait(false);

        return newDatabase;
    }

    /// <summary>
    /// Deletes a database entry asynchronously.
    /// </summary>
    /// <param name="tenantIdentifier">The tenant identifier.</param>
    /// <param name="idOrSlug">The ID or slug of the database to delete.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><c>true</c> if the database was deleted successfully; otherwise, <c>false</c>.</returns>
    public async Task<bool> DeleteDatabaseAsync(
        string? tenantIdentifier,
        string idOrSlug,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(idOrSlug))
        {
            return false;
        }

        idOrSlug = idOrSlug.Trim();
        tenantIdentifier = string.IsNullOrWhiteSpace(tenantIdentifier)
            ? null
            : tenantIdentifier.Trim();

        using var connection = await _registryConnectionFactory
            .OpenConnectionAsync(cancellationToken)
            .ConfigureAwait(false);

        var sql = $"DELETE FROM {nameof(web_databases)} WHERE";
        if (Guid.TryParse(idOrSlug, out var id) && id != Guid.Empty)
        {
            sql += @" id = @Id";
        }
        else
        {
            sql += @" slug = @Slug";
        }
        if (!string.IsNullOrWhiteSpace(tenantIdentifier))
        {
            sql += @" AND tenant_identifier = @TenantIdentifier";
        }
        else
        {
            sql += @" AND tenant_identifier IS NULL";
        }

        var rowsAffected = await connection
            .ExecuteAsync(
                sql,
                new
                {
                    Id = id,
                    Slug = idOrSlug,
                    TenantIdentifier = tenantIdentifier,
                }
            )
            .ConfigureAwait(false);

        return rowsAffected > 0;
    }

    /// <summary>
    /// Retrieves a database entry asynchronously by its unique identifier.
    /// </summary>
    /// <param name="tenantIdentifier">The tenant identifier.</param>
    /// <param name="idOrSlug">The ID or slug of the database to retrieve.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The database with the specified ID, or <c>null</c> if not found.</returns>
    public async Task<DatabaseEntry?> GetDatabaseAsync(
        string? tenantIdentifier,
        string idOrSlug,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(idOrSlug))
        {
            return null;
        }

        idOrSlug = idOrSlug.Trim();
        tenantIdentifier = string.IsNullOrWhiteSpace(tenantIdentifier)
            ? null
            : tenantIdentifier.Trim();

        using var connection = await _registryConnectionFactory
            .OpenConnectionAsync(cancellationToken)
            .ConfigureAwait(false);

        var sql =
            $@"
            SELECT
                id,
                tenant_identifier,
                name,
                slug,
                description,
                provider_type,
                connection_string_name,
                execution_roles,
                management_roles,
                is_active,
                created_at,
                updated_at,
                created_by,
                updated_by
            FROM
                {nameof(web_databases)}
            WHERE";
        if (Guid.TryParse(idOrSlug, out var id) && id != Guid.Empty)
        {
            sql += @" id = @Id";
        }
        else
        {
            sql += @" slug = @Slug";
        }
        if (!string.IsNullOrWhiteSpace(tenantIdentifier))
        {
            sql += @" AND tenant_identifier = @TenantIdentifier";
        }
        else
        {
            sql += @" AND tenant_identifier IS NULL";
        }

        var database = await connection
            .QueryFirstOrDefaultAsync<web_databases>(
                sql,
                new
                {
                    Id = id,
                    Slug = idOrSlug,
                    TenantIdentifier = tenantIdentifier,
                }
            )
            .ConfigureAwait(false);

        if (database == null)
        {
            return null;
        }

        return new DatabaseEntry
        {
            Id = database.id,
            TenantIdentifier = database.tenant_identifier,
            Name = database.name,
            Slug = database.slug,
            Description = database.description,
            ProviderType = Enum.Parse<DbProviderType>(database.provider_type, true),
            ConnectionStringName = database.connection_string_name,
            ExecutionRoles =
                database
                    .execution_roles?.Split(
                        Separator,
                        StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
                    )
                    .ToList() ?? [],
            ManagementRoles =
                database
                    .management_roles?.Split(
                        Separator,
                        StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
                    )
                    .ToList() ?? [],
            IsActive = database.is_active,
            CreatedDate = database.created_at,
            ModifiedDate = database.updated_at,
            CreatedBy = database.created_by,
            ModifiedBy = database.updated_by,
        };
    }

    /// <summary>
    /// Retrieves all database entries asynchronously.
    /// </summary>
    /// <param name="tenantIdentifier">The tenant identifier.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable of all database entries.</returns>
    public async Task<IEnumerable<DatabaseEntry>> GetDatabasesAsync(
        string? tenantIdentifier,
        CancellationToken cancellationToken = default
    )
    {
        tenantIdentifier = string.IsNullOrWhiteSpace(tenantIdentifier)
            ? null
            : tenantIdentifier.Trim();

        using var connection = await _registryConnectionFactory
            .OpenConnectionAsync(cancellationToken)
            .ConfigureAwait(false);

        var sql =
            $@"
            SELECT
                id,
                tenant_identifier,
                name,
                slug,
                description,
                provider_type,
                connection_string_name,
                execution_roles,
                management_roles,
                is_active,
                created_at,
                updated_at,
                created_by,
                updated_by
            FROM
                {nameof(web_databases)}
            WHERE";
        if (!string.IsNullOrWhiteSpace(tenantIdentifier))
        {
            sql += @" tenant_identifier = @TenantIdentifier";
        }
        else
        {
            sql += @" tenant_identifier IS NULL";
        }
        sql += " ORDER BY name";

        var databases = await connection
            .QueryAsync<web_databases>(sql, new { TenantIdentifier = tenantIdentifier, })
            .ConfigureAwait(false);

        return databases.Select(database => new DatabaseEntry
        {
            Id = database.id,
            TenantIdentifier = database.tenant_identifier,
            Name = database.name,
            Slug = database.slug,
            Description = database.description,
            ProviderType = Enum.Parse<DbProviderType>(database.provider_type, true),
            ConnectionStringName = database.connection_string_name,
            ExecutionRoles =
                database
                    .execution_roles?.Split(
                        Separator,
                        StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
                    )
                    .ToList() ?? [],
            ManagementRoles =
                database
                    .management_roles?.Split(
                        Separator,
                        StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
                    )
                    .ToList() ?? [],
            IsActive = database.is_active,
            CreatedDate = database.created_at,
            ModifiedDate = database.updated_at,
            CreatedBy = database.created_by,
            ModifiedBy = database.updated_by,
        });
    }

    /// <summary>
    /// Updates an existing database entry asynchronously.
    /// </summary>
    /// <param name="database">The updated database information.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The updated database entry.</returns>
    public async Task<DatabaseEntry> PatchDatabaseAsync(
        DatabaseEntry database,
        CancellationToken cancellationToken = default
    )
    {
        if (database.Id == Guid.Empty && string.IsNullOrWhiteSpace(database.Slug))
        {
            throw new ArgumentException("Database ID or slug must be specified.");
        }

        var existingDatabase = GetDatabaseAsync(
                database.TenantIdentifier,
                database.Id != Guid.Empty ? database.Id.ToString() : database.Slug!,
                cancellationToken
            )
            .GetAwaiter()
            .GetResult();

        if (existingDatabase == null)
        {
            throw new ArgumentException("Database not found.");
        }

        if (database.Id != Guid.Empty)
        {
            // the slug is ok to update when the id was passed
            if (!string.IsNullOrWhiteSpace(database.Slug))
            {
                existingDatabase.Slug = database.Slug;
            }
        }
        else
        {
            // the id is ok to update when the slug was passed
            existingDatabase.Id = database.Id;
        }

        // we only update fields that were sent in the request
        // an empty string will wipe out the field, BUT null will ignore
        // that field... if the field is required, empty will also be ignored
        if (!string.IsNullOrWhiteSpace(database.Name))
        {
            existingDatabase.Name = database.Name;
        }
        if (database.Description != null)
        {
            existingDatabase.Description = string.IsNullOrWhiteSpace(database.Description)
                ? null
                : database.Description;
        }
        if (database.ProviderType != null && database.ProviderType != DbProviderType.Other)
        {
            // this would be unusual, unless the connection string was also changed
            // we'll allow it for now, but let's monitor the usage
            existingDatabase.ProviderType = database.ProviderType;
        }
        if (!string.IsNullOrWhiteSpace(database.ConnectionStringName))
        {
            existingDatabase.ConnectionStringName = database.ConnectionStringName;
        }
        if (database.ExecutionRoles != null)
        {
            existingDatabase.ExecutionRoles = database.ExecutionRoles;
        }
        if (database.ManagementRoles != null)
        {
            existingDatabase.ManagementRoles = database.ManagementRoles;
        }
        if (database.IsActive != null)
        {
            existingDatabase.IsActive = database.IsActive;
        }
        existingDatabase.ModifiedDate = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(database.ModifiedBy))
        {
            existingDatabase.ModifiedBy = database.ModifiedBy;
        }

        var sql =
            $@"
            UPDATE {nameof(web_databases)} SET
                name = @Name,
                slug = @Slug,
                description = @Description,
                provider_type = @ProviderType,
                connection_string_name = @ConnectionStringName,
                execution_roles = @ExecutionRoles,
                management_roles = @ManagementRoles,
                is_active = @IsActive,
                updated_at = @ModifiedDate,
                updated_by = @ModifiedBy
            WHERE id = @Id";

        using var connection = await _registryConnectionFactory
            .OpenConnectionAsync(cancellationToken)
            .ConfigureAwait(false);

        await connection
            .ExecuteAsync(
                sql,
                new
                {
                    existingDatabase.Id,
                    existingDatabase.Name,
                    existingDatabase.Slug,
                    existingDatabase.Description,
                    ProviderType = existingDatabase.ProviderType!.ToString()!.ToLowerInvariant(),
                    existingDatabase.ConnectionStringName,
                    ExecutionRoles = string.Join(';', existingDatabase.ExecutionRoles ?? []),
                    ManagementRoles = string.Join(';', existingDatabase.ManagementRoles ?? []),
                    existingDatabase.IsActive,
                    existingDatabase.ModifiedDate,
                    existingDatabase.ModifiedBy,
                }
            )
            .ConfigureAwait(false);

        return existingDatabase;
    }
}
