using DotNet.Testcontainers.Containers;

namespace DapperMatic.Tests.ProviderFixtures;

public abstract class DatabaseFixtureBase<TContainer> : IDatabaseFixture, IAsyncLifetime
    where TContainer : DockerContainer, IDatabaseContainer
{
    public abstract TContainer Container { get; }

    public virtual string ConnectionString => Container.GetConnectionString();
    public virtual string ContainerId => $"{Container.Id}";

    public virtual Task InitializeAsync() => Container.StartAsync();

    public virtual Task DisposeAsync() => Container.DisposeAsync().AsTask();
}
