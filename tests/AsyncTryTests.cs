using Xunit;

namespace Svan.Monads.UnitTests
{
    public class AsyncTryTests
    {
        // Simulated async services
        private async Task<Try<int>> ParseNumber(string input)
        {
            await Task.Yield();
            return Try.Catching(() => int.Parse(input));
        }

        private async Task<Try<int>> SafeDivide(int value)
        {
            await Task.Yield();
            return Try.Catching(() => 100 / value);
        }

        private async Task<string> FormatResult(int value)
        {
            await Task.Yield();
            return $"result: {value}";
        }

        [Fact]
        public async Task Await_then_map_and_fold()
        {
            var result = (await ParseNumber("5"))
                .Map(n => n * 2)
                .Fold(error => -1, n => n);

            Assert.Equal(10, result);
        }

        [Fact]
        public async Task Await_then_fold_on_error()
        {
            var result = (await ParseNumber("not a number"))
                .Fold(
                    error => error.Message,
                    n => "ok");

            Assert.Contains("not a number", result);
        }

        [Fact]
        public async Task Await_then_default_with()
        {
            var result = (await ParseNumber("not a number"))
                .DefaultWith(error => -1);

            Assert.Equal(-1, result);
        }

        [Fact]
        public async Task Async_bind_enables_fluent_chaining()
        {
            var result = await ParseNumber("10")
                .BindAsync(SafeDivide);

            Assert.Equal(10, result.SuccessValue());
        }

        [Fact]
        public async Task Async_bind_short_circuits_on_error()
        {
            var result = await ParseNumber("abc")
                .BindAsync(SafeDivide);

            Assert.True(result.IsError());
        }

        [Fact]
        public async Task Async_bind_propagates_second_error()
        {
            var result = await ParseNumber("0")
                .BindAsync(SafeDivide);

            Assert.IsType<DivideByZeroException>(result.ErrorValue());
        }

        [Fact]
        public async Task Async_map_transforms_value()
        {
            var result = await ParseNumber("10")
                .BindAsync(SafeDivide)
                .MapAsync(FormatResult);

            Assert.Equal("result: 10", result.SuccessValue());
        }

        [Fact]
        public async Task Async_chain_then_await_for_sync_operations()
        {
            var message = (await ParseNumber("10")
                    .BindAsync(SafeDivide))
                .Map(n => $"computed: {n}")
                .DefaultWith(error => $"failed: {error.Message}");

            Assert.Equal("computed: 10", message);
        }
 
        [Fact]
        public async Task Sequence_enables_sync_map_with_async_function()
        {
            // When starting from a sync Try, Map with an async function
            // produces Try<Task<T>>. Sequence flips it so we can await.
            Try<int> value = Try.Catching(() => 10);

            var result = await value
                .Map(FormatResult)
                .Sequence();

            Assert.Equal("result: 10", result.SuccessValue());
        }

        [Fact]
        public async Task Sequence_skips_the_async_work_on_error()
        {
            Try<int> value = Try.Catching<int>(() => throw new InvalidOperationException("boom"));

            var result = await value
                .Map(FormatResult)
                .Sequence();

            Assert.Equal("boom", result.ErrorValue().Message);
        }

        [Fact]
        public async Task Sequence_catches_faulted_inner_task()
        {
            Try<Task<string>> value = Try.Catching<Task<string>>(
                () => Task.FromException<string>(new InvalidOperationException("faulted")));

            var result = await value.Sequence();

            Assert.True(result.IsError());
            Assert.Equal("faulted", result.ErrorValue().Message);
        }

        [Fact]
        public void BindCatching_catches_exception_from_binder()
        {
            Try<int> value = Try.Catching(() => 10);

            var result = value
                .BindCatching<string>(n => throw new InvalidOperationException("binder failed"));

            Assert.Equal("binder failed", result.ErrorValue().Message);
        }

        [Fact]
        public async Task BindCatchingAsync_catches_exception_from_async_binder()
        {
            var result = await ParseNumber("10")
                .BindCatchingAsync<int, string>(async n =>
                {
                    await Task.Yield();
                    throw new InvalidOperationException("async binder failed");
                });

            Assert.Equal("async binder failed", result.ErrorValue().Message);
        }

        [Fact]
        public async Task BindCatchingAsync_catches_faulted_task()
        {
            var result = await ParseNumber("10")
                .BindCatchingAsync<int, string>(n =>
                    Task.FromException<Try<string>>(new InvalidOperationException("faulted")));

            Assert.Equal("faulted", result.ErrorValue().Message);
        }

        [Fact]
        public async Task BindCatchingAsync_short_circuits_on_error()
        {
            var result = await ParseNumber("abc")
                .BindCatchingAsync(SafeDivide);

            Assert.True(result.IsError());
        }

        [Fact]
        public async Task MapCatchingAsync_catches_exception_from_async_mapper()
        {
            var result = await ParseNumber("10")
                .MapCatchingAsync<int, string>(async n =>
                {
                    await Task.Yield();
                    throw new InvalidOperationException("async mapper failed");
                });

            Assert.Equal("async mapper failed", result.ErrorValue().Message);
        }

        [Fact]
        public async Task MapCatchingAsync_catches_faulted_task()
        {
            var result = await ParseNumber("10")
                .MapCatchingAsync<int, string>(n =>
                    Task.FromException<string>(new InvalidOperationException("faulted")));

            Assert.Equal("faulted", result.ErrorValue().Message);
        }

        [Fact]
        public async Task MapCatchingAsync_returns_value_on_success()
        {
            var result = await ParseNumber("10")
                .MapCatchingAsync(FormatResult);

            Assert.Equal("result: 10", result.SuccessValue());
        }

        [Fact]
        public async Task MapCatchingAsync_short_circuits_on_error()
        {
            var result = await ParseNumber("abc")
                .MapCatchingAsync(FormatResult);

            Assert.True(result.IsError());
        }
    }
}
