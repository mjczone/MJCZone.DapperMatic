using System.Collections.Concurrent;
using System.Data;
using Dapper;

namespace DapperMatic.Providers;

public abstract class DatabaseExtensionsBase
{
    protected abstract string DefaultSchema { get; }

    protected abstract List<DataTypeMap> DataTypes { get; }

    protected DataTypeMap? GetDbType(Type type)
    {
        var dotnetType = Nullable.GetUnderlyingType(type) ?? type;
        return DataTypes.FirstOrDefault(x => x.DotnetType == type);
    }

    protected string GetSqlTypeString(
        Type type,
        int? length = null,
        int? precision = null,
        int? scale = null
    )
    {
        var dotnetType = Nullable.GetUnderlyingType(type) ?? type;
        var dataType = GetDbType(dotnetType);

        if (dataType == null)
        {
            throw new NotSupportedException($"Type {type} is not supported.");
        }

        if (length != null && length > 0)
        {
            if (length == int.MaxValue)
            {
                return string.Format(dataType.SqlTypeWithMaxLength ?? dataType.SqlType, length);
            }
            else
            {
                return string.Format(dataType.SqlTypeWithLength ?? dataType.SqlType, length);
            }
        }
        else if (precision != null)
        {
            return string.Format(
                dataType.SqlTypeWithPrecisionAndScale ?? dataType.SqlType,
                precision,
                scale ?? 0
            );
        }

        return dataType.SqlType;
    }

    protected virtual string NormalizeName(string name)
    {
        return ToAlphaNumericString(name);
    }

    protected virtual string NormalizeSchemaName(string? schemaName)
    {
        if (string.IsNullOrWhiteSpace(schemaName))
            schemaName = DefaultSchema;
        else
            schemaName = NormalizeName(schemaName);

        return schemaName;
    }

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

    protected virtual string ToAlphaNumericString(string text)
    {
        // var rgx = new Regex("[^a-zA-Z0-9_.]");
        // return rgx.Replace(text, "");

        char[] arr = text.Where(c =>
                char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) || c == '-' || c == '_' || c == '.'
            )
            .ToArray();

        return new string(arr);
    }

    public virtual Task<bool> SupportsSchemasAsync(
        IDbConnection connection,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return Task.FromResult(true);
    }

    public virtual Task<bool> SupportsNamedForeignKeysAsync(
        IDbConnection connection,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        return Task.FromResult(true);
    }

    internal static readonly ConcurrentDictionary<
        string,
        (string sql, object? parameters)
    > _lastSqls = new();

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

    protected virtual async Task<IEnumerable<TOutput>> QueryAsync<TOutput>(
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
            SetLastSql(connection, sql, param);
            return await connection.QueryAsync<TOutput>(
                sql,
                param,
                transaction,
                commandTimeout,
                commandType
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine("SQL: " + sql);
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
            Console.WriteLine(ex.Message);
            Console.WriteLine("SQL: " + sql);
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
            Console.WriteLine(ex.Message);
            Console.WriteLine("SQL: " + sql);
            throw;
        }
    }
}
