namespace Svan.Monads;

public static class Option
{
    /// <summary>
    /// Creates a <c>Some</c> option with the provided value.
    /// </summary>
    public static Option<T> Some<T>(T value) => new(value);

    /// <summary>
    /// Creates a <c>None</c> option of type <c>T</c>.
    /// </summary>
    public static Option<T> None<T>() => new(new None());

    /// <summary>
    /// Converts any type <c>T</c> to <c>Option&lt;T&gt;</c>. 
    /// </summary>
    /// <returns>Returns <c>Some&lt;T&gt;</c> for value types.
    /// Returns <c>Some&lt;T&gt;</c> for reference types that are not <c>null</c> and <c>None</c> for reference types that are <c>null</c>.</returns>
    public static Option<T> ToOption<T>(this T value)
    {
        Option<T> result;

        if (typeof(T).IsValueType)
        {
            result = Some(value);
        }
        else
        {
            result = value == null
                ? None<T>()
                : Some<T>(value);
        }

        return result;
    }
}