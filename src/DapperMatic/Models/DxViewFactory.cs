using System.Collections.Concurrent;
using System.Reflection;
using DapperMatic.DataAnnotations;

namespace DapperMatic.Models;

public static class DxViewFactory
{
    private static ConcurrentDictionary<Type, DxView> _cache = new();

    /// <summary>
    /// Returns an instance of a DxView for the given type. If the type is not a valid DxView,
    /// denoted by the use of a DxViewAAttribute on the class, this method returns null.
    /// </summary>
    public static DxView? GetView(Type type)
    {
        if (_cache.TryGetValue(type, out var view))
            return view;

        var viewAttribute = type.GetCustomAttribute<DxViewAttribute>();
        if (viewAttribute == null)
            return null;

        if (string.IsNullOrWhiteSpace(viewAttribute.Definition))
            throw new InvalidOperationException("Type is missing a view definition.");

        view = new DxView(
            string.IsNullOrWhiteSpace(viewAttribute.SchemaName) ? null : viewAttribute.SchemaName,
            string.IsNullOrWhiteSpace(viewAttribute.ViewName) ? type.Name : viewAttribute.ViewName,
            viewAttribute.Definition.Trim()
        );

        _cache.TryAdd(type, view);
        return view;
    }
}
