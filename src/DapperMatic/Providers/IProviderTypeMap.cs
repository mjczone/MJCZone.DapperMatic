namespace DapperMatic.Providers;

public interface IProviderTypeMap
{
    bool TryGetRecommendedDotnetTypeMatchingSqlType(
        string fullSqlType,
        out (Type dotnetType, int? length, int? precision, int? scale, bool? isAutoIncrementing, Type[] allSupportedTypes)? recommendedDotnetType
    );

    bool TryGetRecommendedSqlTypeMatchingDotnetType(
        Type dotnetType,
        int? length,
        int? precision,
        int? scale,
        bool? autoIncrement,
        out ProviderSqlType? recommendedSqlType
    );
}