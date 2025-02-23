using MJCZone.DapperMatic.WebApi;

namespace MJCZone.DapperMatic.WebApi.Tests.Vaults;

public class ConnectionStringFileVaultTests : IDisposable
{
    private readonly string _testFilePath = "testConnectionStrings.json";
    private readonly string _testEncryptionKey = "testEncryptionKey";
    private readonly ConnectionStringFileVault _vault;

    public ConnectionStringFileVaultTests()
    {
        _vault = new ConnectionStringFileVault(_testFilePath, _testEncryptionKey);
    }

    [Fact]
    public void GetConnectionString_ReturnsCorrectConnectionString()
    {
        // Arrange
        var connectionStringName = "TestConnection";
        var connectionStringValue = "Server=myServer;Database=myDB;User Id=myUser;Password=myPass;";
        var encryptedValue = Crypto.Encrypt(connectionStringValue, _testEncryptionKey);
        File.WriteAllText(_testFilePath, $"{{\"{connectionStringName}\": \"{encryptedValue}\"}}");

        // Act
        var result = _vault.GetConnectionString(connectionStringName);

        // Assert
        Assert.Equal(connectionStringValue, result);
    }

    [Fact]
    public void SetConnectionString_SavesEncryptedConnectionString()
    {
        // Arrange
        var connectionStringName = "TestConnection";
        var connectionStringValue = "Server=myServer;Database=myDB;User Id=myUser;Password=myPass;";

        // Act
        _vault.SetConnectionStringAsync(connectionStringName, connectionStringValue);

        // Assert
        var json = File.ReadAllText(_testFilePath);
        var connectionStrings = System.Text.Json.JsonSerializer.Deserialize<
            Dictionary<string, string>
        >(json);
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
    public void GetConnectionString_ReturnsNullIfNotFound()
    {
        // Arrange
        var connectionStringName = "NonExistentConnection";

        // Act
        var result = _vault.GetConnectionString(connectionStringName);

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
