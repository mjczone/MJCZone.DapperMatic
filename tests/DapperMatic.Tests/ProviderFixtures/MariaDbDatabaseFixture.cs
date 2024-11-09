using Testcontainers.MariaDb;
using Testcontainers.MySql;

namespace DapperMatic.Tests.ProviderFixtures;

public class MariaDb_11_1_DatabaseFixture : MariaDbDatabaseFixture
{
    public MariaDb_11_1_DatabaseFixture()
        : base("mariadb:11.1") { }

    public override bool IgnoreSqlType(string sqlType)
    {
        return sqlType.Equals("geomcollection", StringComparison.OrdinalIgnoreCase)
            || base.IgnoreSqlType(sqlType);
    }
}

public class MariaDb_10_11_DatabaseFixture : MariaDbDatabaseFixture
{
    public MariaDb_10_11_DatabaseFixture()
        : base("mariadb:10.11") { }

    public override bool IgnoreSqlType(string sqlType)
    {
        return sqlType.Equals("geomcollection", StringComparison.OrdinalIgnoreCase)
            || base.IgnoreSqlType(sqlType);
    }
}

public abstract class MariaDbDatabaseFixture(string imageName)
    : DatabaseFixtureBase<MariaDbContainer>
{
    private readonly MariaDbContainer _container = new MariaDbBuilder()
        .WithImage(imageName)
        .WithPassword("Strong_password_123!")
        .WithAutoRemove(true)
        .WithCleanUp(true)
        .Build();

    public override MariaDbContainer Container
    {
        get { return _container; }
    }
}
