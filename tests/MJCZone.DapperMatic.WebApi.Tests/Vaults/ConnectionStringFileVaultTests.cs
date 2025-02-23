using Microsoft.Extensions.Options;
using Moq;

namespace MJCZone.DapperMatic.WebApi.Tests.Vaults;

public class ConnectionStringFileVaultTests : IDisposable
{
    private readonly string _testFilePath = "testConnectionStrings.json";
    private readonly string _testEncryptionKey = "testEncryptionKey";
    private readonly ConnectionStringFileVault _vault;

    public ConnectionStringFileVaultTests()
    {
        var options = new DapperMaticOptions
        {
            ConnectionStringEncryptionKey = _testEncryptionKey,
            ConnectionStringsFilePath = _testFilePath
        };
        var monitor = Mock.Of<IOptionsMonitor<DapperMaticOptions>>(_ => _.CurrentValue == options);
        _vault = new ConnectionStringFileVault(monitor);
    }

    [Fact]
    public async Task GetConnectionString_ReturnsCorrectConnectionString()
    {
        // Arrange
        var connectionStringName = "TestConnection";
        var connectionStringValue = "Server=myServer;Database=myDB;User Id=myUser;Password=myPass;";
        var encryptedValue = Crypto.Encrypt(connectionStringValue, _testEncryptionKey);
        await File.WriteAllTextAsync(
            _testFilePath,
            $"{{\"{connectionStringName}\": \"{encryptedValue}\"}}"
        );

        // Act
        var result = await _vault.GetConnectionStringAsync(connectionStringName);

        // Assert
        Assert.Equal(connectionStringValue, result);
    }

    [Fact]
    public async Task SetConnectionString_SavesEncryptedConnectionString()
    {
        // Arrange
        var connectionStringName = "TestConnection";
        var connectionStringValue = "Server=myServer;Database=myDB;User Id=myUser;Password=myPass;";

        // Act
        await _vault.SetConnectionStringAsync(connectionStringName, connectionStringValue);

        // Assert
        var json = await File.ReadAllTextAsync(_testFilePath);
        var connectionStrings = System.Text.Json.JsonSerializer.Deserialize<
            Dictionary<string, string>
        >(json, DapperMaticOptions.JsonSerializerOptions);
        if (connectionStrings == null)
        {
            Assert.Fail("Connection strings dictionary is null.");
        }
        var encryptedValue = connectionStrings.TryGetValue(connectionStringName, out var value)
            ? value
            : null;
        if (encryptedValue == null)
        {
            Assert.Fail("Connection string not found in file.");
        }
        var decryptedValue = Crypto.Decrypt(encryptedValue, _testEncryptionKey);
        Assert.Equal(connectionStringValue, decryptedValue);
    }

    [Fact]
    public async Task GetConnectionString_ReturnsNullIfNotFound()
    {
        // Arrange
        var connectionStringName = "NonExistentConnection";

        // Act
        var result = await _vault.GetConnectionStringAsync(connectionStringName);

        // Assert
        Assert.Null(result);
    }

    public void Dispose()
    {
        if (File.Exists(_testFilePath))
        {
            // File.Delete(_testFilePath);
        }
    }
}
