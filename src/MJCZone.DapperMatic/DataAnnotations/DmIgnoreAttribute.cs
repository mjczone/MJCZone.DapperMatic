namespace MJCZone.DapperMatic.DataAnnotations;

/// <summary>
/// Attribute to ignore a property in the mapping.
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class DmIgnoreAttribute : Attribute { }
