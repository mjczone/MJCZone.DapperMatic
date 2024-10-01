using System.Data;
using DapperMatic.Models;

namespace DapperMatic.Providers.Sqlite;

public partial class SqliteMethods
{
    public override async Task<bool> CreateForeignKeyConstraintIfNotExistsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string constraintName,
        DxOrderedColumn[] sourceColumns,
        string referencedTableName,
        DxOrderedColumn[] referencedColumns,
        DxForeignKeyAction onDelete = DxForeignKeyAction.NoAction,
        DxForeignKeyAction onUpdate = DxForeignKeyAction.NoAction,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        if (string.IsNullOrWhiteSpace(tableName))
            throw new ArgumentException("Table name is required.", nameof(tableName));

        if (string.IsNullOrWhiteSpace(constraintName))
            throw new ArgumentException("Constraint name is required.", nameof(constraintName));

        if (sourceColumns.Length == 0)
            throw new ArgumentException(
                "At least one column must be specified.",
                nameof(sourceColumns)
            );

        if (string.IsNullOrWhiteSpace(referencedTableName))
            throw new ArgumentException(
                "Referenced table name is required.",
                nameof(referencedTableName)
            );

        if (referencedColumns.Length == 0)
            throw new ArgumentException(
                "At least one column must be specified.",
                nameof(referencedColumns)
            );

        if (sourceColumns.Length != referencedColumns.Length)
            throw new ArgumentException(
                "The number of source columns must match the number of referenced columns.",
                nameof(referencedColumns)
            );

        (schemaName, tableName, constraintName) = NormalizeNames(
            schemaName,
            tableName,
            constraintName
        );

        return await AlterTableUsingRecreateTableStrategyAsync(
                db,
                schemaName,
                tableName,
                table =>
                {
                    return table.ForeignKeyConstraints.All(x =>
                        !x.ConstraintName.Equals(constraintName, StringComparison.OrdinalIgnoreCase)
                    );
                },
                table =>
                {
                    table.ForeignKeyConstraints.Add(
                        new DxForeignKeyConstraint(
                            schemaName,
                            tableName,
                            constraintName,
                            sourceColumns,
                            referencedTableName,
                            referencedColumns,
                            onDelete,
                            onUpdate
                        )
                    );
                    return table;
                },
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }

    public override async Task<bool> DropForeignKeyConstraintIfExistsAsync(
        IDbConnection db,
        string? schemaName,
        string tableName,
        string constraintName,
        IDbTransaction? tx = null,
        CancellationToken cancellationToken = default
    )
    {
        (schemaName, tableName, constraintName) = NormalizeNames(
            schemaName,
            tableName,
            constraintName
        );

        return await AlterTableUsingRecreateTableStrategyAsync(
                db,
                schemaName,
                tableName,
                table =>
                {
                    return table.ForeignKeyConstraints.Any(x =>
                        x.ConstraintName.Equals(constraintName, StringComparison.OrdinalIgnoreCase)
                    );
                },
                table =>
                {
                    var foreignKey = table.ForeignKeyConstraints.FirstOrDefault(x =>
                        x.ConstraintName.Equals(constraintName, StringComparison.OrdinalIgnoreCase)
                    );
                    if (foreignKey is not null)
                    {
                        // remove the foreign key from the related column
                        foreach (var column in foreignKey.SourceColumns)
                        {
                            var sc = table.Columns.FirstOrDefault(x =>
                                x.ColumnName.Equals(
                                    column.ColumnName,
                                    StringComparison.OrdinalIgnoreCase
                                )
                            );
                            if (sc is not null)
                            {
                                sc.IsForeignKey = false;
                                sc.ReferencedTableName = null;
                                sc.ReferencedColumnName = null;
                                sc.OnDelete = null;
                                sc.OnUpdate = null;
                            }
                        }

                        table.ForeignKeyConstraints.Remove(foreignKey);
                    }
                    table.ForeignKeyConstraints.RemoveAll(x =>
                        x.ConstraintName.Equals(constraintName, StringComparison.OrdinalIgnoreCase)
                    );
                    return table;
                },
                tx,
                cancellationToken
            )
            .ConfigureAwait(false);
    }
}
