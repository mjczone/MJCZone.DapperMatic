namespace MJCZone.DapperMatic.DataAnnotations;

/// <summary>
/// Attribute to ignore a property in the mapping.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class DmIgnoreAttribute : Attribute { }
