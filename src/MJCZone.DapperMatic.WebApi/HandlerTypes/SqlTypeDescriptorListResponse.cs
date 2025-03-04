namespace MJCZone.DapperMatic.WebApi.HandlerTypes;

/// <summary>
/// Represents a response containing a list of <see cref="SqlTypeDescriptor"/> objects.
/// </summary>
public class SqlTypeDescriptorListResponse
{
    /// <summary>
    /// Gets or sets initializes a new instance of the <see cref="SqlTypeDescriptorListResponse"/> class.
    /// </summary>
    public List<SqlTypeDescriptor>? Results { get; set; }
}
