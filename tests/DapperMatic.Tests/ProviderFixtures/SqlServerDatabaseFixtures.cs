using Testcontainers.MsSql;

namespace DapperMatic.Tests.ProviderFixtures;

public class SqlServer_2022_CU13_Ubuntu_DatabaseFixture : SqlServerDatabaseFixture
{
    public SqlServer_2022_CU13_Ubuntu_DatabaseFixture()
        : base("mcr.microsoft.com/mssql/server:2022-CU13-ubuntu-22.04") { }
    // : base("mcr.microsoft.com/mssql/server:2022-latest") { }
}

public class SqlServer_2019_CU27_DatabaseFixture : SqlServerDatabaseFixture
{
    public SqlServer_2019_CU27_DatabaseFixture()
        : base("mcr.microsoft.com/mssql/server:2019-CU27-ubuntu-20.04") { }
    // : base("mcr.microsoft.com/mssql/server:2019-latest") { }
}

public class SqlServer_2017_CU29_DatabaseFixture : SqlServerDatabaseFixture
{
    public SqlServer_2017_CU29_DatabaseFixture()
        : base("mcr.microsoft.com/mssql/server:2017-CU29-ubuntu-16.04") { }
    // : base("mcr.microsoft.com/mssql/server:2017-latest") { }
}

public abstract class SqlServerDatabaseFixture(string imageName)
    : DatabaseFixtureBase<MsSqlContainer>
{
    private readonly MsSqlContainer _container = new MsSqlBuilder()
        .WithImage(imageName)
        .WithPassword("Strong_password_123!")
        .WithAutoRemove(true)
        .WithCleanUp(true)
        .Build();

    public override MsSqlContainer Container
    {
        get { return _container; }
    }
}
