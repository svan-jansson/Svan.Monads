using Xunit;
using Svan.Monads;

namespace Svan.Monads.UnitTest
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

        // ── Already works today: await then chain sync operations ──

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

        // ── With two async extension methods: BindAsync and MapAsync ──

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
    }
}
