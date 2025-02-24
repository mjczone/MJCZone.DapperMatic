namespace MJCZone.DapperMatic.WebApi;

/// <summary>
/// Provides functionality to create instances of connection string vaults.
/// </summary>
public interface IConnectionStringsVaultFactory
{
    /// <summary>
    /// Gets the name of the connection string vault factory.
    /// </summary>
    /// <remarks>
    /// This is used to identify the factory when resolving it from a service provider. It should be unique.
    /// This is not the name of the vault itself, but the name of the factory that creates the vault.
    /// For example, a factory that creates a vault from a file might be named "File".
    /// A factory that creates a vault from a database might be named "Database".
    /// A factory that creates a vault from a configuration section might be named "Configuration".
    /// A factory that creates a vault from a key vault might be named "KeyVault".
    /// The instance of a vault created by this factory might be named "ConnectionStringsFile1Vault", "ConnectionStringsFile2Vault".
    /// The instances of a vault created by this factory might be named "ConnectionStringsDatabase1Vault", and "ConnectionStringsDatabase2Vault".
    /// The instance of a vault created by this factory might be named "ConnectionStringsConfigurationVault".
    /// </remarks>
    string Name { get; }

    /// <summary>
    /// Creates a new instance of the connection string vault.
    /// </summary>
    /// <param name="name">The name of the connection string vault.</param>
    /// <param name="vaultOptions">The options for the connection string vault.</param>
    /// <returns>A new instance of the connection string vault.</returns>
    IConnectionStringsVault Create(string name, ConnectionStringsVaultOptions vaultOptions);
}
