using Xunit;

namespace Svan.Monads.UnitTests
{
    public class TryTests
    {
        [Fact]
        public void Try_can_wrap_caught_exceptions()
        {
            Try.Catching<string>(() =>
                {
                    throw new Exception("Error that should be caught");
                })
                .DoIfError((actual) => Assert.IsType<Exception>(actual))
                .Do((_) => Assert.False(true));
        }

        [Fact]
        public void Try_can_be_casted_to_result()
        {
            Try.Catching(() => "a string")
                .DoIfError((_) => Assert.True(false))
                .Do((actual) => Assert.Equal("a string", actual))
                .MapCatching<string>((value) => throw new ArgumentException("This failed"))
                .DoIfError((exception) => Assert.IsType<ArgumentException>(exception))
                .MapError((exception) =>
                {
                    Assert.Equal("This failed", exception.Message);
                    return new ErrorThatIsNotAnExceptionType();
                })
                .Do((value) => Assert.True(false));
        }

        [Fact]
        public void BindCatching_catches_exception_from_binder()
        {
            var result = Try.Catching(() => 42)
                .BindCatching<string>(n => throw new ArgumentException("binder threw"));

            Assert.Equal("binder threw", result.ErrorValue().Message);
        }

        [Fact]
        public void BindCatching_returns_value_on_success()
        {
            var result = Try.Catching(() => 42)
                .BindCatching(n => Try.Catching(() => $"value: {n}"));

            Assert.Equal("value: 42", result.SuccessValue());
        }

        [Fact]
        public void BindCatching_short_circuits_on_error()
        {
            var executed = false;

            var result = Try.Catching<int>(() => throw new Exception("initial error"))
                .BindCatching(n =>
                {
                    executed = true;
                    return Try.Catching(() => n * 2);
                });

            Assert.False(executed);
            Assert.Equal("initial error", result.ErrorValue().Message);
        }

        [Fact]
        public void BindCatching_propagates_error_from_binder_result()
        {
            var result = Try.Catching(() => 42)
                .BindCatching<string>(n => new Exception("binder returned error"));

            Assert.Equal("binder returned error", result.ErrorValue().Message);
        }

        class ErrorThatIsNotAnExceptionType { }
    }
}