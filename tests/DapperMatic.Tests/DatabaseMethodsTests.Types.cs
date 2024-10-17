using DapperMatic.Models;
using DapperMatic.Providers;

namespace DapperMatic.Tests;

public abstract partial class DatabaseMethodsTests
{
    private static Type[] GetSupportedTypes(IProviderTypeMap dbTypeMap)
    {
        Type[] supportedTypes = dbTypeMap
            .GetProviderSqlTypes()
            .SelectMany(t =>
            {
                var dotnetTypes = new List<Type>();
                if (
                    dbTypeMap.TryGetRecommendedDotnetTypeMatchingSqlType(
                        t.SqlType,
                        out var dotnetTypeInfo
                    )
                    && dotnetTypeInfo != null
                )
                {
                    dotnetTypes.AddRange(dotnetTypeInfo.Value.otherSupportedTypes);
                }
                return dotnetTypes;
            })
            .Distinct()
            .ToArray();

        return supportedTypes;
    }

    public class TestClassDao
    {
        public Guid Id { get; set; }
    }

    [Fact]
    protected virtual async Task Provider_type_map_supports_all_desired_dotnet_types()
    {
        using var db = await OpenConnectionAsync();

        // desired supported types
        Type[] desiredSupportedTypes =
        [
            typeof(byte),
            typeof(short),
            typeof(int),
            typeof(long),
            typeof(bool),
            typeof(float),
            typeof(double),
            typeof(decimal),
            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(TimeSpan),
            typeof(byte[]),
            typeof(object),
            typeof(string),
            typeof(Guid),
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

        var dbTypeMap = db.GetProviderTypeMap();
        var actualSupportedTypes = GetSupportedTypes(dbTypeMap);

        foreach (var desiredType in desiredSupportedTypes)
        {
            var exists = dbTypeMap.TryGetRecommendedSqlTypeMatchingDotnetType(
                desiredType,
                out var sqlType
            );

            Assert.True(exists, "Could not find a SQL type for " + desiredType.FullName);
            Assert.NotNull(sqlType);
        }
    }
}
