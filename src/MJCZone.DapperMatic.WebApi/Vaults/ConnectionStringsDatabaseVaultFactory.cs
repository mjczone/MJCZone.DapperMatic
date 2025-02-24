namespace MJCZone.DapperMatic.WebApi;

/// <summary>
/// Provides functionality to create instances of connection string vaults from a database.
/// </summary>
public class ConnectionStringsDatabaseVaultFactory : IConnectionStringsVaultFactory
{
    /// <summary>
    /// The name of the connection string vault factory.
    /// </summary>
    public const string FactoryName = "DatabaseVault";

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionStringsDatabaseVaultFactory"/> class.
    /// </summary>
    public ConnectionStringsDatabaseVaultFactory() { }

    /// <summary>
    /// Gets the name of the connection string vault factory.
    /// </summary>
    public string Name => FactoryName;

    /// <summary>
    /// Creates a new instance of the connection string vault.
    /// </summary>
    /// <param name="name">The name of the connection string vault.</param>
    /// <param name="vaultOptions">The options for the connection string vault.</param>
    /// <returns>A new instance of the connection string vault.</returns>
    public IConnectionStringsVault Create(string name, ConnectionStringsVaultOptions vaultOptions)
    {
        return new ConnectionStringsDatabaseVault(name, vaultOptions);
    }
}
