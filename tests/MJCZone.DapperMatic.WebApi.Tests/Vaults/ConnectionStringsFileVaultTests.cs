using Microsoft.Extensions.Options;
using Moq;

namespace MJCZone.DapperMatic.WebApi.Tests.Vaults;

public class ConnectionStringsFileVaultTests : IDisposable
{
    private readonly string _testFilePath = "testConnectionStrings.json";
    private readonly string _testEncryptionKey = "testEncryptionKey";
    private readonly ConnectionStringsFileVault _vault;

    public ConnectionStringsFileVaultTests()
    {
        var options = new DapperMaticOptions();
        options.ConnectionStringsVaults = new Dictionary<string, ConnectionStringsVaultOptions?>
        {
            {
                "TestVault",
                new ConnectionStringsVaultOptions
                {
                    FactoryName = "FileVault",
                    IsReadOnly = false,
                    EncryptionKey = _testEncryptionKey,
                    Settings = new Dictionary<string, object?> { { "FileName", _testFilePath }, },
                }
            }
        };
        options.DefaultConnectionStringsVaultName = "TestVault";

        var monitor = Mock.Of<IOptionsMonitor<DapperMaticOptions>>(_ => _.CurrentValue == options);
        var factory = new ConnectionStringsFileVaultFactory();
        _vault = (
            factory.Create(
                "TestVault",
                new ConnectionStringsVaultOptions
                {
                    FactoryName = "FileVault",
                    IsReadOnly = false,
                    EncryptionKey = _testEncryptionKey,
                    Settings = new Dictionary<string, object?> { { "FileName", _testFilePath }, },
                }
            ) as ConnectionStringsFileVault
        )!;
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
            $"{{\"connection_strings\": [{{ \"name\": \"{connectionStringName}\", \"value\": \"{encryptedValue}\", \"id\": 100 }}]}}"
        );

        // run it once
        await RunGetAsync(connectionStringName, connectionStringValue);

        // run it 100 times in parallel (to test concurrency)
        // this runs fast because values are cached by default
        var tasks = new List<Task>();
        for (var i = 0; i < 100; i++)
        {
            tasks.Add(RunGetAsync(connectionStringName, connectionStringValue));
        }
        await Task.WhenAll(tasks);
    }

    private async Task RunGetAsync(string connectionStringName, string connectionStringValue)
    {
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

        // run it once
        await RunSetAsync(connectionStringName, connectionStringValue);

        // run it 100 times in parallel
        var tasks = new List<Task>();
        for (var i = 0; i < 100; i++)
        {
            tasks.Add(RunSetAsync(connectionStringName, connectionStringValue));
        }
        await Task.WhenAll(tasks);
    }

    private async Task RunSetAsync(string connectionStringName, string connectionStringValue)
    {
        // Act
        await _vault.SetConnectionStringAsync(connectionStringName, connectionStringValue);

        // Assert
        var json = await File.ReadAllTextAsync(_testFilePath);
        var connectionStrings = System
            .Text.Json.JsonSerializer.Deserialize<cs_file>(
                json,
                DapperMaticOptions.JsonSerializerOptions
            )
            ?.connection_strings?.ToDictionary(c => c.name, c => c.value);

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
            File.Delete(_testFilePath);
        }
    }

    public class cs_file
    {
        public List<cs_record> connection_strings { get; set; } = [];
    }

    public class cs_record
    {
        public string name { get; set; } = string.Empty;
        public string value { get; set; } = string.Empty;
        public int id { get; set; }
    }
}
