namespace Svan.Monads;

/// <summary>Non-generic factory methods for creating <see cref="Either{TLeft,TRight}"/> instances.</summary>
public static class Either
{
    /// <summary>
    /// Creates an <c>Either</c> (left) with the provided value.
    /// </summary>
    public static Either<TLeft, TRight> FromLeft<TLeft, TRight>(TLeft value) => Either<TLeft, TRight>.FromLeft(value);
    /// <summary>
    /// Creates an <c>Either</c> (right) with the provided value.
    /// </summary>
    public static Either<TLeft, TRight> FromRight<TLeft, TRight>(TRight value) => Either<TLeft, TRight>.FromRight(value);

    /// <summary>
    /// Creates an <c>Either</c> (left) with the provided value.
    /// </summary>
    /// <example>
    /// <code>
    /// Either&lt;string, int&gt; left = "error".ToLeft&lt;string, int&gt;();
    /// </code>
    /// </example>
    public static Either<TLeft, TRight> ToLeft<TLeft, TRight>(this TLeft value) => FromLeft<TLeft, TRight>(value);
    /// <summary>
    /// Creates an <c>Either</c> (right) with the provided value.
    /// </summary>
    /// <example>
    /// <code>
    /// Either&lt;string, int&gt; right = 42.ToRight&lt;string, int&gt;();
    /// </code>
    /// </example>
    public static Either<TLeft, TRight> ToRight<TLeft, TRight>(this TRight value) => FromRight<TLeft, TRight>(value);
}

