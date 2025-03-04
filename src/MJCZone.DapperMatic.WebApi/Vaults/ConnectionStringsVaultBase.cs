using System.Collections.Concurrent;
using MJCZone.DapperMatic.WebApi.Options;

namespace MJCZone.DapperMatic.WebApi.Vaults;

/// <summary>
/// Provides a base class for connection string vaults.
/// </summary>
public abstract class ConnectionStringsVaultBase : IConnectionStringsVault
{
    private static readonly ConcurrentDictionary<string, string> Cache = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionStringsVaultBase"/> class.
    /// </summary>
    /// <param name="name">The name of the vault.</param>
    /// <param name="vaultOptions">The options for the vault.</param>
    protected ConnectionStringsVaultBase(string name, ConnectionStringsVaultOptions vaultOptions)
    {
        this.Name = name;
        this.VaultOptions = vaultOptions;
    }

    /// <summary>
    /// Gets the name of the vault.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets a value indicating whether the vault is read-only.
    /// </summary>
    public bool IsReadOnly => this.VaultOptions.IsReadOnly == true;

    /// <summary>
    /// Gets the options for the vault.
    /// </summary>
    protected ConnectionStringsVaultOptions VaultOptions { get; }

    /// <summary>
    /// Gets the roles that have access to the vault.
    /// </summary>
    /// <returns>The roles that have access to the vault.</returns>
    public string[] GetRoles()
    {
        return this.VaultOptions.Roles?.Split(
                [',', ';', ' '],
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
            ) ?? [];
    }

    /// <summary>
    /// Gets the connection string for the specified name.
    /// </summary>
    /// <param name="connectionStringName">The name of the connection string to retrieve.</param>
    /// <param name="tenantIdentifier">The tenant identifier for the connection string.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The connection string if found; otherwise, null.</returns>
    /// <exception cref="ArgumentException">Thrown when the connection string name is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the connection string decryption fails.</exception>
    public virtual async Task<string?> GetConnectionStringAsync(
        string connectionStringName,
        string? tenantIdentifier = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(connectionStringName))
        {
            throw new ArgumentException("Connection string name cannot be null or empty.");
        }

        var cacheKey = GetConnectionStringCacheKey(connectionStringName, tenantIdentifier);

        if (Cache.TryGetValue(cacheKey, out var cachedConnectionString))
        {
            return cachedConnectionString;
        }

        var encryptedConnectionString = await GetEncryptedConnectionStringAsync(
                connectionStringName,
                tenantIdentifier,
                cancellationToken
            )
            .ConfigureAwait(false);

        var decryptedConnectionString = Decrypt(
            encryptedConnectionString,
            this.VaultOptions.EncryptionKey
        );

#pragma warning disable IDE0046 // Convert to conditional expression
        if (
            string.IsNullOrWhiteSpace(decryptedConnectionString)
            && !string.IsNullOrWhiteSpace(encryptedConnectionString)
        )
        {
            throw new InvalidOperationException("Connection string decryption failed.");
        }
#pragma warning restore IDE0046 // Convert to conditional expression

        if (!string.IsNullOrWhiteSpace(decryptedConnectionString))
        {
            Cache.AddOrUpdate(
                cacheKey,
                decryptedConnectionString!,
                (_, _) => decryptedConnectionString!
            );
        }

        return string.IsNullOrWhiteSpace(decryptedConnectionString)
            ? null
            : decryptedConnectionString;
    }

    /// <summary>
    /// Sets the connection string for the specified name.
    /// </summary>
    /// <param name="connectionStringName">The name of the connection string to set.</param>
    /// <param name="connectionString">The connection string value to set.</param>
    /// <param name="tenantIdentifier">The tenant identifier for the connection string.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
    /// <exception cref="ArgumentException">Thrown when the connection string name or value is null or empty.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the vault is read-only.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the connection string encryption fails.</exception>
    public virtual async Task SetConnectionStringAsync(
        string connectionStringName,
        string connectionString,
        string? tenantIdentifier = null,
        CancellationToken cancellationToken = default
    )
    {
        if (this.VaultOptions.IsReadOnly == true)
        {
            throw new InvalidOperationException("Connection string vault is read-only.");
        }

        if (string.IsNullOrWhiteSpace(connectionStringName))
        {
            throw new ArgumentException("Connection string name cannot be null or empty.");
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new ArgumentException("Connection string cannot be null or empty.");
        }

        var encryptedConnectionString = Encrypt(connectionString, this.VaultOptions.EncryptionKey);

        if (string.IsNullOrWhiteSpace(encryptedConnectionString))
        {
            throw new InvalidOperationException("Connection string encryption failed.");
        }

        await SetEncryptedConnectionStringAsync(
                connectionStringName,
                encryptedConnectionString,
                tenantIdentifier,
                cancellationToken
            )
            .ConfigureAwait(false);

        Cache.TryRemove(GetConnectionStringCacheKey(connectionStringName, tenantIdentifier), out _);
    }

    /// <summary>
    /// Deletes the connection string for the specified name.
    /// </summary>
    /// <param name="connectionStringName">The name of the connection string to delete.</param>
    /// <param name="tenantIdentifier">The tenant identifier for the connection string.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
    public virtual async Task DeleteConnectionStringAsync(
        string connectionStringName,
        string? tenantIdentifier = null,
        CancellationToken cancellationToken = default
    )
    {
        if (this.VaultOptions.IsReadOnly == true)
        {
            throw new InvalidOperationException("Connection string vault is read-only.");
        }

        if (string.IsNullOrWhiteSpace(connectionStringName))
        {
            throw new ArgumentException("Connection string name cannot be null or empty.");
        }

        await DeleteEncryptedConnectionStringAsync(
                connectionStringName,
                tenantIdentifier,
                cancellationToken
            )
            .ConfigureAwait(false);

        Cache.TryRemove(GetConnectionStringCacheKey(connectionStringName, tenantIdentifier), out _);
    }

    /// <summary>
    /// Clears the cache of connection strings.
    /// </summary>
    /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
    public virtual Task ClearCacheAsync()
    {
        Cache.Clear();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Encrypts a value using the encryption key.
    /// </summary>
    /// <param name="value">The value to encrypt.</param>
    /// <param name="encryptionKey">The encryption key to use for encrypting the value.</param>
    /// <returns>The encrypted value.</returns>
    protected virtual string? Encrypt(string? value, string? encryptionKey)
    {
        return string.IsNullOrWhiteSpace(value)
            ? value
            : string.IsNullOrWhiteSpace(encryptionKey)
                ? value
                : Crypto.Encrypt(value, encryptionKey);
    }

    /// <summary>
    /// Decrypts a value using the encryption key.
    /// </summary>
    /// <param name="encryptedValue">The value to decrypt.</param>
    /// <param name="encryptionKey">The encryption key to use for decrypting the value.</param>
    /// <returns>The decrypted value.</returns>
    protected virtual string? Decrypt(string? encryptedValue, string? encryptionKey)
    {
        return string.IsNullOrWhiteSpace(encryptedValue)
            ? encryptedValue
            : string.IsNullOrWhiteSpace(encryptionKey)
                ? encryptedValue
                : Crypto.Decrypt(encryptedValue, encryptionKey);
    }

    /// <summary>
    /// Gets the encrypted connection string for the specified name.
    /// </summary>
    /// <param name="connectionStringName">The name of the connection string to retrieve.</param>
    /// <param name="tenantIdentifier">The tenant identifier for the connection string.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The encrypted connection string if found; otherwise, null.</returns>
    protected abstract Task<string?> GetEncryptedConnectionStringAsync(
        string connectionStringName,
        string? tenantIdentifier = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Sets the encrypted connection string for the specified name.
    /// </summary>
    /// <param name="connectionStringName">The name of the connection string to set.</param>
    /// <param name="encryptedConnectionString">The encrypted connection string value to set.</param>
    /// <param name="tenantIdentifier">The tenant identifier for the connection string.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
    protected abstract Task SetEncryptedConnectionStringAsync(
        string connectionStringName,
        string encryptedConnectionString,
        string? tenantIdentifier = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Deletes the encrypted connection string for the specified name.
    /// </summary>
    /// <param name="connectionStringName">The name of the connection string to delete.</param>
    /// <param name="tenantIdentifier">The tenant identifier for the connection string.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
    protected abstract Task DeleteEncryptedConnectionStringAsync(
        string connectionStringName,
        string? tenantIdentifier = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Gets the cache key for the connection string.
    /// </summary>
    /// <param name="connectionStringName">The name of the connection string.</param>
    /// <param name="tenantIdentifier">The tenant identifier for the connection string.</param>
    /// <returns>The cache key for the connection string.</returns>
    protected string GetConnectionStringCacheKey(
        string connectionStringName,
        string? tenantIdentifier = null
    )
    {
        return string.IsNullOrWhiteSpace(tenantIdentifier)
            ? connectionStringName
            : $"{tenantIdentifier}:{connectionStringName}";
    }
}
