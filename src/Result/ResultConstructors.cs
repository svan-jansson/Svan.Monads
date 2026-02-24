namespace Svan.Monads;

/// <summary>Non-generic factory methods for creating <see cref="Result{TError,TSuccess}"/> instances.</summary>
public static class Result
{
    /// <summary>
    /// Creates a <typeparamref name="TSuccess"/> result with the provided value.
    /// </summary>
    public static Result<TError, TSuccess> Success<TError, TSuccess>(TSuccess value) => Result<TError, TSuccess>.Success(value);

    /// <summary>
    /// Creates a <typeparamref name="TError"/> result with the provided value.
    /// </summary>
    public static Result<TError, TSuccess> Error<TError, TSuccess>(TError value) => Result<TError, TSuccess>.Error(value);

    /// <summary>
    /// Creates a <typeparamref name="TSuccess"/> result with the provided value.
    /// </summary>
    /// <example>
    /// <code>
    /// var result = 42.ToSuccess&lt;string, int&gt;(); // Success(42)
    /// </code>
    /// </example>
    public static Result<TError, TSuccess> ToSuccess<TError, TSuccess>(this TSuccess value) => Success<TError, TSuccess>(value);

    /// <summary>
    /// Creates a <typeparamref name="TError"/> result with the provided value.
    /// </summary>
    /// <example>
    /// <code>
    /// var result = "something went wrong".ToError&lt;string, int&gt;(); // Error("something went wrong")
    /// </code>
    /// </example>
    public static Result<TError, TSuccess> ToError<TError, TSuccess>(this TError value) => Error<TError, TSuccess>(value);
}