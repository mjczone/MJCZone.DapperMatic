using System.Data;
using Dapper;
using DapperMatic.Interfaces;

namespace DapperMatic.Providers.Base;

public abstract class DatabaseMethodsBase<TMap> : DatabaseMethodsBase
    where TMap : IDbProviderTypeMap, new()
{
    internal DatabaseMethodsBase(DbProviderType providerType)
    {
        ProviderType = providerType;
    }

    public override DbProviderType ProviderType { get; }

    public override IDbProviderTypeMap ProviderTypeMap => new TMap();

    protected override async Task<List<TOutput>> QueryAsync<TOutput>(
        IDbConnection db,
        string sql,
        object? param = null,
        IDbTransaction? tx = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default
    )
    {
        return (
            await db.QueryAsync<TOutput>(sql, param, tx, commandTimeout, commandType)
                .ConfigureAwait(false)
        ).AsList();
    }

    protected override async Task<TOutput?> ExecuteScalarAsync<TOutput>(
        IDbConnection db,
        string sql,
        object? param = null,
        IDbTransaction? tx = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default
    )
        where TOutput : default
    {
        var result = await db.ExecuteScalarAsync<TOutput>(
            sql,
            param,
            tx,
            commandTimeout,
            commandType
        );
        return result;
    }

    protected override async Task<int> ExecuteAsync(
        IDbConnection db,
        string sql,
        object? param = null,
        IDbTransaction? tx = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default
    )
    {
        return await db.ExecuteAsync(sql, param, tx, commandTimeout, commandType);
    }
}

public abstract partial class DatabaseMethodsBase : IDatabaseMethods
{
    public abstract DbProviderType ProviderType { get; }

    public abstract IDbProviderTypeMap ProviderTypeMap { get; }

    protected abstract string DefaultSchema { get; }

    public virtual bool SupportsSchemas => !string.IsNullOrWhiteSpace(DefaultSchema);

    public virtual Task<bool> SupportsCheckConstraintsAsync(
        IDbConnection db,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    ) => Task.FromResult(true);

    public virtual Task<bool> SupportsOrderedKeysInConstraintsAsync(
        IDbConnection db,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    ) => Task.FromResult(true);

    public virtual Task<bool> SupportsDefaultConstraintsAsync(
        IDbConnection db,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    ) => Task.FromResult(true);

    public virtual DotnetTypeDescriptor GetDotnetTypeFromSqlType(string sqlType)
    {
        if (
            ProviderTypeMap.TryGetDotnetTypeDescriptorMatchingFullSqlTypeName(
                sqlType,
                out var dotnetTypeDescriptor
            )
            && dotnetTypeDescriptor?.DotnetType != null
        )
            return dotnetTypeDescriptor;

        throw new NotSupportedException($"SQL type {sqlType} is not supported.");
    }

    public string GetSqlTypeFromDotnetType(DotnetTypeDescriptor descriptor)
    {
        if (
            ProviderTypeMap.TryGetProviderSqlTypeMatchingDotnetType(
                descriptor,
                out var providerDataType
            ) && !string.IsNullOrWhiteSpace(providerDataType?.SqlTypeName)
        )
            return providerDataType.SqlTypeName;

        throw new NotSupportedException($"No provider data type found for .NET type {descriptor}.");
    }

    public abstract Task<Version> GetDatabaseVersionAsync(
        IDbConnection db,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );

    protected abstract Task<List<TOutput>> QueryAsync<TOutput>(
        IDbConnection db,
        string sql,
        object? param = null,
        IDbTransaction? tx = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default
    );

    protected abstract Task<TOutput?> ExecuteScalarAsync<TOutput>(
        IDbConnection db,
        string sql,
        object? param = null,
        IDbTransaction? tx = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default
    );

    protected abstract Task<int> ExecuteAsync(
        IDbConnection db,
        string sql,
        object? param = null,
        IDbTransaction? tx = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default
    );

    public abstract char[] QuoteChars { get; }

    protected virtual string GetQuotedIdentifier(string identifier)
    {
        return "".ToQuotedIdentifier(QuoteChars, identifier);
    }

    protected virtual string GetQuotedCompoundIdentifier(string[] identifiers, string union = ".")
    {
        return string.Join(union, identifiers.Select(x => "".ToQuotedIdentifier(QuoteChars, x)));
    }

    protected virtual string ToSafeString(string text, string allowedSpecialChars = "-_.*")
    {
        return text.ToAlphaNumeric(allowedSpecialChars);
    }

    protected virtual string ToLikeString(string text, string allowedSpecialChars = "-_.*")
    {
        return text.ToAlphaNumeric(allowedSpecialChars).Replace("*", "%"); //.Replace("?", "_");
    }

    public virtual string GetSchemaQualifiedIdentifierName(string? schemaName, string tableName)
    {
        schemaName = NormalizeSchemaName(schemaName);
        tableName = NormalizeName(tableName);

        return SupportsSchemas && !string.IsNullOrWhiteSpace(schemaName)
            ? $"{schemaName.ToQuotedIdentifier(QuoteChars)}.{tableName.ToQuotedIdentifier(QuoteChars)}"
            : tableName.ToQuotedIdentifier(QuoteChars);
    }

    /// <summary>
    /// The default implementation simply removes all non-alphanumeric characters from the provided name identifier, replacing them with underscores.
    /// </summary>
    public virtual string NormalizeName(string name)
    {
        return name.ToAlphaNumeric("_");
    }

    /// <summary>
    /// The schema name is normalized to the default schema if it is null or empty.
    /// If the default schema is null or empty,
    /// the implementation simply removes all non-alphanumeric characters from the provided name, replacing them with underscores.
    /// </summary>
    /// <param name="schemaName"></param>
    /// <returns></returns>
    protected virtual string NormalizeSchemaName(string? schemaName)
    {
        if (!SupportsSchemas)
            return string.Empty;

        return string.IsNullOrWhiteSpace(schemaName) ? DefaultSchema : NormalizeName(schemaName);
    }

    /// <summary>
    /// The default implementation simply removes all non-alphanumeric characters from the schema, table, and identifier names, replacing them with underscores.
    /// The schema name is normalized to the default schema if it is null or empty.
    /// If the default schema is null or empty, the schema name is normalized as the other names.
    /// </summary>
    /// <param name="schemaName"></param>
    /// <param name="tableName"></param>
    /// <param name="identifierName"></param>
    /// <returns></returns>
    protected virtual (string schemaName, string tableName, string identifierName) NormalizeNames(
        string? schemaName = null,
        string? tableName = null,
        string? identifierName = null
    )
    {
        schemaName = NormalizeSchemaName(schemaName);

        if (!string.IsNullOrWhiteSpace(tableName))
            tableName = NormalizeName(tableName);

        if (!string.IsNullOrWhiteSpace(identifierName))
            identifierName = NormalizeName(identifierName);

        return (schemaName, tableName ?? "", identifierName ?? "");
    }
}
