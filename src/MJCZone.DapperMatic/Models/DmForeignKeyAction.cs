namespace MJCZone.DapperMatic.Models;

/// <summary>
/// Specifies the action to take on a foreign key constraint.
/// </summary>
[Serializable]
public enum DmForeignKeyAction
{
    /// <summary>
    /// No action will be taken.
    /// </summary>
    NoAction,

    /// <summary>
    /// Delete or update the row from the parent table and automatically delete or update the matching rows in the child table.
    /// </summary>
    Cascade,

    /// <summary>
    /// Reject the delete or update operation for the parent table.
    /// </summary>
    Restrict,

    /// <summary>
    /// Set the foreign key column or columns in the child table to NULL.
    /// </summary>
    SetNull,
}

/// <summary>
/// Provides extension methods for <see cref="DmForeignKeyAction"/>.
/// </summary>
public static class DmForeignKeyActionExtensions
{
    /// <summary>
    /// Converts the foreign key action to its SQL representation.
    /// </summary>
    /// <param name="foreignKeyAction">The foreign key action.</param>
    /// <returns>The SQL representation of the foreign key action.</returns>
    public static string ToSql(this DmForeignKeyAction foreignKeyAction)
    {
        return foreignKeyAction switch
        {
            DmForeignKeyAction.NoAction => "NO ACTION",
            DmForeignKeyAction.Cascade => "CASCADE",
            DmForeignKeyAction.Restrict => "RESTRICT",
            DmForeignKeyAction.SetNull => "SET NULL",
            _ => "NO ACTION"
        };
    }

    /// <summary>
    /// Converts a string to its corresponding <see cref="DmForeignKeyAction"/>.
    /// </summary>
    /// <param name="behavior">The string representation of the foreign key action.</param>
    /// <returns>The corresponding <see cref="DmForeignKeyAction"/>.</returns>
    public static DmForeignKeyAction ToForeignKeyAction(this string behavior)
    {
        return behavior.ToAlpha().ToUpperInvariant() switch
        {
            "NOACTION" => DmForeignKeyAction.NoAction,
            "CASCADE" => DmForeignKeyAction.Cascade,
            "RESTRICT" => DmForeignKeyAction.Restrict,
            "SETNULL" => DmForeignKeyAction.SetNull,
            _ => DmForeignKeyAction.NoAction
        };
    }
}
