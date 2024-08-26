using Testcontainers.PostgreSql;

namespace DapperMatic.Tests.ProviderFixtures;

public class PostgreSql_Postgres15_DatabaseFixture : PostgreSqlDatabaseFixture
{
    public PostgreSql_Postgres15_DatabaseFixture()
        : base("postgres:16") { }
}

public class PostgreSql_Postgres16_DatabaseFixture : PostgreSqlDatabaseFixture
{
    public PostgreSql_Postgres16_DatabaseFixture()
        : base("postgres:16") { }
}

public class PostgreSql_Postgis15_DatabaseFixture : PostgreSqlDatabaseFixture
{
    public PostgreSql_Postgis15_DatabaseFixture()
        : base("postgis/postgis:15-3.4") { }
}

public class PostgreSql_Postgis16_DatabaseFixture : PostgreSqlDatabaseFixture
{
    public PostgreSql_Postgis16_DatabaseFixture()
        : base("postgis/postgis:16-3.4") { }
}

public abstract class PostgreSqlDatabaseFixture(string imageName)
    : DatabaseFixtureBase<PostgreSqlContainer>
{
    private readonly PostgreSqlContainer container = new PostgreSqlBuilder()
        .WithImage(imageName)
        .WithPassword("Strong_password_123!")
        .WithAutoRemove(true)
        .WithCleanUp(true)
        .Build();

    public override PostgreSqlContainer Container
    {
        get { return container; }
    }
}
