namespace DapperMatic.Models;

[Serializable]
public enum DxForeignKeyAction
{
    NoAction,
    Cascade,
    Restrict,
    SetNull
}

public static class DxForeignKeyActionExtensions
{
    public static string ToSql(this DxForeignKeyAction foreignKeyAction)
    {
        return foreignKeyAction switch
        {
            DxForeignKeyAction.NoAction => "NO ACTION",
            DxForeignKeyAction.Cascade => "CASCADE",
            DxForeignKeyAction.Restrict => "RESTRICT",
            DxForeignKeyAction.SetNull => "SET NULL",
            _ => "NO ACTION",
        };
    }

    public static DxForeignKeyAction ToForeignKeyAction(this string behavior)
    {
        return (behavior ?? "").ToAlpha().ToUpperInvariant() switch
        {
            "NOACTION" => DxForeignKeyAction.NoAction,
            "CASCADE" => DxForeignKeyAction.Cascade,
            "RESTRICT" => DxForeignKeyAction.Restrict,
            "SETNULL" => DxForeignKeyAction.SetNull,
            _ => DxForeignKeyAction.NoAction,
        };
    }
}
