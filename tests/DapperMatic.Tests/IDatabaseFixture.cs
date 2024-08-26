namespace DapperMatic.Tests;

public interface IDatabaseFixture
{
    string ConnectionString { get; }
    string ContainerId { get; }
}
