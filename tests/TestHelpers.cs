using Xunit;
using Xunit.Sdk;

namespace Svan.Monads.UnitTests;

public static class TestHelpers
{
    public static T AssertSome<T>(this Option<T> option)
    {
        Assert.True(option.IsSome(), "Expected option to be SOME.");
        return option.Value();
    }

    public static T AssertSome<T>(this Option<T> option, T expected)
    {
        var value = option.AssertSome();
        Assert.Equal(expected, value);
        return value;
    }

    public static void AssertNone<T>(this Option<T> option)
    {
        Assert.True(option.IsNone(), $"Expected option to be NONE, but was SOME");
    }

    public static TSuccess AssertSuccess<TError, TSuccess>(this Result<TError, TSuccess> result)
    {
        Assert.True(result.IsSuccess(), "Expected result to be SUCCESS.");
        return result.SuccessValue();
    }

    public static TSuccess AssertSuccess<TError, TSuccess>(this Result<TError, TSuccess> result, TSuccess expected)
    {
        var value = result.AssertSuccess();
        Assert.Equal(expected, value);
        return value;
    }

    public static TError AssertError<TError, TSuccess>(this Result<TError, TSuccess> result)
    {
        Assert.True(result.IsError(), "Expected result to be ERROR");
        return result.ErrorValue();
    }

    public static TError AssertError<TError, TSuccess>(this Result<TError, TSuccess> result, TError expected)
    {
        var value = result.AssertError();
        Assert.Equal(expected, value);
        return value;
    }
}