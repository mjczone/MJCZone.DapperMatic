namespace DapperMatic
{
    /// <summary>
    /// Specifies the type of database provider.
    /// </summary>
    public enum DbProviderType
    {
        /// <summary>
        /// SQLite database provider.
        /// </summary>
        Sqlite,

        /// <summary>
        /// SQL Server database provider.
        /// </summary>
        SqlServer,

        /// <summary>
        /// MySQL database provider.
        /// </summary>
        MySql,

        /// <summary>
        /// PostgreSQL database provider.
        /// </summary>
        PostgreSql,

        /// <summary>
        /// Other database provider.
        /// </summary>
        Other,
    }
}
