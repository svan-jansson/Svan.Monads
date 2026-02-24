using Xunit;
using Xunit.Sdk;

namespace Svan.Monads.UnitTests;

public static class TestHelpers
{
    extension<T>(Option<T> option)
    {
        public T AssertSome()
        {
            Assert.True(option.IsSome(), "Expected option to be SOME.");
            return option.Value();
        }

        public T AssertSome(T expected)
        {
            var value = option.AssertSome();
            Assert.Equal(expected, value);
            return value;
        }

        public void AssertNone()
        {
            Assert.True(option.IsNone(), $"Expected option to be NONE, but was SOME");
        }
    }

    extension<TError, TSuccess>(Result<TError, TSuccess> result)
    {
        public TSuccess AssertSuccess()
        {
            Assert.True(result.IsSuccess(), "Expected result to be SUCCESS.");
            return result.SuccessValue();
        }

        public TSuccess AssertSuccess(TSuccess expected)
        {
            var value = result.AssertSuccess();
            Assert.Equal(expected, value);
            return value;
        }

        public TError AssertError()
        {
            Assert.True(result.IsError(), "Expected result to be ERROR");
            return result.ErrorValue();
        }

        public TError AssertError(TError expected)
        {
            var value = result.AssertError();
            Assert.Equal(expected, value);
            return value;
        }
    }
}