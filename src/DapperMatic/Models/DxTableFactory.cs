using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using DapperMatic.DataAnnotations;

namespace DapperMatic.Models;

public static class DxTableFactory
{
    private static ConcurrentDictionary<Type, DxTable> _cache = new();

    /// <summary>
    /// Returns an instance of a DxTable for the given type. If the type is not a valid DxTable,
    /// denoted by the use of a DxTableAAttribute on the class, this method returns null.
    /// </summary>
    public static DxTable? GetTable(Type type)
    {
        if (_cache.TryGetValue(type, out var table))
            return table;

        var tableAttribute = type.GetCustomAttribute<DxTableAttribute>();
        if (tableAttribute == null)
            return null;

        var schemaName = string.IsNullOrWhiteSpace(tableAttribute.SchemaName)
            ? null
            : tableAttribute.SchemaName;

        var tableName = string.IsNullOrWhiteSpace(tableAttribute.TableName)
            ? type.Name
            : tableAttribute.TableName;

        // columns must bind to public properties that can be both read and written
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.CanWrite);
        var propertyNameToColumnMap = new Dictionary<string, DxColumn>();

        DxPrimaryKeyConstraint? primaryKey = null;
        var columns = new List<DxColumn>();
        var checkConstraints = new List<DxCheckConstraint>();
        var defaultConstraints = new List<DxDefaultConstraint>();
        var uniqueConstraints = new List<DxUniqueConstraint>();
        var foreignKeyConstraints = new List<DxForeignKeyConstraint>();
        var indexes = new List<DxIndex>();

        foreach (var property in properties)
        {
            var columnAttribute = property.GetCustomAttribute<DxColumnAttribute>();
            var columnName = string.IsNullOrWhiteSpace(tableAttribute?.TableName)
                ? type.Name
                : tableAttribute.TableName;

            var column = new DxColumn(
                schemaName,
                tableName,
                columnName,
                property.PropertyType,
                columnAttribute?.ProviderDataType,
                columnAttribute?.Length,
                columnAttribute?.Precision,
                columnAttribute?.Scale,
                string.IsNullOrWhiteSpace(columnAttribute?.CheckExpression)
                    ? null
                    : columnAttribute?.CheckExpression,
                string.IsNullOrWhiteSpace(columnAttribute?.DefaultExpression)
                    ? null
                    : columnAttribute?.DefaultExpression,
                columnAttribute?.IsNullable ?? true,
                columnAttribute?.IsPrimaryKey ?? false,
                columnAttribute?.IsAutoIncrement ?? false,
                columnAttribute?.IsUnique ?? false,
                columnAttribute?.IsIndexed ?? false,
                columnAttribute?.IsForeignKey ?? false,
                string.IsNullOrWhiteSpace(columnAttribute?.ReferencedTableName)
                    ? null
                    : columnAttribute?.ReferencedTableName,
                string.IsNullOrWhiteSpace(columnAttribute?.ReferencedColumnName)
                    ? null
                    : columnAttribute?.ReferencedColumnName,
                columnAttribute?.OnDelete ?? null,
                columnAttribute?.OnUpdate ?? null
            );
            columns.Add(column);
            propertyNameToColumnMap.Add(property.Name, column);

            if (column.Length == null)
            {
                var stringLengthAttribute = property.GetCustomAttribute<StringLengthAttribute>();
                if (stringLengthAttribute != null)
                {
                    column.Length = stringLengthAttribute.MaximumLength;
                }
            }

            // set primary key if present
            var columnPrimaryKeyAttribute =
                property.GetCustomAttribute<DxPrimaryKeyConstraintAttribute>();
            if (columnPrimaryKeyAttribute != null)
            {
                column.IsPrimaryKey = true;
                if (primaryKey == null)
                {
                    primaryKey = new DxPrimaryKeyConstraint(
                        schemaName,
                        tableName,
                        !string.IsNullOrWhiteSpace(columnPrimaryKeyAttribute.ConstraintName)
                            ? columnPrimaryKeyAttribute.ConstraintName
                            : string.Empty,
                        [new(columnName)]
                    );
                }
                else
                {
                    primaryKey.Columns =
                    [
                        .. new List<DxOrderedColumn>(primaryKey.Columns) { new(columnName) }
                    ];
                    if (!string.IsNullOrWhiteSpace(columnPrimaryKeyAttribute.ConstraintName))
                    {
                        primaryKey.ConstraintName = columnPrimaryKeyAttribute.ConstraintName;
                    }
                }
            }

            // set check expression if present
            var columnCheckConstraintAttribute =
                property.GetCustomAttribute<DxCheckConstraintAttribute>();
            if (columnCheckConstraintAttribute != null)
            {
                var checkConstraint = new DxCheckConstraint(
                    schemaName,
                    tableName,
                    columnName,
                    !string.IsNullOrWhiteSpace(columnCheckConstraintAttribute.ConstraintName)
                        ? columnCheckConstraintAttribute.ConstraintName
                        : $"ck_{tableName}_{columnName}",
                    columnCheckConstraintAttribute.Expression
                );
                checkConstraints.Add(checkConstraint);

                column.CheckExpression = columnCheckConstraintAttribute.Expression;
            }

            // set default expression if present
            var columnDefaultConstraintAttribute =
                property.GetCustomAttribute<DxDefaultConstraintAttribute>();
            if (columnDefaultConstraintAttribute != null)
            {
                var defaultConstraint = new DxDefaultConstraint(
                    schemaName,
                    tableName,
                    columnName,
                    !string.IsNullOrWhiteSpace(columnDefaultConstraintAttribute.ConstraintName)
                        ? columnDefaultConstraintAttribute.ConstraintName
                        : $"df_{tableName}_{columnName}",
                    columnDefaultConstraintAttribute.Expression
                );
                defaultConstraints.Add(defaultConstraint);

                column.DefaultExpression = columnDefaultConstraintAttribute.Expression;
            }

            // set unique constraint if present
            var columnUniqueConstraintAttribute =
                property.GetCustomAttribute<DxUniqueConstraintAttribute>();
            if (columnUniqueConstraintAttribute != null)
            {
                var uniqueConstraint = new DxUniqueConstraint(
                    schemaName,
                    tableName,
                    !string.IsNullOrWhiteSpace(columnUniqueConstraintAttribute.ConstraintName)
                        ? columnUniqueConstraintAttribute.ConstraintName
                        : $"uc_{tableName}_{columnName}",
                    [new(columnName)]
                );
                uniqueConstraints.Add(uniqueConstraint);

                column.IsUnique = true;
            }

            // set index if present
            var columnIndexAttribute = property.GetCustomAttribute<DxIndexAttribute>();
            if (columnIndexAttribute != null)
            {
                var index = new DxIndex(
                    schemaName,
                    tableName,
                    !string.IsNullOrWhiteSpace(columnIndexAttribute.IndexName)
                        ? columnIndexAttribute.IndexName
                        : $"ix_{tableName}_{columnName}",
                    [new(columnName)],
                    isUnique: columnIndexAttribute.IsUnique
                );
                indexes.Add(index);

                column.IsIndexed = true;
                if (index.IsUnique)
                    column.IsUnique = true;
            }

            // set foreign key constraint if present
            var columnForeignKeyConstraintAttribute =
                property.GetCustomAttribute<DxForeignKeyConstraintAttribute>();
            if (columnForeignKeyConstraintAttribute != null)
            {
                var referencedTableName = columnForeignKeyConstraintAttribute.ReferencedTableName;
                var referencedColumnNames =
                    columnForeignKeyConstraintAttribute.ReferencedColumnNames;
                var onDelete = columnForeignKeyConstraintAttribute.OnDelete;
                var onUpdate = columnForeignKeyConstraintAttribute.OnUpdate;
                if (
                    !string.IsNullOrWhiteSpace(referencedTableName)
                    && referencedColumnNames != null
                    && referencedColumnNames.Length > 0
                    && !string.IsNullOrWhiteSpace(referencedColumnNames[0])
                )
                {
                    var foreignKeyConstraint = new DxForeignKeyConstraint(
                        schemaName,
                        tableName,
                        !string.IsNullOrWhiteSpace(
                            columnForeignKeyConstraintAttribute.ConstraintName
                        )
                            ? columnForeignKeyConstraintAttribute.ConstraintName
                            : $"fk_{tableName}_{columnName}_{referencedTableName}_{referencedColumnNames[0]}",
                        [new(columnName)],
                        referencedTableName,
                        [new(referencedColumnNames[0])],
                        onDelete ?? DxForeignKeyAction.NoAction,
                        onUpdate ?? DxForeignKeyAction.NoAction
                    );
                    foreignKeyConstraints.Add(foreignKeyConstraint);

                    column.IsForeignKey = true;
                    column.ReferencedTableName = referencedTableName;
                    column.ReferencedColumnName = referencedColumnNames[0];
                    column.OnDelete = onDelete;
                    column.OnUpdate = onUpdate;
                }
            }

            if (columnAttribute == null)
                continue;

            columns.Add(column);
        }

        // TRUST that the developer knows what they are doing and not creating double the amount of attributes then
        // necessary. Class level attributes get used without questioning.

        var cpa = type.GetCustomAttribute<DxPrimaryKeyConstraintAttribute>();
        if (cpa != null && cpa.Columns != null)
        {
            var constraintName = !string.IsNullOrWhiteSpace(cpa.ConstraintName)
                ? cpa.ConstraintName
                : $"pk_{tableName}_{string.Join('_', cpa.Columns.Select(c => c.ColumnName))}";

            primaryKey = new DxPrimaryKeyConstraint(
                schemaName,
                tableName,
                constraintName,
                cpa.Columns
            );

            // flag the column as part of the primary key
            foreach (var c in cpa.Columns)
            {
                var column = columns.FirstOrDefault(c =>
                    c.ColumnName.Equals(c.ColumnName, StringComparison.OrdinalIgnoreCase)
                );
                if (column != null)
                    column.IsPrimaryKey = true;
            }
        }

        var ccas = type.GetCustomAttributes<DxCheckConstraintAttribute>();
        var ccaId = 1;
        foreach (var cca in ccas)
        {
            if (cca != null && !string.IsNullOrWhiteSpace(cca.Expression))
            {
                var constraintName = !string.IsNullOrWhiteSpace(cca.ConstraintName)
                    ? cca.ConstraintName
                    : $"ck_{tableName}_{ccaId++}";

                checkConstraints.Add(
                    new DxCheckConstraint(
                        schemaName,
                        tableName,
                        null,
                        constraintName,
                        cca.Expression
                    )
                );
            }
        }

        var ucas = type.GetCustomAttributes<DxUniqueConstraintAttribute>() ?? [];
        foreach (var uca in ucas)
        {
            if (uca.Columns == null)
                continue;

            var constraintName = !string.IsNullOrWhiteSpace(uca.ConstraintName)
                ? uca.ConstraintName
                : $"uc_{tableName}_{string.Join('_', uca.Columns.Select(c => c.ColumnName))}";

            uniqueConstraints.Add(
                new DxUniqueConstraint(schemaName, tableName, constraintName, uca.Columns)
            );

            if (uca.Columns.Length == 1)
            {
                var column = columns.FirstOrDefault(c =>
                    c.ColumnName.Equals(
                        uca.Columns[0].ColumnName,
                        StringComparison.OrdinalIgnoreCase
                    )
                );
                if (column != null)
                    column.IsUnique = true;
            }
        }

        var cias = type.GetCustomAttributes<DxIndexAttribute>();
        foreach (var cia in cias)
        {
            if (cia.Columns == null)
                continue;

            var indexName = !string.IsNullOrWhiteSpace(cia.IndexName)
                ? cia.IndexName
                : $"ix_{tableName}_{string.Join('_', cia.Columns.Select(c => c.ColumnName))}";

            indexes.Add(
                new DxIndex(schemaName, tableName, indexName, cia.Columns, isUnique: cia.IsUnique)
            );

            if (cia.Columns.Length == 1)
            {
                var column = columns.FirstOrDefault(c =>
                    c.ColumnName.Equals(
                        cia.Columns[0].ColumnName,
                        StringComparison.OrdinalIgnoreCase
                    )
                );
                if (column != null)
                {
                    column.IsIndexed = true;
                    if (cia.IsUnique)
                        column.IsUnique = true;
                }
            }
        }

        var cfkas = type.GetCustomAttributes<DxForeignKeyConstraintAttribute>();
        foreach (var cfk in cfkas)
        {
            if (
                cfk.SourceColumnNames == null
                || cfk.SourceColumnNames.Length == 0
                || string.IsNullOrWhiteSpace(cfk.ReferencedTableName)
                || cfk.ReferencedColumnNames == null
                || cfk.ReferencedColumnNames.Length == 0
                || cfk.SourceColumnNames.Length != cfk.ReferencedColumnNames.Length
            )
                continue;

            var constraintName = !string.IsNullOrWhiteSpace(cfk.ConstraintName)
                ? cfk.ConstraintName
                : $"fk_{tableName}_{string.Join('_', cfk.SourceColumnNames)}_{cfk.ReferencedTableName}_{string.Join('_', cfk.ReferencedColumnNames)}";

            var foreignKeyConstraint = new DxForeignKeyConstraint(
                schemaName,
                tableName,
                constraintName,
                [.. cfk.SourceColumnNames.Select(c => new DxOrderedColumn(c))],
                cfk.ReferencedTableName,
                [.. cfk.ReferencedColumnNames.Select(c => new DxOrderedColumn(c))],
                cfk.OnDelete ?? DxForeignKeyAction.NoAction,
                cfk.OnUpdate ?? DxForeignKeyAction.NoAction
            );

            foreignKeyConstraints.Add(foreignKeyConstraint);

            for (int i = 0; i < cfk.SourceColumnNames.Length; i++)
            {
                var sc = cfk.SourceColumnNames[i];
                var column = columns.FirstOrDefault(c =>
                    c.ColumnName.Equals(sc, StringComparison.OrdinalIgnoreCase)
                );
                if (column != null)
                {
                    column.IsForeignKey = true;
                    column.ReferencedTableName = cfk.ReferencedTableName;
                    column.ReferencedColumnName = cfk.ReferencedColumnNames[i];
                    column.OnDelete = cfk.OnDelete;
                    column.OnUpdate = cfk.OnUpdate;
                }
            }
        }

        table = new DxTable(
            schemaName,
            tableName,
            [.. columns],
            primaryKey,
            [.. checkConstraints],
            [.. defaultConstraints],
            [.. uniqueConstraints],
            [.. foreignKeyConstraints],
            [.. indexes]
        );

        _cache.TryAdd(type, table);
        return table;
    }
}
