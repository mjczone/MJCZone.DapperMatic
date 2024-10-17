namespace DapperMatic.Providers;

public interface IProviderTypeMap
{
    public IReadOnlyList<ProviderSqlType> GetProviderSqlTypes();

    bool TryAddOrUpdateProviderSqlType(ProviderSqlType providerSqlType);

    void AddDotnetTypeToSqlTypeMap(Func<Type, string?> map);

    void AddSqlTypeToDotnetTypeMap(
        Func<
            string,
            (Type dotnetType, int? length, int? precision, int? scale, Type[] otherSupportedTypes)?
        > map
    );

    public bool TryGetRecommendedDotnetTypeMatchingSqlType(
        string fullSqlType,
        out (
            Type dotnetType,
            int? length,
            int? precision,
            int? scale,
            Type[] otherSupportedTypes
        )? recommendedDotnetType
    );

    public bool TryGetRecommendedSqlTypeMatchingDotnetType(
        Type dotnetType,
        out ProviderSqlType? recommendedSqlType
    );
}
