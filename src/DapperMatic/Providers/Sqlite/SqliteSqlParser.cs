using System.Text;
using System.Text.RegularExpressions;
using DapperMatic.Models;

namespace DapperMatic.Providers.Sqlite;

public static partial class SqliteSqlParser
{
    public static Type GetDotnetTypeFromSqlType(string sqlType)
    {
        var simpleSqlType = sqlType.Split('(')[0].ToLower();

        var match = DataTypeMapFactory
            .GetDefaultDbProviderDataTypeMap(DbProviderType.Sqlite)
            .FirstOrDefault(x =>
                x.SqlType.Equals(simpleSqlType, StringComparison.OrdinalIgnoreCase)
            )
            ?.DotnetType;

        if (match != null)
            return match;

        // SQLite specific types, see https://www.sqlite.org/datatype3.html
        switch (simpleSqlType)
        {
            case "int":
            case "integer":
            case "mediumint":
            case "int2":
            case "int8":
                return typeof(int);
            case "tinyint":
            case "smallint":
                return typeof(short);
            case "bigint":
            case "unsigned big int":
                return typeof(long);
            case "character":
            case "varchar":
            case "varying character":
            case "nchar":
            case "native character":
            case "nvarchar":
            case "text":
            case "clob":
                return typeof(string);
            case "blob":
                return typeof(byte[]);
            case "real":
            case "double":
                return typeof(double);
            case "float":
            case "double precision":
            case "numeric":
            case "decimal":
                return typeof(decimal);
            case "date":
            case "datetime":
                return typeof(DateTime);
            case "boolean":
            case "bool":
                return typeof(bool);
            default:
                // If no match, default to object
                return typeof(object);
        }
    }

    public static DxTable? ParseCreateTableStatement(string createTableSql)
    {
        var statements = ParseDdlSql(createTableSql);
        var createTableStatement = statements.SingleOrDefault() as SqlCompoundClause;
        if (
            createTableStatement == null
            || createTableStatement.FindTokenIndex("CREATE") != 0
                && createTableStatement.FindTokenIndex("TABLE") != 1
        )
            return null;

        var tableName = createTableStatement.GetChild<SqlWordClause>(2)?.text;
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
            x.children.Count > 0 && x.parenthesis == true
        );
        if (tableGuts == null || tableGuts.children.Count == 0)
            return table;

        // we now iterate over these guts to parse out columns, primary keys, unique constraints, check constraints, default constraints, and foreign key constraints
        // constraint clauses can appear as part of the column definition, or as separate clauses:
        //  - if as part of column definition, they appear inline
        //  - if separate as table constraint definitions, they always start with either the word "CONSTRAINT" or the constraint type identifier "PRIMARY KEY", "FOREIGN KEY", "UNIQUE", "CHECK", "DEFAULT"

        Func<SqlClause, bool> isColumnDefinitionClause = (SqlClause clause) =>
        {
            return !(
                clause.FindTokenIndex("CONSTRAINT") == 0
                || clause.FindTokenIndex("PRIMARY KEY") == 0
                || clause.FindTokenIndex("FOREIGN KEY") == 0
                || clause.FindTokenIndex("UNIQUE") == 0
                || clause.FindTokenIndex("CHECK") == 0
                || clause.FindTokenIndex("DEFAULT") == 0
            );
        };

        // based on the documentation of the CREATE TABLE statement, we know that column definitions appear before table constraint clauses,
        // so we can safely assume that by the time we start parsing constraints, all the column definitions will have been added to the table.columns list
        for (var clauseIndex = 0; clauseIndex < tableGuts.children.Count; clauseIndex++)
        {
            var clause = tableGuts.children[clauseIndex];
            // see if it's a column definition or a table constraint
            if (isColumnDefinitionClause(clause))
            {
                // it's a column definition, parse it
                // see:https://www.sqlite.org/syntax/column-def.html
                if (clause is not SqlCompoundClause columnDefinition)
                    continue;

                // first word in the column name
                var columnName = columnDefinition.GetChild<SqlWordClause>(0)?.text;
                if (string.IsNullOrWhiteSpace(columnName))
                    continue;

                // second word is the column type
                var columnDataType = columnDefinition.GetChild<SqlWordClause>(1)?.text;
                if (string.IsNullOrWhiteSpace(columnDataType))
                    continue;

                int? length = null;
                int? precision = null;
                int? scale = null;

                var remainingWordsIndex = 2;
                if (columnDefinition.children!.Count > 2)
                {
                    var thirdChild = columnDefinition.GetChild<SqlCompoundClause>(2);
                    if (
                        thirdChild != null
                        && thirdChild.children.Count > 0
                        && thirdChild.children.Count <= 2
                    )
                    {
                        if (thirdChild.children.Count == 1)
                        {
                            if (
                                thirdChild.children[0] is SqlWordClause sw1
                                && int.TryParse(sw1.text, out var intValue)
                            )
                            {
                                length = intValue;
                            }
                        }
                        if (thirdChild.children.Count == 2)
                        {
                            if (
                                thirdChild.children[0] is SqlWordClause sw1
                                && int.TryParse(sw1.text, out var intValue)
                            )
                            {
                                precision = intValue;
                            }
                            if (
                                thirdChild.children[1] is SqlWordClause sw2
                                && int.TryParse(sw2.text, out var intValue2)
                            )
                            {
                                scale = intValue2;
                            }
                        }
                        remainingWordsIndex = 3;
                    }
                }

                var column = new DxColumn(
                    null,
                    tableName,
                    columnName,
                    GetDotnetTypeFromSqlType(columnDataType),
                    columnDataType,
                    length,
                    precision,
                    scale
                );
                table.Columns.Add(column);

                // remaining words are optional in the column definition
                if (columnDefinition.children!.Count > remainingWordsIndex)
                {
                    string? inlineConstraintName = null;
                    for (var i = remainingWordsIndex; i < columnDefinition.children.Count; i++)
                    {
                        var opt = columnDefinition.children[i];
                        if (opt is SqlWordClause swc)
                        {
                            switch (swc.text.ToUpper())
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
                                        ?.text;
                                    // skip the next opt
                                    i++;
                                    break;

                                case "DEFAULT":
                                    // the clause can be a compound clause, or literal-value (quoted), or a number (integer, float, etc.)
                                    // if the clause is a compound parenthesized clause, we will remove the parentheses and trim the text
                                    column.DefaultExpression = columnDefinition
                                        .GetChild<SqlClause>(i + 1)
                                        ?.ToString()
                                        ?.Trim(['(', ')', ' ']);
                                    // skip the next opt
                                    i++;
                                    if (!string.IsNullOrWhiteSpace(column.DefaultExpression))
                                    {
                                        // add the default constraint to the table
                                        var defaultConstraintName =
                                            inlineConstraintName
                                            ?? ProviderUtils.GenerateDefaultConstraintName(
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
                                        ?? ProviderUtils.GenerateUniqueConstraintName(
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
                                        ?.Trim(['(', ')', ' ']);
                                    // skip the next opt
                                    i++;
                                    if (!string.IsNullOrWhiteSpace(column.CheckExpression))
                                    {
                                        // add the default constraint to the table
                                        var checkConstraintName =
                                            inlineConstraintName
                                            ?? ProviderUtils.GenerateCheckConstraintName(
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
                                        ?? ProviderUtils.GeneratePrimaryKeyConstraintName(
                                            tableName,
                                            columnName
                                        );
                                    var columnOrder = DxColumnOrder.Ascending;
                                    if (
                                        columnDefinition
                                            .GetChild<SqlClause>(i + 1)
                                            ?.ToString()
                                            ?.Equals("DESC", StringComparison.OrdinalIgnoreCase)
                                        == true
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
                                        ?.text;
                                    if (string.IsNullOrWhiteSpace(referencedTableName))
                                        break;

                                    // skip next opt
                                    i++;

                                    // TODO: sqlite doesn't require the referenced column name, but we will for now in our library
                                    var referenceColumnName = columnDefinition
                                        .GetChild<SqlCompoundClause>(referenceColumnNamesIndex)
                                        ?.GetChild<SqlWordClause>(0)
                                        ?.text;
                                    if (string.IsNullOrWhiteSpace(referenceColumnName))
                                        break;

                                    // skip next opt
                                    i++;

                                    var constraintName =
                                        inlineConstraintName
                                        ?? ProviderUtils.GenerateForeignKeyConstraintName(
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
                                            ?.text;
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
                                            ?.text;
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
            }
            else
            {
                // it's a table constraint clause, parse it
                // see: https://www.sqlite.org/syntax/table-constraint.html
                if (clause is not SqlCompoundClause tableConstraint)
                    continue;

                string? inlineConstraintName = null;
                for (var i = 0; i < tableConstraint.children.Count; i++)
                {
                    var opt = tableConstraint.children[i];
                    if (opt is SqlWordClause swc)
                    {
                        switch (swc.text.ToUpper())
                        {
                            case "CONSTRAINT":
                                inlineConstraintName = tableConstraint
                                    .GetChild<SqlWordClause>(i + 1)
                                    ?.text;
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
                                        ?? ProviderUtils.GeneratePrimaryKeyConstraintName(
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
                                        ?? ProviderUtils.GenerateUniqueConstraintName(
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
                                    ?.Trim(['(', ')', ' ']);

                                if (!string.IsNullOrWhiteSpace(checkConstraintExpression))
                                {
                                    // add the default constraint to the table
                                    var checkConstraintName =
                                        inlineConstraintName
                                        ?? ProviderUtils.GenerateCheckConstraintName(
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
                                    ?.text;
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
                                    ?? ProviderUtils.GenerateForeignKeyConstraintName(
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
                                        ?.text;
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
                                        ?.text;
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
            || pkColumnsClause.children.Count == 0
            || pkColumnsClause.parenthesis == false
        )
            return Array.Empty<DxOrderedColumn>();

        var pkOrderedColumns = pkColumnsClause
            .children.Select(child =>
            {
                if (child is SqlWordClause wc)
                {
                    return new DxOrderedColumn(wc.text, DxColumnOrder.Ascending);
                }
                if (child is SqlCompoundClause cc)
                {
                    var ccName = cc.GetChild<SqlWordClause>(0)?.text;
                    if (string.IsNullOrWhiteSpace(ccName))
                        return null;
                    var ccOrder = DxColumnOrder.Ascending;
                    if (
                        cc.GetChild<SqlWordClause>(1)
                            ?.text?.Equals("DESC", StringComparison.OrdinalIgnoreCase) == true
                    )
                    {
                        ccOrder = DxColumnOrder.Descending;
                    }
                    return new DxOrderedColumn(ccName, ccOrder);
                }
                return null;
            })
            .Where(oc => oc != null)
            .Cast<DxOrderedColumn>()
            .ToArray();
        return pkOrderedColumns;
    }
}

public static partial class SqliteSqlParser
{
    public static List<SqlClause> ParseDdlSql(string sql)
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
            if (rootClause != null)
                rootClause = clauseBuilder.ReduceNesting(rootClause);
            if (rootClause != null)
                statements.Add(rootClause);
        }

        return statements;
    }

    private static string StripCommentsFromSql(string sqlQuery)
    {
        // Regular expression patterns to match single-line and multi-line comments
        string singleLineCommentPattern = @"--.*?$";
        string multiLineCommentPattern = @"/\*.*?\*/";

        // Remove multi-line comments (non-greedy)
        sqlQuery = Regex.Replace(sqlQuery, multiLineCommentPattern, "", RegexOptions.Singleline);

        // Remove single-line comments
        sqlQuery = Regex.Replace(sqlQuery, singleLineCommentPattern, "", RegexOptions.Multiline);

        return sqlQuery;
    }

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
                new char[] { ' ', '\r', '\n' },
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
                if (parts.Any())
                {
                    statements.Add(substitute_decode(parts).ToArray());
                    parts = new List<string>();
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

        if (parts.Any())
        {
            statements.Add(substitute_decode(parts).ToArray());
            parts = new List<string>();
        }

        return statements;
    }

    #region Static Variables

    private static string substitute_encode(string text)
    {
        foreach (var s in substitutions)
        {
            text = text.Replace(s.Key, s.Value, StringComparison.OrdinalIgnoreCase);
        }
        return text;
    }

    private static List<string> substitute_decode(List<string> strings)
    {
        var parts = new List<string>();
        for (var pi = 0; pi < strings.Count; pi++)
        {
            parts.Add(substitute_decode(strings[pi]));
        }
        return parts;
    }

    private static string substitute_decode(string text)
    {
        foreach (var s in substitutions)
        {
            text = text.Replace(s.Value, s.Key, StringComparison.OrdinalIgnoreCase);
        }
        return text;
    }

    /// <summary>
    /// Keep certain words together that belong together while parsing a CREATE TABLE statement
    /// </summary>
    private static readonly Dictionary<string, string> substitutions = new List<string>
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
    public static readonly List<string> keyword =
        new()
        {
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
        };
    #endregion // Static Variables

    #region ClauseBuilder Classes

    public abstract class SqlClause
    {
        private SqlCompoundClause? parent;

        public SqlClause(SqlCompoundClause? parent)
        {
            this.parent = parent;
        }

        public bool HasParent()
        {
            return parent != null;
        }

        public SqlCompoundClause? GetParent()
        {
            return this.parent;
        }

        public void SetParent(SqlCompoundClause clause)
        {
            this.parent = clause;
        }

        public int FindTokenIndex(string token)
        {
            if (this is SqlCompoundClause scc)
            {
                if (scc.children != null)
                    return scc.children.FindIndex(c =>
                        c is SqlWordClause swc
                        && swc.text.Equals(token, StringComparison.OrdinalIgnoreCase)
                    );
            }
            return -1;
        }

        public TClause? GetChild<TClause>(int index)
            where TClause : SqlClause
        {
            if (this is SqlCompoundClause scc)
            {
                if (scc.children != null && index >= 0 && index < scc.children.Count)
                    return scc.children[index] as TClause;
            }
            return null;
        }

        public TClause? GetChild<TClause>(Func<TClause, bool> predicate)
            where TClause : SqlClause
        {
            if (this is SqlCompoundClause scc)
            {
                foreach (var child in scc.children)
                {
                    if (child is TClause tc && predicate(tc))
                        return tc;
                }
            }
            return null;
        }
    }

    public class SqlWordClause : SqlClause
    {
        private string _rawtext = string.Empty;

        public SqlWordClause(SqlCompoundClause? parent, string text)
            : base(parent)
        {
            _rawtext = text;
            if (text.StartsWith('[') && text.EndsWith(']'))
            {
                quotes = new[] { '[', ']' };
                this.text = text.Trim('[', ']');
            }
            else if (text.StartsWith('\'') && text.EndsWith('\''))
            {
                quotes = new[] { '\'', '\'' };
                this.text = text.Trim('\'');
            }
            else if (text.StartsWith('"') && text.EndsWith('"'))
            {
                quotes = new[] { '"', '"' };
                this.text = text.Trim('"');
            }
            else if (text.StartsWith('`') && text.EndsWith('`'))
            {
                quotes = new[] { '`', '`' };
                this.text = text.Trim('`');
            }
            else
            {
                quotes = null;
                this.text = text;
            }
        }

        public string text { get; set; } = string.Empty;
        public char[]? quotes { get; set; }

        public override string ToString()
        {
            return (quotes == null || quotes.Length != 2)
                ? this.text
                : $"{quotes[0]}{this.text}{quotes[1]}";
        }
    }

    public class SqlStatementClause : SqlCompoundClause
    {
        public SqlStatementClause(SqlCompoundClause? parent)
            : base(parent) { }

        public override string ToString()
        {
            return $"{base.ToString()};";
        }
    }

    public class SqlCompoundClause : SqlClause
    {
        public SqlCompoundClause(SqlCompoundClause? parent)
            : base(parent) { }

        public List<SqlClause> children { get; set; } = new();
        public bool parenthesis { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (parenthesis)
            {
                sb.Append("(");
            }
            var first = true;
            foreach (var child in children)
            {
                if (!first)
                    sb.Append(parenthesis ? ", " : " ");
                else
                    first = false;

                sb.Append(child.ToString());
            }
            if (parenthesis)
            {
                sb.Append(")");
            }
            return sb.ToString();
        }
    }

    public class ClauseBuilder
    {
        private SqlCompoundClause rootClause;
        private SqlCompoundClause activeClause;

        private List<SqlCompoundClause> allCompoundClauses = new List<SqlCompoundClause>();

        public ClauseBuilder()
        {
            rootClause = new SqlStatementClause(null);
            activeClause = rootClause;
        }

        public SqlClause GetRootClause()
        {
            return rootClause;
        }

        public void AddPart(string part)
        {
            if (part == "(")
            {
                // start a new compound clause and add it to the current active clause
                var newClause = new SqlCompoundClause(activeClause) { parenthesis = true };
                allCompoundClauses.Add(newClause);
                activeClause.children.Add(newClause);
                // add a compound clause to this clause, and make that the active clause
                var firstChildClause = new SqlCompoundClause(newClause);
                allCompoundClauses.Add(firstChildClause);
                newClause.children.Add(firstChildClause);
                // switch the active clause to the new clause
                activeClause = firstChildClause;
                return;
            }
            if (part == ")")
            {
                // end the existing clause by making the active clause the parent (up 2 levels)
                if (activeClause.HasParent())
                {
                    activeClause = activeClause.GetParent()!;
                    if (activeClause.HasParent())
                    {
                        activeClause = activeClause.GetParent()!;
                    }
                }
                return;
            }
            if (part == ",")
            {
                // start a new clause and add it to the current active clause
                var newClause = new SqlCompoundClause(activeClause.GetParent());
                allCompoundClauses.Add(newClause);
                activeClause.GetParent()!.children.Add(newClause);
                activeClause = newClause;
                return;
            }

            activeClause.children.Add(new SqlWordClause(activeClause, part));
        }

        public void Complete()
        {
            foreach (
                var c in allCompoundClauses /*.Where(x => x.parenthesis)*/
            )
            {
                if (c.children.Count == 1)
                {
                    var child = c.children[0];
                    if (child is SqlCompoundClause scc && scc.parenthesis == false)
                    {
                        if (scc.children.Count == 1)
                        {
                            // reduce indentation, reduce nesting
                            var gscc = scc.children[0];
                            gscc.SetParent(c);
                            c.children = new List<SqlClause> { gscc };
                        }
                    }
                }
            }
        }

        public SqlClause ReduceNesting(SqlClause clause)
        {
            var currentClause = clause;
            if (currentClause is SqlCompoundClause scc)
            {
                var children = new List<SqlClause>();
                foreach (var child in scc.children)
                {
                    var reducedChild = ReduceNesting(child);
                    children.Add(reducedChild);
                }
                scc.children = children;

                // reduce nesting
                if (!scc.parenthesis && children.Count == 1 && children[0] is SqlWordClause cswc)
                {
                    return cswc;
                }

                return scc;
            }

            return currentClause;
        }
    }

    #endregion // ClauseBuilder Classes
}
