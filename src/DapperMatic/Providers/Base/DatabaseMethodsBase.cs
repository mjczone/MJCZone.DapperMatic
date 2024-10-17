using System.Collections.Concurrent;
using System.Data;
using System.Text.Json;
using Dapper;
using DapperMatic.Interfaces;
using DapperMatic.Logging;
using Microsoft.Extensions.Logging;

namespace DapperMatic.Providers.Base;

public abstract partial class DatabaseMethodsBase : IDatabaseMethods
{
    public abstract DbProviderType ProviderType { get; }

    public abstract IProviderTypeMap ProviderTypeMap { get; }

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

    private ILogger Logger => DxLogger.CreateLogger(GetType());

    public virtual (
        Type dotnetType,
        int? length,
        int? precision,
        int? scale,
        Type[] otherSupportedTypes
    ) GetDotnetTypeFromSqlType(string sqlType)
    {
        if (
            !ProviderTypeMap.TryGetRecommendedDotnetTypeMatchingSqlType(
                sqlType,
                out var providerDataType
            ) || !providerDataType.HasValue
        )
            throw new NotSupportedException($"SQL type {sqlType} is not supported.");

        return providerDataType.Value;
    }

    public string GetSqlTypeFromDotnetType(
        Type type,
        int? length = null,
        int? precision = null,
        int? scale = null
    )
    {
        if (
            !ProviderTypeMap.TryGetRecommendedSqlTypeMatchingDotnetType(
                type,
                out var providerDataType
            )
            || providerDataType == null
        )
            throw new NotSupportedException($"No provider data type found for .NET type {type}.");

        if (providerDataType.SupportsLength())
        {
            length ??= providerDataType.DefaultLength;
            if (length.HasValue)
            {
                if (!string.IsNullOrWhiteSpace(providerDataType.SqlTypeWithLength))
                    return
                        length == int.MaxValue
                        && !string.IsNullOrWhiteSpace(providerDataType.SqlTypeWithMaxLength)
                        ? string.Format(providerDataType.SqlTypeWithMaxLength, length)
                        : string.Format(providerDataType.SqlTypeWithLength, length);
            }
        }
        else if (providerDataType.SupportsPrecision())
        {
            precision ??= providerDataType.DefaultPrecision;
            scale ??= providerDataType.DefaultScale;

            if (
                scale.HasValue
                && !string.IsNullOrWhiteSpace(providerDataType.SqlTypeWithPrecisionAndScale)
            )
                return string.Format(
                    providerDataType.SqlTypeWithPrecisionAndScale,
                    precision,
                    scale
                );

            if (!string.IsNullOrWhiteSpace(providerDataType.SqlTypeWithPrecision))
                return string.Format(providerDataType.SqlTypeWithPrecision, precision);
        }

        return providerDataType.SqlType;
    }

    internal static readonly ConcurrentDictionary<
        string,
        (string sql, object? parameters)
    > LastSqls = new();

    public abstract Task<Version> GetDatabaseVersionAsync(
        IDbConnection db,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    );

    public string GetLastSql(IDbConnection db)
    {
        return LastSqls.TryGetValue(db.ConnectionString, out var sql) ? sql.sql : "";
    }

    public (string sql, object? parameters) GetLastSqlWithParams(IDbConnection db)
    {
        return LastSqls.TryGetValue(db.ConnectionString, out var sql) ? sql : ("", null);
    }

    private static void SetLastSql(IDbConnection db, string sql, object? param = null)
    {
        LastSqls.AddOrUpdate(db.ConnectionString, (sql, param), (_, _) => (sql, param));
    }

    protected virtual async Task<List<TOutput>> QueryAsync<TOutput>(
        IDbConnection db,
        string sql,
        object? param = null,
        IDbTransaction? tx = null,
        int? commandTimeout = null,
        CommandType? commandType = null
    )
    {
        try
        {
            Log(
                LogLevel.Information,
                "[{provider}] Executing SQL query: {sql}, with parameters {parameters}",
                ProviderType,
                sql,
                param == null ? "{}" : JsonSerializer.Serialize(param)
            );

            SetLastSql(db, sql, param);
            return (
                await db.QueryAsync<TOutput>(sql, param, tx, commandTimeout, commandType)
                    .ConfigureAwait(false)
            ).AsList();
        }
        catch (Exception ex)
        {
            Log(
                LogLevel.Error,
                ex,
                "An error occurred while executing SQL query: {sql}, with parameters {parameters}.\n{message}",
                sql,
                param == null ? "{}" : JsonSerializer.Serialize(param),
                ex.Message
            );
            throw;
        }
    }

    protected virtual async Task<TOutput?> ExecuteScalarAsync<TOutput>(
        IDbConnection db,
        string sql,
        object? param = null,
        IDbTransaction? tx = null,
        int? commandTimeout = null,
        CommandType? commandType = null
    )
    {
        try
        {
            Log(
                LogLevel.Information,
                "[{provider}] Executing SQL scalar: {sql}, with parameters {parameters}",
                ProviderType,
                sql,
                param == null ? "{}" : JsonSerializer.Serialize(param)
            );

            SetLastSql(db, sql, param);
            return await db.ExecuteScalarAsync<TOutput>(
                sql,
                param,
                tx,
                commandTimeout,
                commandType
            );
        }
        catch (Exception ex)
        {
            Log(
                LogLevel.Error,
                ex,
                "An error occurred while executing SQL scalar query: {sql}, with parameters {parameters}.\n{message}",
                sql,
                param == null ? "{}" : JsonSerializer.Serialize(param),
                ex.Message
            );
            throw;
        }
    }

    protected virtual async Task<int> ExecuteAsync(
        IDbConnection db,
        string sql,
        object? param = null,
        IDbTransaction? tx = null,
        int? commandTimeout = null,
        CommandType? commandType = null
    )
    {
        try
        {
            Log(
                LogLevel.Information,
                "[{provider}] Executing SQL statement: {sql}, with parameters {parameters}",
                ProviderType,
                sql,
                param == null ? "{}" : JsonSerializer.Serialize(param)
            );

            SetLastSql(db, sql, param);
            return await db.ExecuteAsync(sql, param, tx, commandTimeout, commandType);
        }
        catch (Exception ex)
        {
            Log(
                LogLevel.Error,
                ex,
                "An error occurred while executing SQL statement: {sql}, with parameters {parameters}.\n{message}",
                sql,
                param == null ? "{}" : JsonSerializer.Serialize(param),
                ex.Message
            );
            throw;
        }
    }

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

    // ReSharper disable once MemberCanBePrivate.Global
    protected void Log(LogLevel logLevel, string message, params object?[] args)
    {
        if (!Logger.IsEnabled(logLevel))
            return;

        try
        {
            Logger.Log(logLevel, message, args);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    // ReSharper disable once MemberCanBePrivate.Global
    protected void Log(
        LogLevel logLevel,
        Exception exception,
        string message,
        params object?[] args
    )
    {
        if (!Logger.IsEnabled(logLevel))
            return;

        try
        {
            Logger.Log(logLevel, exception, message, args);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}
