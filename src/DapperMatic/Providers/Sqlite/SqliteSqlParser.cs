using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;
using DapperMatic.Models;

// ReSharper disable ForCanBeConvertedToForeach

namespace DapperMatic.Providers.Sqlite;

[SuppressMessage("ReSharper", "ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator")]
public static partial class SqliteSqlParser
{
    public static DxTable? ParseCreateTableStatement(string createTableSql)
    {
        var statements = ParseDdlSql(createTableSql);
        if (
            statements.SingleOrDefault() is not SqlCompoundClause createTableStatement
            || createTableStatement.FindTokenIndex("CREATE") != 0
                && createTableStatement.FindTokenIndex("TABLE") != 1
        )
            return null;

        var tableName = createTableStatement.GetChild<SqlWordClause>(2)?.Text;
        if (string.IsNullOrWhiteSpace(tableName))
            return null;

        var table = new DxTable(null, tableName);

        // there are lots of variation of CREATE TABLE statements in SQLite, so we need to handle each variation
        // we can iterate this process to parse different variations and improve this over time, for now, we will
        // brute-force this to get it to work

        // statements we are interested in will look like this, where everything inside the first ( ... ) represent the guts of a table,
        // so we are looking for the first compount clause that has children and is wrapped in parentheses
        // CREATE TABLE table_name ( ... )

        // see: https://www.sqlite.org/lang_createtable.html

        var tableGuts = createTableStatement.GetChild<SqlCompoundClause>(x =>
            x.Children.Count > 0 && x.Parenthesis
        );
        if (tableGuts == null || tableGuts.Children.Count == 0)
            return table;

        // we now iterate over these guts to parse out columns, primary keys, unique constraints, check constraints, default constraints, and foreign key constraints
        // constraint clauses can appear as part of the column definition, or as separate clauses:
        //  - if as part of column definition, they appear inline
        //  - if separate as table constraint definitions, they always start with either the word "CONSTRAINT" or the constraint type identifier "PRIMARY KEY", "FOREIGN KEY", "UNIQUE", "CHECK", "DEFAULT"

        bool IsColumnDefinitionClause(SqlClause clause)
        {
            return !(
                clause.FindTokenIndex("CONSTRAINT") == 0
                || clause.FindTokenIndex("PRIMARY KEY") == 0
                || clause.FindTokenIndex("FOREIGN KEY") == 0
                || clause.FindTokenIndex("UNIQUE") == 0
                || clause.FindTokenIndex("CHECK") == 0
                || clause.FindTokenIndex("DEFAULT") == 0
            );
        }

        // based on the documentation of the CREATE TABLE statement, we know that column definitions appear before table constraint clauses,
        // so we can safely assume that by the time we start parsing constraints, all the column definitions will have been added to the table.columns list
        for (var clauseIndex = 0; clauseIndex < tableGuts.Children.Count; clauseIndex++)
        {
            var clause = tableGuts.Children[clauseIndex];
            // see if it's a column definition or a table constraint
            if (IsColumnDefinitionClause(clause))
            {
                // it's a column definition, parse it
                // see:https://www.sqlite.org/syntax/column-def.html
                if (clause is not SqlCompoundClause columnDefinition)
                    continue;

                // first word in the column name
                var columnName = columnDefinition.GetChild<SqlWordClause>(0)?.Text;
                if (string.IsNullOrWhiteSpace(columnName))
                    continue;

                // second word is the column type
                var columnDataType = columnDefinition.GetChild<SqlWordClause>(1)?.Text;
                if (string.IsNullOrWhiteSpace(columnDataType))
                    continue;

                int? length = null;
                int? precision = null;
                int? scale = null;

                var remainingWordsIndex = 2;
                if (columnDefinition.Children.Count > 2)
                {
                    var thirdChild = columnDefinition.GetChild<SqlCompoundClause>(2);
                    if (thirdChild is { Children.Count: > 0 and <= 2 })
                    {
                        switch (thirdChild.Children.Count)
                        {
                            case 1:
                            {
                                if (
                                    thirdChild.Children[0] is SqlWordClause sw1
                                    && int.TryParse(sw1.Text, out var intValue)
                                )
                                {
                                    length = intValue;
                                }

                                break;
                            }
                            case 2:
                            {
                                if (
                                    thirdChild.Children[0] is SqlWordClause sw1
                                    && int.TryParse(sw1.Text, out var intValue)
                                )
                                {
                                    precision = intValue;
                                }
                                if (
                                    thirdChild.Children[1] is SqlWordClause sw2
                                    && int.TryParse(sw2.Text, out var intValue2)
                                )
                                {
                                    scale = intValue2;
                                }

                                break;
                            }
                        }

                        remainingWordsIndex = 3;
                    }
                }

                var providerTypeMap = SqliteProviderTypeMap.Instance.Value;

                // if we don't recognize the column data type, we skip it
                if (
                    !providerTypeMap.TryGetDotnetTypeDescriptorMatchingFullSqlTypeName(
                        columnDataType,
                        out var dotnetTypeDescriptor
                    )
                    || dotnetTypeDescriptor == null
                )
                    continue;

                var column = new DxColumn(
                    null,
                    tableName,
                    columnName,
                    dotnetTypeDescriptor.DotnetType,
                    new Dictionary<DbProviderType, string>
                    {
                        { DbProviderType.Sqlite, columnDataType }
                    },
                    length,
                    precision,
                    scale
                );
                table.Columns.Add(column);

                // remaining words are optional in the column definition
                if (columnDefinition.Children.Count <= remainingWordsIndex)
                    continue;

                string? inlineConstraintName = null;
                for (var i = remainingWordsIndex; i < columnDefinition.Children.Count; i++)
                {
                    var opt = columnDefinition.Children[i];
                    if (opt is SqlWordClause swc)
                    {
                        switch (swc.Text.ToUpper())
                        {
                            case "NOT NULL":
                                column.IsNullable = false;
                                break;

                            case "AUTOINCREMENT":
                                column.IsAutoIncrement = true;
                                break;

                            case "CONSTRAINT":
                                inlineConstraintName = columnDefinition
                                    .GetChild<SqlWordClause>(i + 1)
                                    ?.Text;
                                // skip the next opt
                                i++;
                                break;

                            case "DEFAULT":
                                // the clause can be a compound clause, or literal-value (quoted), or a number (integer, float, etc.)
                                // if the clause is a compound parenthesized clause, we will remove the parentheses and trim the text
                                column.DefaultExpression = columnDefinition
                                    .GetChild<SqlClause>(i + 1)
                                    ?.ToString()
                                    ?.Trim('(', ')', ' ');
                                // skip the next opt
                                i++;
                                if (!string.IsNullOrWhiteSpace(column.DefaultExpression))
                                {
                                    // add the default constraint to the table
                                    var defaultConstraintName =
                                        inlineConstraintName
                                        ?? DbProviderUtils.GenerateDefaultConstraintName(
                                            tableName,
                                            columnName
                                        );
                                    table.DefaultConstraints.Add(
                                        new DxDefaultConstraint(
                                            null,
                                            tableName,
                                            column.ColumnName,
                                            defaultConstraintName,
                                            column.DefaultExpression
                                        )
                                    );
                                }
                                inlineConstraintName = null;
                                break;

                            case "UNIQUE":
                                column.IsUnique = true;
                                // add the default constraint to the table
                                var uniqueConstraintName =
                                    inlineConstraintName
                                    ?? DbProviderUtils.GenerateUniqueConstraintName(
                                        tableName,
                                        columnName
                                    );
                                table.UniqueConstraints.Add(
                                    new DxUniqueConstraint(
                                        null,
                                        tableName,
                                        uniqueConstraintName,
                                        [new DxOrderedColumn(column.ColumnName)]
                                    )
                                );
                                inlineConstraintName = null;
                                break;

                            case "CHECK":
                                // the check expression is typically a compound clause based on the SQLite documentation
                                // if the check expression is a compound parenthesized clause, we will remove the parentheses and trim the text
                                column.CheckExpression = columnDefinition
                                    .GetChild<SqlClause>(i + 1)
                                    ?.ToString()
                                    ?.Trim('(', ')', ' ');
                                // skip the next opt
                                i++;
                                if (!string.IsNullOrWhiteSpace(column.CheckExpression))
                                {
                                    // add the default constraint to the table
                                    var checkConstraintName =
                                        inlineConstraintName
                                        ?? DbProviderUtils.GenerateCheckConstraintName(
                                            tableName,
                                            columnName
                                        );
                                    table.CheckConstraints.Add(
                                        new DxCheckConstraint(
                                            null,
                                            tableName,
                                            column.ColumnName,
                                            checkConstraintName,
                                            column.CheckExpression
                                        )
                                    );
                                }
                                inlineConstraintName = null;
                                break;

                            case "PRIMARY KEY":
                                column.IsPrimaryKey = true;
                                // add the default constraint to the table
                                var pkConstraintName =
                                    inlineConstraintName
                                    ?? DbProviderUtils.GeneratePrimaryKeyConstraintName(
                                        tableName,
                                        columnName
                                    );
                                var columnOrder = DxColumnOrder.Ascending;
                                if (
                                    columnDefinition
                                        .GetChild<SqlClause>(i + 1)
                                        ?.ToString()
                                        ?.Equals("DESC", StringComparison.OrdinalIgnoreCase) == true
                                )
                                {
                                    columnOrder = DxColumnOrder.Descending;
                                    // skip the next opt
                                    i++;
                                }
                                table.PrimaryKeyConstraint = new DxPrimaryKeyConstraint(
                                    null,
                                    tableName,
                                    pkConstraintName,
                                    [new DxOrderedColumn(column.ColumnName, columnOrder)]
                                );
                                inlineConstraintName = null;
                                break;

                            case "REFERENCES":
                                // see: https://www.sqlite.org/syntax/foreign-key-clause.html
                                column.IsForeignKey = true;

                                var referenceTableNameIndex = i + 1;
                                var referenceColumnNamesIndex = i + 2;

                                var referencedTableName = columnDefinition
                                    .GetChild<SqlWordClause>(referenceTableNameIndex)
                                    ?.Text;
                                if (string.IsNullOrWhiteSpace(referencedTableName))
                                    break;

                                // skip next opt
                                i++;

                                // TODO: sqlite doesn't require the referenced column name, but we will for now in our library
                                var referenceColumnName = columnDefinition
                                    .GetChild<SqlCompoundClause>(referenceColumnNamesIndex)
                                    ?.GetChild<SqlWordClause>(0)
                                    ?.Text;
                                if (string.IsNullOrWhiteSpace(referenceColumnName))
                                    break;

                                // skip next opt
                                i++;

                                var constraintName =
                                    inlineConstraintName
                                    ?? DbProviderUtils.GenerateForeignKeyConstraintName(
                                        tableName,
                                        columnName,
                                        referencedTableName,
                                        referenceColumnName
                                    );

                                var foreignKey = new DxForeignKeyConstraint(
                                    null,
                                    tableName,
                                    constraintName,
                                    [new DxOrderedColumn(column.ColumnName)],
                                    referencedTableName,
                                    [new DxOrderedColumn(referenceColumnName)]
                                );

                                var onDeleteTokenIndex = columnDefinition.FindTokenIndex(
                                    "ON DELETE"
                                );
                                if (onDeleteTokenIndex >= i)
                                {
                                    var onDelete = columnDefinition
                                        .GetChild<SqlWordClause>(onDeleteTokenIndex + 1)
                                        ?.Text;
                                    if (!string.IsNullOrWhiteSpace(onDelete))
                                        foreignKey.OnDelete = onDelete.ToForeignKeyAction();
                                }

                                var onUpdateTokenIndex = columnDefinition.FindTokenIndex(
                                    "ON UPDATE"
                                );
                                if (onUpdateTokenIndex >= i)
                                {
                                    var onUpdate = columnDefinition
                                        .GetChild<SqlWordClause>(onUpdateTokenIndex + 1)
                                        ?.Text;
                                    if (!string.IsNullOrWhiteSpace(onUpdate))
                                        foreignKey.OnUpdate = onUpdate.ToForeignKeyAction();
                                }

                                column.ReferencedTableName = foreignKey.ReferencedTableName;
                                column.ReferencedColumnName = foreignKey
                                    .ReferencedColumns[0]
                                    .ColumnName;
                                column.OnDelete = foreignKey.OnDelete;
                                column.OnUpdate = foreignKey.OnUpdate;

                                table.ForeignKeyConstraints.Add(foreignKey);

                                inlineConstraintName = null;
                                break;

                            case "COLLATE":
                                var collation = columnDefinition
                                    .GetChild<SqlWordClause>(i + 1)
                                    ?.ToString();
                                if (!string.IsNullOrWhiteSpace(collation))
                                {
                                    // TODO: not supported at this time
                                    // column.Collation = collation;
                                    // skip the next opt
                                    i++;
                                }
                                break;
                        }
                    }
                }
            }
            else
            {
                // it's a table constraint clause, parse it
                // see: https://www.sqlite.org/syntax/table-constraint.html
                if (clause is not SqlCompoundClause tableConstraint)
                    continue;

                string? inlineConstraintName = null;
                for (var i = 0; i < tableConstraint.Children.Count; i++)
                {
                    var opt = tableConstraint.Children[i];
                    if (opt is SqlWordClause swc)
                    {
                        switch (swc.Text.ToUpper())
                        {
                            case "CONSTRAINT":
                                inlineConstraintName = tableConstraint
                                    .GetChild<SqlWordClause>(i + 1)
                                    ?.Text;
                                // skip the next opt
                                i++;
                                break;
                            case "PRIMARY KEY":
                                var pkColumnsClause = tableConstraint.GetChild<SqlCompoundClause>(
                                    i + 1
                                );

                                var pkOrderedColumns = ExtractOrderedColumnsFromClause(
                                    pkColumnsClause
                                );

                                var pkColumnNames = pkOrderedColumns
                                    .Select(oc => oc.ColumnName)
                                    .ToArray();

                                if (pkColumnNames.Length == 0)
                                    continue; // skip this clause as it's invalid

                                table.PrimaryKeyConstraint = new DxPrimaryKeyConstraint(
                                    null,
                                    tableName,
                                    inlineConstraintName
                                        ?? DbProviderUtils.GeneratePrimaryKeyConstraintName(
                                            tableName,
                                            pkColumnNames
                                        ),
                                    pkOrderedColumns
                                );
                                foreach (var column in table.Columns)
                                {
                                    if (
                                        pkColumnNames.Contains(
                                            column.ColumnName,
                                            StringComparer.OrdinalIgnoreCase
                                        )
                                    )
                                    {
                                        column.IsPrimaryKey = true;
                                    }
                                }
                                continue; // we're done with this clause, so we can move on to the next constraint
                            case "UNIQUE":
                                var ucColumnsClause = tableConstraint.GetChild<SqlCompoundClause>(
                                    i + 1
                                );

                                var ucOrderedColumns = ExtractOrderedColumnsFromClause(
                                    ucColumnsClause
                                );

                                var ucColumnNames = ucOrderedColumns
                                    .Select(oc => oc.ColumnName)
                                    .ToArray();

                                if (ucColumnNames.Length == 0)
                                    continue; // skip this clause as it's invalid

                                var ucConstraint = new DxUniqueConstraint(
                                    null,
                                    tableName,
                                    inlineConstraintName
                                        ?? DbProviderUtils.GenerateUniqueConstraintName(
                                            tableName,
                                            ucColumnNames
                                        ),
                                    ucOrderedColumns
                                );
                                table.UniqueConstraints.Add(ucConstraint);
                                if (ucConstraint.Columns.Length == 1)
                                {
                                    var column = table.Columns.FirstOrDefault(c =>
                                        c.ColumnName.Equals(
                                            ucConstraint.Columns[0].ColumnName,
                                            StringComparison.OrdinalIgnoreCase
                                        )
                                    );
                                    if (column != null)
                                        column.IsUnique = true;
                                }
                                continue; // we're done with this clause, so we can move on to the next constraint
                            case "CHECK":
                                var checkConstraintExpression = tableConstraint
                                    .GetChild<SqlCompoundClause>(i + 1)
                                    ?.ToString()
                                    .Trim('(', ')', ' ');

                                if (!string.IsNullOrWhiteSpace(checkConstraintExpression))
                                {
                                    // add the default constraint to the table
                                    var checkConstraintName =
                                        inlineConstraintName
                                        ?? DbProviderUtils.GenerateCheckConstraintName(
                                            tableName,
                                            table.CheckConstraints.Count > 0
                                                ? $"{table.CheckConstraints.Count}"
                                                : ""
                                        );
                                    table.CheckConstraints.Add(
                                        new DxCheckConstraint(
                                            null,
                                            tableName,
                                            null,
                                            checkConstraintName,
                                            checkConstraintExpression
                                        )
                                    );
                                }
                                continue; // we're done with this clause, so we can move on to the next constraint
                            case "FOREIGN KEY":
                                var fkSourceColumnsClause =
                                    tableConstraint.GetChild<SqlCompoundClause>(i + 1);
                                if (fkSourceColumnsClause == null)
                                    continue; // skip this clause as it's invalid

                                var fkOrderedSourceColumns = ExtractOrderedColumnsFromClause(
                                    fkSourceColumnsClause
                                );
                                var fkSourceColumnNames = fkOrderedSourceColumns
                                    .Select(oc => oc.ColumnName)
                                    .ToArray();
                                if (fkSourceColumnNames.Length == 0)
                                    continue; // skip this clause as it's invalid

                                var referencesClauseIndex = tableConstraint.FindTokenIndex(
                                    "REFERENCES"
                                );
                                if (referencesClauseIndex == -1)
                                    continue; // skip this clause as it's invalid

                                var referencedTableName = tableConstraint
                                    .GetChild<SqlWordClause>(referencesClauseIndex + 1)
                                    ?.Text;
                                var fkReferencedColumnsClause =
                                    tableConstraint.GetChild<SqlCompoundClause>(
                                        referencesClauseIndex + 2
                                    );
                                if (
                                    string.IsNullOrWhiteSpace(referencedTableName)
                                    || fkReferencedColumnsClause == null
                                )
                                    continue; // skip this clause as it's invalid

                                var fkOrderedReferencedColumns = ExtractOrderedColumnsFromClause(
                                    fkReferencedColumnsClause
                                );
                                var fkReferencedColumnNames = fkOrderedReferencedColumns
                                    .Select(oc => oc.ColumnName)
                                    .ToArray();
                                if (fkReferencedColumnNames.Length == 0)
                                    continue; // skip this clause as it's invalid

                                var constraintName =
                                    inlineConstraintName
                                    ?? DbProviderUtils.GenerateForeignKeyConstraintName(
                                        tableName,
                                        fkSourceColumnNames,
                                        referencedTableName,
                                        fkReferencedColumnNames
                                    );

                                var foreignKey = new DxForeignKeyConstraint(
                                    null,
                                    tableName,
                                    constraintName,
                                    fkOrderedSourceColumns,
                                    referencedTableName,
                                    fkOrderedReferencedColumns
                                );

                                var onDeleteTokenIndex = tableConstraint.FindTokenIndex(
                                    "ON DELETE"
                                );
                                if (onDeleteTokenIndex >= i)
                                {
                                    var onDelete = tableConstraint
                                        .GetChild<SqlWordClause>(onDeleteTokenIndex + 1)
                                        ?.Text;
                                    if (!string.IsNullOrWhiteSpace(onDelete))
                                        foreignKey.OnDelete = onDelete.ToForeignKeyAction();
                                }

                                var onUpdateTokenIndex = tableConstraint.FindTokenIndex(
                                    "ON UPDATE"
                                );
                                if (onUpdateTokenIndex >= i)
                                {
                                    var onUpdate = tableConstraint
                                        .GetChild<SqlWordClause>(onUpdateTokenIndex + 1)
                                        ?.Text;
                                    if (!string.IsNullOrWhiteSpace(onUpdate))
                                        foreignKey.OnUpdate = onUpdate.ToForeignKeyAction();
                                }

                                if (
                                    fkSourceColumnNames.Length == 1
                                    && fkReferencedColumnNames.Length == 1
                                )
                                {
                                    var column = table.Columns.FirstOrDefault(c =>
                                        c.ColumnName.Equals(
                                            fkSourceColumnNames[0],
                                            StringComparison.OrdinalIgnoreCase
                                        )
                                    );
                                    if (column != null)
                                    {
                                        column.IsForeignKey = true;
                                        column.ReferencedTableName = foreignKey.ReferencedTableName;
                                        column.ReferencedColumnName = foreignKey
                                            .ReferencedColumns[0]
                                            .ColumnName;
                                        column.OnDelete = foreignKey.OnDelete;
                                        column.OnUpdate = foreignKey.OnUpdate;
                                    }
                                }

                                table.ForeignKeyConstraints.Add(foreignKey);
                                continue; // we're done processing the FOREIGN KEY clause, so we can move on to the next constraint
                        }
                    }
                }
            }
        }

        return table;
    }

    private static DxOrderedColumn[] ExtractOrderedColumnsFromClause(
        SqlCompoundClause? pkColumnsClause
    )
    {
        if (
            pkColumnsClause == null
            || pkColumnsClause.Children.Count == 0
            || pkColumnsClause.Parenthesis == false
        )
            return [];

        var pkOrderedColumns = pkColumnsClause
            .Children.Select(child =>
            {
                switch (child)
                {
                    case SqlWordClause wc:
                        return new DxOrderedColumn(wc.Text);
                    case SqlCompoundClause cc:
                    {
                        var ccName = cc.GetChild<SqlWordClause>(0)?.Text;
                        if (string.IsNullOrWhiteSpace(ccName))
                            return null;
                        var ccOrder = DxColumnOrder.Ascending;
                        if (
                            cc.GetChild<SqlWordClause>(1)
                                ?.Text.Equals("DESC", StringComparison.OrdinalIgnoreCase) == true
                        )
                        {
                            ccOrder = DxColumnOrder.Descending;
                        }
                        return new DxOrderedColumn(ccName, ccOrder);
                    }
                    default:
                        return null;
                }
            })
            .Where(oc => oc != null)
            .Cast<DxOrderedColumn>()
            .ToArray();
        return pkOrderedColumns;
    }
}

[SuppressMessage("ReSharper", "ForCanBeConvertedToForeach")]
[SuppressMessage("ReSharper", "ConvertIfStatementToSwitchStatement")]
[SuppressMessage("ReSharper", "InvertIf")]
[SuppressMessage("ReSharper", "ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator")]
public static partial class SqliteSqlParser
{
    private static List<SqlClause> ParseDdlSql(string sql)
    {
        var statementParts = ParseSqlIntoStatementParts(sql);

        var statements = new List<SqlClause>();
        foreach (var parts in statementParts)
        {
            var clauseBuilder = new ClauseBuilder();
            foreach (var part in parts)
            {
                clauseBuilder.AddPart(part);
            }
            // clauseBuilder.Complete();

            var rootClause = clauseBuilder.GetRootClause();
            rootClause = ClauseBuilder.ReduceNesting(rootClause);
            statements.Add(rootClause);
        }

        return statements;
    }

    private static string StripCommentsFromSql(string sqlQuery)
    {
        // Remove multi-line comments (non-greedy)
        sqlQuery = MultiLineCommentRegex().Replace(sqlQuery, "");

        // Remove single-line comments
        sqlQuery = SingleLineCommentRegex().Replace(sqlQuery, "");

        return sqlQuery;
    }

    [SuppressMessage("ReSharper", "RedundantAssignment")]
    private static List<string[]> ParseSqlIntoStatementParts(string sql)
    {
        sql = StripCommentsFromSql(sql);

        sql = substitute_encode(sql);

        var statements = new List<string[]>();

        var parts = new List<string>();

        // split the SQL into parts
        sql = string.Join(
            ' ',
            sql.Split(
                [' ', '\r', '\n'],
                StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries
            )
        );
        var cpart = string.Empty;
        var inQuotes = false;
        for (var ci = 0; ci < sql.Length; ci++)
        {
            var c = sql[ci];
            if (inQuotes && c != '\"')
            {
                cpart += c;
                continue;
            }
            if (inQuotes && c == '\"')
            {
                cpart += c;
                parts.Add(cpart);
                cpart = string.Empty;
                inQuotes = false;
                continue;
            }
            if (!inQuotes && c == '\"')
            {
                if (!string.IsNullOrWhiteSpace(cpart))
                {
                    parts.Add(cpart);
                    cpart = string.Empty;
                }
                inQuotes = true;
                cpart += c;
                continue;
            }
            // detect end of statement
            if (!inQuotes && c == ';')
            {
                if (parts.Count != 0)
                {
                    statements.Add(substitute_decode(parts).ToArray());
                    parts = [];
                }
                continue;
            }
            if (c.Equals(' '))
            {
                if (!string.IsNullOrWhiteSpace(cpart))
                {
                    parts.Add(cpart);
                    cpart = string.Empty;
                }
                continue;
            }
            if (c.Equals('(') || c.Equals(')') || c.Equals(','))
            {
                if (!string.IsNullOrWhiteSpace(cpart))
                {
                    parts.Add(cpart);
                    cpart = string.Empty;
                }
                parts.Add(c.ToString());
                continue;
            }
            cpart += c;
        }
        if (!string.IsNullOrWhiteSpace(cpart))
        {
            parts.Add(cpart);
            cpart = string.Empty;
        }

        if (parts.Count != 0)
        {
            statements.Add(substitute_decode(parts).ToArray());
            parts = [];
        }

        return statements;
    }

    #region Static Variables

    private static string substitute_encode(string text)
    {
        foreach (var s in Substitutions)
        {
            text = text.Replace(s.Key, s.Value, StringComparison.OrdinalIgnoreCase);
        }
        return text;
    }

    private static List<string> substitute_decode(List<string> strings)
    {
        var parts = new List<string>();
        foreach (var t in strings)
        {
            parts.Add(substitute_decode(t));
        }
        return parts;
    }

    private static string substitute_decode(string text)
    {
        foreach (var s in Substitutions)
        {
            text = text.Replace(s.Value, s.Key, StringComparison.OrdinalIgnoreCase);
        }
        return text;
    }

    /// <summary>
    /// Keep certain words together that belong together while parsing a CREATE TABLE statement
    /// </summary>
    private static readonly Dictionary<string, string> Substitutions = new List<string>
    {
        "FOREIGN KEY",
        "PRIMARY KEY",
        "ON DELETE",
        "ON UPDATE",
        "SET NULL",
        "SET DEFAULT",
        "NO ACTION",
        "NOT NULL",
        "UNSIGNED BIG INT",
        "VARYING CHARACTER",
        "NATIVE CHARACTER",
        "DOUBLE PRECISION"
    }.ToDictionary(x => x, v => v.Replace(' ', '_'));

    /// <summary>
    /// Don't mistake words as identifiers with keywords
    /// </summary>
    public static readonly List<string> Keyword =
    [
        "ABORT",
        "ACTION",
        "ADD",
        "AFTER",
        "ALL",
        "ALTER",
        "ALWAYS",
        "ANALYZE",
        "AND",
        "AS",
        "ASC",
        "ATTACH",
        "AUTOINCREMENT",
        "BEFORE",
        "BEGIN",
        "BETWEEN",
        "BY",
        "CASCADE",
        "CASE",
        "CAST",
        "CHECK",
        "COLLATE",
        "COLUMN",
        "COMMIT",
        "CONFLICT",
        "CONSTRAINT",
        "CREATE",
        "CROSS",
        "CURRENT",
        "CURRENT_DATE",
        "CURRENT_TIME",
        "CURRENT_TIMESTAMP",
        "DATABASE",
        "DEFAULT",
        "DEFERRABLE",
        "DEFERRED",
        "DELETE",
        "DESC",
        "DETACH",
        "DISTINCT",
        "DO",
        "DROP",
        "EACH",
        "ELSE",
        "END",
        "ESCAPE",
        "EXCEPT",
        "EXCLUDE",
        "EXCLUSIVE",
        "EXISTS",
        "EXPLAIN",
        "FAIL",
        "FILTER",
        "FIRST",
        "FOLLOWING",
        "FOR",
        "FOREIGN",
        "FROM",
        "FULL",
        "GENERATED",
        "GLOB",
        "GROUP",
        "GROUPS",
        "HAVING",
        "IF",
        "IGNORE",
        "IMMEDIATE",
        "IN",
        "INDEX",
        "INDEXED",
        "INITIALLY",
        "INNER",
        "INSERT",
        "INSTEAD",
        "INTERSECT",
        "INTO",
        "IS",
        "ISNULL",
        "JOIN",
        "KEY",
        "LAST",
        "LEFT",
        "LIKE",
        "LIMIT",
        "MATCH",
        "MATERIALIZED",
        "NATURAL",
        "NO",
        "NOT",
        "NOTHING",
        "NOTNULL",
        "NULL",
        "NULLS",
        "OF",
        "OFFSET",
        "ON",
        "OR",
        "ORDER",
        "OTHERS",
        "OUTER",
        "OVER",
        "PARTITION",
        "PLAN",
        "PRAGMA",
        "PRECEDING",
        "PRIMARY",
        "QUERY",
        "RAISE",
        "RANGE",
        "RECURSIVE",
        "REFERENCES",
        "REGEXP",
        "REINDEX",
        "RELEASE",
        "RENAME",
        "REPLACE",
        "RESTRICT",
        "RETURNING",
        "RIGHT",
        "ROLLBACK",
        "ROW",
        "ROWS",
        "SAVEPOINT",
        "SELECT",
        "SET",
        "TABLE",
        "TEMP",
        "TEMPORARY",
        "THEN",
        "TIES",
        "TO",
        "TRANSACTION",
        "TRIGGER",
        "UNBOUNDED",
        "UNION",
        "UNIQUE",
        "UPDATE",
        "USING",
        "VACUUM",
        "VALUES",
        "VIEW",
        "VIRTUAL",
        "WHEN",
        "WHERE",
        "WINDOW",
        "WITH",
        "WITHOUT"
    ];
    #endregion // Static Variables

    #region ClauseBuilder Classes

    public abstract class SqlClause(SqlCompoundClause? parent)
    {
        private SqlCompoundClause? _parent = parent;

        public bool HasParent()
        {
            return _parent != null;
        }

        public SqlCompoundClause? GetParent()
        {
            return _parent;
        }

        public void SetParent(SqlCompoundClause clause)
        {
            _parent = clause;
        }

        public int FindTokenIndex(string token)
        {
            if (this is SqlCompoundClause scc)
            {
                return scc.Children.FindIndex(c =>
                    c is SqlWordClause swc
                    && swc.Text.Equals(token, StringComparison.OrdinalIgnoreCase)
                );
            }
            return -1;
        }

        public TClause? GetChild<TClause>(int index)
            where TClause : SqlClause
        {
            if (this is not SqlCompoundClause scc)
                return null;

            if (index >= 0 && index < scc.Children.Count)
                return scc.Children[index] as TClause;

            return null;
        }

        public TClause? GetChild<TClause>(Func<TClause, bool> predicate)
            where TClause : SqlClause
        {
            if (this is not SqlCompoundClause scc)
                return null;

            foreach (var child in scc.Children)
            {
                if (child is TClause tc && predicate(tc))
                    return tc;
            }

            return null;
        }
    }

    public class SqlWordClause : SqlClause
    {
        public SqlWordClause(SqlCompoundClause? parent, string text)
            : base(parent)
        {
            if (text.StartsWith('[') && text.EndsWith(']'))
            {
                Quotes = ['[', ']'];
                Text = text.Trim('[', ']');
            }
            else if (text.StartsWith('\'') && text.EndsWith('\''))
            {
                Quotes = ['\'', '\''];
                Text = text.Trim('\'');
            }
            else if (text.StartsWith('"') && text.EndsWith('"'))
            {
                Quotes = ['"', '"'];
                Text = text.Trim('"');
            }
            else if (text.StartsWith('`') && text.EndsWith('`'))
            {
                Quotes = ['`', '`'];
                Text = text.Trim('`');
            }
            else
            {
                Quotes = null;
                Text = text;
            }
        }

        public string Text { get; set; }

        // ReSharper disable once MemberCanBePrivate.Global
        public char[]? Quotes { get; set; }

        public override string ToString()
        {
            return Quotes is not { Length: 2 } ? Text : $"{Quotes[0]}{Text}{Quotes[1]}";
        }
    }

    public class SqlStatementClause(SqlCompoundClause? parent) : SqlCompoundClause(parent)
    {
        public override string ToString()
        {
            return $"{base.ToString()};";
        }
    }

    public class SqlCompoundClause(SqlCompoundClause? parent) : SqlClause(parent)
    {
        public List<SqlClause> Children { get; set; } = [];
        public bool Parenthesis { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (Parenthesis)
            {
                sb.Append('(');
            }
            var first = true;
            foreach (var child in Children)
            {
                if (!first)
                    sb.Append(Parenthesis ? ", " : " ");
                else
                    first = false;

                sb.Append(child);
            }
            if (Parenthesis)
            {
                sb.Append(')');
            }
            return sb.ToString();
        }
    }

    public class ClauseBuilder
    {
        private readonly SqlCompoundClause _rootClause;
        private SqlCompoundClause _activeClause;

        private readonly List<SqlCompoundClause> _allCompoundClauses = [];

        public ClauseBuilder()
        {
            _rootClause = new SqlStatementClause(null);
            _activeClause = _rootClause;
        }

        public SqlClause GetRootClause()
        {
            return _rootClause;
        }

        public void AddPart(string part)
        {
            if (part == "(")
            {
                // start a new compound clause and add it to the current active clause
                var newClause = new SqlCompoundClause(_activeClause) { Parenthesis = true };
                _allCompoundClauses.Add(newClause);
                _activeClause.Children.Add(newClause);
                // add a compound clause to this clause, and make that the active clause
                var firstChildClause = new SqlCompoundClause(newClause);
                _allCompoundClauses.Add(firstChildClause);
                newClause.Children.Add(firstChildClause);
                // switch the active clause to the new clause
                _activeClause = firstChildClause;
                return;
            }
            if (part == ")")
            {
                // end the existing clause by making the active clause the parent (up 2 levels)
                if (_activeClause.HasParent())
                {
                    _activeClause = _activeClause.GetParent()!;
                    if (_activeClause.HasParent())
                    {
                        _activeClause = _activeClause.GetParent()!;
                    }
                }
                return;
            }
            if (part == ",")
            {
                // start a new clause and add it to the current active clause
                var newClause = new SqlCompoundClause(_activeClause.GetParent());
                _allCompoundClauses.Add(newClause);
                _activeClause.GetParent()!.Children.Add(newClause);
                _activeClause = newClause;
                return;
            }

            _activeClause.Children.Add(new SqlWordClause(_activeClause, part));
        }

        public void Complete()
        {
            foreach (
                var c in _allCompoundClauses /*.Where(x => x.parenthesis)*/
            )
            {
                if (c.Children.Count != 1)
                    continue;

                var child = c.Children[0];
                if (child is not SqlCompoundClause { Parenthesis: false } scc)
                    continue;

                if (scc.Children.Count != 1)
                    continue;

                // reduce indentation, reduce nesting
                var gscc = scc.Children[0];
                gscc.SetParent(c);
                c.Children = [gscc];
            }
        }

        public static SqlClause ReduceNesting(SqlClause clause)
        {
            if (clause is not SqlCompoundClause scc)
                return clause;

            var children = new List<SqlClause>();
            // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
            foreach (var child in scc.Children)
            {
                var reducedChild = ReduceNesting(child);
                children.Add(reducedChild);
            }
            scc.Children = children;

            // reduce nesting
            if (!scc.Parenthesis && children is [SqlWordClause cswc])
            {
                return cswc;
            }

            return scc;
        }
    }

    // Regular expression patterns to match single-line and multi-line comments
    // const string singleLineCommentPattern = @"--.*?$";
    // const string multiLineCommentPattern = @"/\*.*?\*/";

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline)]
    private static partial Regex MultiLineCommentRegex();

    [GeneratedRegex("--.*?$", RegexOptions.Multiline)]
    private static partial Regex SingleLineCommentRegex();

    #endregion // ClauseBuilder Classes
}
