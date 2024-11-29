namespace DapperMatic.Models;

/// <summary>
/// Represents a SQL command with its associated parameters.
/// </summary>
public class DxCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DxCommand"/> class.
    /// </summary>
    public DxCommand() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="DxCommand"/> class with the specified SQL command text and parameters.
    /// </summary>
    /// <param name="sql">The SQL command text.</param>
    /// <param name="parameters">The parameters for the SQL command.</param>
    public DxCommand(string sql, IDictionary<string, object?>? parameters = null)
    {
        Sql = sql;
        Parameters = parameters;
    }

    /// <summary>
    /// Gets or sets the SQL command text.
    /// </summary>
    public string? Sql { get; set; }

    /// <summary>
    /// Gets or sets the parameters for the SQL command.
    /// </summary>
    public IDictionary<string, object?>? Parameters { get; set; }
}
