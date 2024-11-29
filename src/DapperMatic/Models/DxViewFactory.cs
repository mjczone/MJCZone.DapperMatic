using System.Collections.Concurrent;
using System.Reflection;
using DapperMatic.DataAnnotations;

namespace DapperMatic.Models;

/// <summary>
/// Factory class for creating and caching instances of <see cref="DxView"/>.
/// </summary>
public static class DxViewFactory
{
    /// <summary>
    /// Cache for storing created <see cref="DxView"/> instances.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, DxView> Cache = new();

    /// <summary>
    /// Returns an instance of a <see cref="DxView"/> for the given type. If the type is not a valid <see cref="DxView"/>,
    /// denoted by the use of a <see cref="DxViewAttribute"/> on the class, this method returns null.
    /// </summary>
    /// <param name="type">The type for which to get the <see cref="DxView"/>.</param>
    /// <returns>An instance of <see cref="DxView"/> if the type is valid; otherwise, null.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the type is missing a view definition.</exception>
    public static DxView? GetView(Type type)
    {
        if (Cache.TryGetValue(type, out var view))
        {
            return view;
        }

        var viewAttribute = type.GetCustomAttribute<DxViewAttribute>();
        if (viewAttribute == null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(viewAttribute.Definition))
        {
            throw new InvalidOperationException("Type is missing a view definition.");
        }

        view = new DxView(
            string.IsNullOrWhiteSpace(viewAttribute.SchemaName) ? null : viewAttribute.SchemaName,
            string.IsNullOrWhiteSpace(viewAttribute.ViewName) ? type.Name : viewAttribute.ViewName,
            viewAttribute.Definition.Trim()
        );

        Cache.TryAdd(type, view);
        return view;
    }
}
