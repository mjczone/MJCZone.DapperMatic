using Testcontainers.MySql;

namespace MJCZone.DapperMatic.Tests.ProviderFixtures;

public class MySql_90_DatabaseFixture : MySqlDatabaseFixture
{
    public MySql_90_DatabaseFixture()
        : base("mysql:9.0") { }
}

public class MySql_84_DatabaseFixture : MySqlDatabaseFixture
{
    public MySql_84_DatabaseFixture()
        : base("mysql:8.4") { }
}

public class MySql_57_DatabaseFixture : MySqlDatabaseFixture
{
    public MySql_57_DatabaseFixture()
        : base("mysql:5.7") { }

    public override bool IgnoreSqlType(string sqlType)
    {
        return sqlType.Equals("geomcollection", StringComparison.OrdinalIgnoreCase)
            || base.IgnoreSqlType(sqlType);
    }
}

public abstract class MySqlDatabaseFixture(string imageName) : DatabaseFixtureBase<MySqlContainer>
{
    private readonly MySqlContainer _container = new MySqlBuilder()
        .WithImage(imageName)
        .WithPassword("Strong_password_123!")
        .WithAutoRemove(true)
        .WithCleanUp(true)
        .Build();

    public override MySqlContainer Container
    {
        get { return _container; }
    }
}
