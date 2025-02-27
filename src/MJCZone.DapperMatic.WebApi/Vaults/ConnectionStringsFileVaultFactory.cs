using MJCZone.DapperMatic.WebApi.Options;

namespace MJCZone.DapperMatic.WebApi.Vaults;

/// <summary>
/// Provides functionality to create instances of connection string vaults from a file.
/// </summary>
public class ConnectionStringsFileVaultFactory : IConnectionStringsVaultFactory
{
    /// <summary>
    /// The name of the connection string vault factory.
    /// </summary>
    public const string FactoryName = "FileVault";

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionStringsFileVaultFactory"/> class.
    /// </summary>
    public ConnectionStringsFileVaultFactory() { }

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
        return new ConnectionStringsFileVault(name, vaultOptions);
    }
}
