namespace DapperMatic.Providers;

public interface IProviderTypeMap
{
    ProviderDataType[] GetProviderDataTypes();
    ProviderDataType GetRecommendedDataTypeForDotnetType(Type dotnetType);
    ProviderDataType[] GetSupportedDataTypesForDotnetType(Type dotnetType);
    ProviderDataType GetRecommendedDataTypeForSqlType(string sqlTypeWithLengthPrecisionOrScale);
}
