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
                .BindCatching<string>(n => Try.Exception<string>(new Exception("binder returned error")));

            Assert.Equal("binder returned error", result.ErrorValue().Message);
        }

        [Fact]
        public void Flatten_returns_inner_try_when_success()
        {
            var nested = Try<Try<int>>.Success(Try<int>.Success(42));
            nested.Flatten().AssertSuccess(42);
        }

        [Fact]
        public void Flatten_returns_inner_exception_when_inner_is_error()
        {
            var inner = Try<int>.Exception(new Exception("inner error"));
            var nested = Try<Try<int>>.Success(inner);
            var result = nested.Flatten();
            Assert.True(result.IsError());
            Assert.Equal("inner error", result.ErrorValue().Message);
        }

        [Fact]
        public void Flatten_returns_outer_exception_when_outer_is_error()
        {
            var nested = Try<Try<int>>.Exception(new Exception("outer error"));
            var result = nested.Flatten();
            Assert.True(result.IsError());
            Assert.Equal("outer error", result.ErrorValue().Message);
        }

        [Fact]
        public void Sequence_returns_all_values_when_all_are_success()
        {
            var tries = new[]
            {
                Try<int>.Success(1),
                Try<int>.Success(2),
                Try<int>.Success(3),
            };

            var sequenced = tries.Sequence();
            Assert.True(sequenced.IsSuccess());
            Assert.Equal(new[] { 1, 2, 3 }, sequenced.SuccessValue());
        }

        [Fact]
        public void Sequence_returns_first_exception_when_any_is_error()
        {
            var tries = new[]
            {
                Try<int>.Success(1),
                Try<int>.Exception(new Exception("first error")),
                Try<int>.Exception(new Exception("second error")),
            };

            var sequenced = tries.Sequence();
            Assert.True(sequenced.IsError());
            Assert.Equal("first error", sequenced.ErrorValue().Message);
        }

        [Fact]
        public void Sequence_returns_success_with_empty_list_when_empty()
        {
            var tries = System.Array.Empty<Try<int>>();

            var sequenced = tries.Sequence();
            Assert.True(sequenced.IsSuccess());
            Assert.Empty(sequenced.SuccessValue());
        }

        [Fact]
        public void Zip_returns_exception_from_other_when_this_is_success_and_other_is_error()
        {
            var success = Try<int>.Success(1);
            var error = Try<int>.Exception(new Exception("other failed"));

            var result = success.Zip(error, (a, b) => a + b);

            Assert.True(result.IsError());
            Assert.Equal("other failed", result.ErrorValue().Message);
        }

        [Fact]
        public void Zip_returns_exception_from_this_when_this_is_error_and_other_is_success()
        {
            var error = Try<int>.Exception(new Exception("this failed"));
            var success = Try<int>.Success(1);

            var result = error.Zip(success, (a, b) => a + b);

            Assert.True(result.IsError());
            Assert.Equal("this failed", result.ErrorValue().Message);
        }

        [Fact]
        public void Zip_returns_success_when_both_are_success()
        {
            var result = Try<int>.Success(3).Zip(Try<int>.Success(4), (a, b) => a + b);

            Assert.Equal(7, result.SuccessValue());
        }

        class ErrorThatIsNotAnExceptionType { }
    }
}