namespace DapperMatic;

public interface IDbTypeConverter<TFrom, TTo>
{
    /// <summary>
    /// Tries to convert an object of type <typeparamref name="TFrom"/> to an object of type <typeparamref name="TTo"/>.
    /// </summary>
    /// <param name="from">The object to convert from.</param>
    /// <param name="to">The converted object, if the conversion was successful.</param>
    /// <returns>True if the conversion was successful; otherwise, false.</returns>
    bool TryConvert(TFrom from, out TTo? to);
}
