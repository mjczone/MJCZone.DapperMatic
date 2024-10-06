using System.Collections.Concurrent;
using System.Data;
using System.Text.Json;
using Dapper;
using DapperMatic.Logging;
using Microsoft.Extensions.Logging;

namespace DapperMatic.Providers;

public abstract partial class DatabaseMethodsBase : IDatabaseMethods
{
    public abstract DbProviderType ProviderType { get; }
    public virtual bool SupportsOrderedKeysInConstraints => true;
    protected virtual ILogger Logger => DxLogger.CreateLogger(GetType());

    protected virtual List<DataTypeMap> DataTypes =>
        DataTypeMapFactory.GetDefaultDbProviderDataTypeMap(ProviderType);

    protected DataTypeMap? GetDataType(Type type)
    {
        var dotnetType = Nullable.GetUnderlyingType(type) ?? type;
        return DataTypes.FirstOrDefault(x => x.DotnetType == type);
    }

    public abstract Type GetDotnetTypeFromSqlType(string sqlType);

    public string GetSqlTypeFromDotnetType(
        Type type,
        int? length = null,
        int? precision = null,
        int? scale = null
    )
    {
        var dotnetType = Nullable.GetUnderlyingType(type) ?? type;
        var dataType = GetDataType(dotnetType);

        if (dataType == null)
        {
            throw new NotSupportedException($"Type {type} is not supported.");
        }

        string? sqlType = null;

        if (length != null && length > 0)
        {
            // there are times where a length is passed in, but the datatype supports precision instead, accommodate for that case
            if (
                precision == null
                && scale == null
                && string.IsNullOrWhiteSpace(dataType.SqlTypeWithLength)
                && string.IsNullOrWhiteSpace(dataType.SqlTypeWithMaxLength)
                && !string.IsNullOrWhiteSpace(dataType.SqlTypeWithPrecisionAndScale)
            )
            {
                sqlType = string.Format(
                    dataType.SqlTypeWithPrecisionAndScale ?? dataType.SqlType,
                    length,
                    0
                );
            }
            else if (length == int.MaxValue)
            {
                sqlType = string.Format(dataType.SqlTypeWithMaxLength ?? dataType.SqlType, length);
            }
            else
            {
                sqlType = string.Format(dataType.SqlTypeWithLength ?? dataType.SqlType, length);
            }
        }
        else if (precision != null)
        {
            sqlType = string.Format(
                dataType.SqlTypeWithPrecisionAndScale ?? dataType.SqlType,
                precision,
                scale ?? 0
            );
        }

        return sqlType ?? dataType.SqlType;
    }

    internal static readonly ConcurrentDictionary<
        string,
        (string sql, object? parameters)
    > _lastSqls = new();

    public abstract Task<string> GetDatabaseVersionAsync(
        IDbConnection connection,
        IDbTransaction? tx,
        CancellationToken cancellationToken = default
    );

    public string GetLastSql(IDbConnection connection)
    {
        return _lastSqls.TryGetValue(connection.ConnectionString, out var sql) ? sql.sql : "";
    }

    public (string sql, object? parameters) GetLastSqlWithParams(IDbConnection connection)
    {
        return _lastSqls.TryGetValue(connection.ConnectionString, out var sql) ? sql : ("", null);
    }

    private static void SetLastSql(IDbConnection connection, string sql, object? param = null)
    {
        _lastSqls.AddOrUpdate(
            connection.ConnectionString,
            (sql, param),
            (key, oldValue) => (sql, param)
        );
    }

    protected virtual async Task<List<TOutput>> QueryAsync<TOutput>(
        IDbConnection connection,
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null
    )
    {
        try
        {
            Logger.LogInformation(
                "[{provider}] Executing SQL query: {sql}, with parameters {parameters}",
                ProviderType,
                sql,
                param == null ? "{}" : JsonSerializer.Serialize(param)
            );

            SetLastSql(connection, sql, param);
            return (
                await connection
                    .QueryAsync<TOutput>(sql, param, transaction, commandTimeout, commandType)
                    .ConfigureAwait(false)
            ).AsList();
        }
        catch (Exception ex)
        {
            Logger.LogError(
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
        IDbConnection connection,
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null
    )
    {
        try
        {
            Logger.LogInformation(
                "[{provider}] Executing SQL scalar: {sql}, with parameters {parameters}",
                ProviderType,
                sql,
                param == null ? "{}" : JsonSerializer.Serialize(param)
            );

            SetLastSql(connection, sql, param);
            return await connection.ExecuteScalarAsync<TOutput>(
                sql,
                param,
                transaction,
                commandTimeout,
                commandType
            );
        }
        catch (Exception ex)
        {
            Logger.LogError(
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
        IDbConnection connection,
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null
    )
    {
        try
        {
            Logger.LogInformation(
                "[{provider}] Executing SQL statement: {sql}, with parameters {parameters}",
                ProviderType,
                sql,
                param == null ? "{}" : JsonSerializer.Serialize(param)
            );

            SetLastSql(connection, sql, param);
            return await connection.ExecuteAsync(
                sql,
                param,
                transaction,
                commandTimeout,
                commandType
            );
        }
        catch (Exception ex)
        {
            Logger.LogError(
                ex,
                "An error occurred while executing SQL statement: {sql}, with parameters {parameters}.\n{message}",
                sql,
                param == null ? "{}" : JsonSerializer.Serialize(param),
                ex.Message
            );
            throw;
        }
    }

    // create a wildcard pattern matching algorithm that accepts wildcards (*) and questions (?)
    // for example:
    // *abc* should match abc, abcd, abcdabc, etc.
    // a?c should match ac, abc, abcc, etc.
    // the method should take in a string and a wildcard pattern and return true/false whether the string
    // matches the wildcard pattern.
    /// <summary>
    /// Wildcard pattern matching algorithm. Accepts wildcards (*) and question marks (?)
    /// </summary>
    /// <param name="input">A string</param>
    /// <param name="wildcardPattern">Wildcard pattern string</param>
    /// <returns>bool</returns>
    protected virtual bool IsWildcardPatternMatch(
        string input,
        string wildcardPattern,
        bool ignoreCase = true
    )
    {
        if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(wildcardPattern))
            return false;

        if (ignoreCase)
        {
            input = input.ToLowerInvariant();
            wildcardPattern = wildcardPattern.ToLowerInvariant();
        }

        var inputIndex = 0;
        var patternIndex = 0;
        var inputLength = input.Length;
        var patternLength = wildcardPattern.Length;
        var lastWildcardIndex = -1;
        var lastInputIndex = -1;

        while (inputIndex < inputLength)
        {
            if (
                patternIndex < patternLength
                && (
                    wildcardPattern[patternIndex] == '?'
                    || wildcardPattern[patternIndex] == input[inputIndex]
                )
            )
            {
                patternIndex++;
                inputIndex++;
            }
            else if (patternIndex < patternLength && wildcardPattern[patternIndex] == '*')
            {
                lastWildcardIndex = patternIndex;
                lastInputIndex = inputIndex;
                patternIndex++;
            }
            else if (lastWildcardIndex != -1)
            {
                patternIndex = lastWildcardIndex + 1;
                lastInputIndex++;
                inputIndex = lastInputIndex;
            }
            else
            {
                return false;
            }
        }

        while (patternIndex < patternLength && wildcardPattern[patternIndex] == '*')
        {
            patternIndex++;
        }

        return patternIndex == patternLength;
    }

    protected virtual void ExtractColumnTypeInfoFromFullSqlType(
        string data_type,
        string data_type_ext,
        out Type dotnetType,
        out int? length,
        out int? precision,
        out int? scale
    )
    {
        dotnetType = GetDotnetTypeFromSqlType(data_type);
        if (!data_type_ext.Contains('('))
        {
            length = null;
            precision = null;
            scale = null;
            return;
        }

        // extract length, precision, and scale from data_type_ext
        // example: data_type_ext = 'character varying(255)' or 'numeric(18,2)' or 'time(6) with time zone'
        var typeInfo = data_type_ext.Split('(')[1].Split(')')[0].Trim().Split(',');

        if (typeInfo.Length == 2)
        {
            length = null;
            precision = int.TryParse(typeInfo[0], out var p) ? p : null;
            scale = int.TryParse(typeInfo[1], out var s) ? s : null;
            return;
        }

        if (typeInfo.Length == 1)
        {
            // detect it it's a length using the data_type, otherwise it's a precision
            if (
                data_type.Contains("char", StringComparison.OrdinalIgnoreCase)
                || data_type.Contains("bit", StringComparison.OrdinalIgnoreCase)
                || data_type.Contains("text", StringComparison.OrdinalIgnoreCase)
            )
            {
                length = int.TryParse(typeInfo[0], out var l) ? l : null;
                precision = null;
                scale = null;
            }
            else
            {
                length = null;
                precision = int.TryParse(typeInfo[0], out var p) ? p : null;
                scale = null;
            }
            return;
        }

        length = null;
        precision = null;
        scale = null;
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
        if (!SupportsSchemas || string.IsNullOrWhiteSpace(schemaName))
            schemaName = DefaultSchema;
        else
            schemaName = NormalizeName(schemaName);

        return schemaName;
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

        return (schemaName ?? "", tableName ?? "", identifierName ?? "");
    }
}
