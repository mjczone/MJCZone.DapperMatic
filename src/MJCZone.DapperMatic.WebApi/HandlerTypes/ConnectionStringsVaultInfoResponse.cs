using Dapper;

namespace MJCZone.DapperMatic.WebApi.HandlerTypes;

/// <summary>
/// Represents a response containing a list of connection strings vault info.
/// </summary>
public class ConnectionStringsVaultInfoResponse
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionStringsVaultInfoResponse"/> class.
    /// </summary>
    /// <remarks>
    /// This constructor is used by Dapper to map the results of a query to an instance of this class.
    /// </remarks>
    public ConnectionStringsVaultInfoResponse() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConnectionStringsVaultInfoResponse"/> class.
    /// </summary>
    /// <param name="data">The list of connection strings vault info.</param>
    public ConnectionStringsVaultInfoResponse(IEnumerable<ConnectionStringsVaultInfo> data)
    {
        Results = data.AsList();
    }

    /// <summary>
    /// Gets or sets the list of connection strings vault info.
    /// </summary>
    public List<ConnectionStringsVaultInfo>? Results { get; set; }
}

/// <summary>
/// Represents a connection strings vault info.
/// </summary>
public class ConnectionStringsVaultInfo
{
    /// <summary>
    /// Gets or sets the name of the connection strings vault.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the name of the factory.
    /// </summary>
    public string? FactoryName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the connection strings vault is read only.
    /// </summary>
    public bool? IsReadOnly { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the connection strings vault is the default.
    /// </summary>
    public bool? IsDefault { get; set; }
}
