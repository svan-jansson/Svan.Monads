namespace Svan.Monads;

public static class Result
{
    /// <summary>
    /// Creates a <c>Success</c> result with the provided value.
    /// </summary>
    public static Result<TError, TSuccess> Success<TError, TSuccess>(TSuccess value) => Result<TError, TSuccess>.Success(value);

    /// <summary>
    /// Creates an <c>Error</c> result with the provided value.
    /// </summary>
    public static Result<TError, TSuccess> Error<TError, TSuccess>(TError value) => Result<TError, TSuccess>.Error(value);

    /// <summary>
    /// Creates a <c>Success</c> result with the provided value.
    /// </summary>
    public static Result<TError, TSuccess> ToSuccess<TError, TSuccess>(this TSuccess value) => Success<TError, TSuccess>(value);

    /// <summary>
    /// Creates an <c>Error</c> result with the provided value.
    /// </summary>
    public static Result<TError, TSuccess> ToError<TError, TSuccess>(this TError value) => Error<TError, TSuccess>(value);
}