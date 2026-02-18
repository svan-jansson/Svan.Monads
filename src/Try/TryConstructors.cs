using System;

namespace Svan.Monads;

/// <summary>
/// Factory for creating <c>Try&lt;TSuccess&gt;</c> instances by catching exceptions.
/// </summary>
public static class Try
{
    /// <summary>
    /// Execute <paramref name="codeBlock"/> and return its result as <c>Success</c>.
    /// If the code block throws, the exception is caught and returned as the error state.
    /// </summary>
    public static Try<TSuccess> Catching<TSuccess>(Func<TSuccess> codeBlock)
    {
        try
        {
            return Try<TSuccess>.Success(codeBlock());
        }
        catch (Exception ex)
        {
            return Try<TSuccess>.Exception(ex);
        }
    }

    /// <summary>
    /// Creates a <c>Success</c> try with the provided value.
    /// </summary>
    public static Try<TSuccess> Success<TSuccess>(TSuccess value) => Try<TSuccess>.Success(value);

    /// <summary>
    /// Creates an <c>Exception</c> try with the provided value.
    /// </summary>
    public static Try<TSuccess> Exception<TSuccess>(Exception value) => Try<TSuccess>.Exception(value);

    /// <summary>
    /// Creates a <c>Success</c> try with the provided value.
    /// </summary>
    public static Try<TSuccess> ToSuccess<TSuccess>(this TSuccess value) => Success(value);

    /// <summary>
    /// Creates an <c>Exception</c> try with the provided value.
    /// </summary>
    public static Try<TSuccess> ToException<TSuccess>(this Exception value) => Exception<TSuccess>(value);
}