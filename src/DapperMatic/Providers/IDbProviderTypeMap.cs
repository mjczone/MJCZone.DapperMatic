namespace DapperMatic.Providers;

public interface IDbProviderTypeMap
{
    bool TryGetDotnetTypeDescriptorMatchingFullSqlTypeName(
        string fullSqlType,
        out DbProviderDotnetTypeDescriptor? descriptor
    );

    bool TryGetProviderSqlTypeMatchingDotnetType(
        DbProviderDotnetTypeDescriptor descriptor,
        out DbProviderSqlType? providerSqlType
    );
}
