using MJCZone.DapperMatic.Providers;

namespace MJCZone.DapperMatic.Tests;

public abstract partial class DatabaseMethodsTests
{
    private static Type[] GetSupportedTypes(IDbProviderTypeMap dbTypeMap)
    {
        Type[] typesToSupport =
        [
            typeof(bool),
            typeof(byte),
            typeof(sbyte),
            typeof(short),
            typeof(int),
            typeof(long),
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(char),
            typeof(string),
            typeof(char[]),
            typeof(ReadOnlyMemory<byte>[]),
            typeof(Stream),
            typeof(Guid),
            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(TimeSpan),
            typeof(DateOnly),
            typeof(TimeOnly),
            typeof(byte[]),
            typeof(object),
            // generic definitions
            typeof(IDictionary<,>),
            typeof(Dictionary<,>),
            typeof(IEnumerable<>),
            typeof(ICollection<>),
            typeof(List<>),
            typeof(object[]),
            // generics
            typeof(IDictionary<string, string>),
            typeof(Dictionary<string, string>),
            typeof(Dictionary<string, object>),
            typeof(Dictionary<string, object>),
            typeof(Dictionary<int, string>),
            typeof(Dictionary<Guid, object>),
            typeof(Dictionary<long, DateTime>),
            typeof(IEnumerable<string>),
            typeof(IEnumerable<Guid>),
            typeof(ICollection<string>),
            typeof(ICollection<Guid>),
            typeof(List<string>),
            typeof(List<decimal>),
            typeof(List<DateTimeOffset>),
            typeof(string[]),
            typeof(Guid[]),
            typeof(int[]),
            typeof(long[]),
            typeof(double[]),
            typeof(decimal[]),
            typeof(TimeSpan[]),
            // custom classes
            typeof(TestClassDao)
        ];

        return typesToSupport;
    }

    public class TestClassDao
    {
        public Guid Id { get; set; }
    }

    [Fact]
    protected virtual async Task Provider_type_map_supports_all_desired_dotnet_types()
    {
        using var db = await OpenConnectionAsync();
        var dbTypeMap = db.GetProviderTypeMap();
        foreach (var desiredType in GetSupportedTypes(dbTypeMap))
        {
            var exists = dbTypeMap.TryGetProviderSqlTypeMatchingDotnetType(
                new DotnetTypeDescriptor(desiredType.OrUnderlyingTypeIfNullable()),
                out var sqlType
            );

            Assert.True(exists, "Could not find a SQL type for " + desiredType.FullName);
            Assert.NotNull(sqlType);
        }
    }
}
