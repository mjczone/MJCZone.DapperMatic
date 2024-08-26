namespace DapperMatic.Models;

public enum ReferentialAction
{
    NoAction,
    Cascade,
    SetNull,
}

public static class ReferentialActionExtensions
{
    public static string ToSql(this ReferentialAction referentialAction)
    {
        return referentialAction switch
        {
            ReferentialAction.NoAction => "NO ACTION",
            ReferentialAction.Cascade => "CASCADE",
            ReferentialAction.SetNull => "SET NULL",
            _
                => throw new ArgumentOutOfRangeException(
                    nameof(referentialAction),
                    referentialAction,
                    null
                ),
        };
    }
}
