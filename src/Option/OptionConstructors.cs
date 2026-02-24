namespace Svan.Monads;

/// <summary>Non-generic factory methods for creating <see cref="Option{T}"/> instances.</summary>
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
    /// Converts any type <c>T</c> to <see cref="Option{T}"/>.
    /// </summary>
    /// <returns>Returns <c>Some</c> for value types.
    /// Returns <c>Some</c> for reference types that are not <c>null</c> and <c>None</c> for reference types that are <c>null</c>.</returns>
    /// <example>
    /// <code>
    /// var some = "hello".ToOption();   // Some("hello")
    /// string? s = null;
    /// var none = s.ToOption();         // None
    /// var intSome = 42.ToOption();     // Some(42) — value types are always Some
    /// </code>
    /// </example>
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