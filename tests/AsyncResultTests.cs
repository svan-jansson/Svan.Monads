using Xunit;
using Svan.Monads;

namespace Svan.Monads.UnitTest
{
    public class AsyncResultTests
    {
        // Simulated async services
        private async Task<Result<string, int>> ParseUserId(string input)
        {
            await Task.Yield();
            return int.TryParse(input, out var id)
                ? Result<string, int>.Success(id)
                : Result<string, int>.Error($"'{input}' is not a valid id");
        }

        private async Task<Result<string, string>> LookupUsername(int userId)
        {
            await Task.Yield();
            return userId == 42
                ? Result<string, string>.Success("varmkorv")
                : Result<string, string>.Error($"user {userId} not found");
        }

        private async Task<string> FormatGreeting(string username)
        {
            await Task.Yield();
            return $"Hello, {username}!";
        }

        [Fact]
        public async Task Await_then_map_and_fold()
        {
            var result = (await ParseUserId("42"))
                .Map(id => id * 2)
                .Fold(error => -1, id => id);

            Assert.Equal(84, result);
        }

        [Fact]
        public async Task Await_then_fold_on_error()
        {
            var result = (await ParseUserId("abc"))
                .Fold(
                    error => error,
                    id => "ok");

            Assert.Equal("'abc' is not a valid id", result);
        }

        [Fact]
        public async Task Await_then_default_with()
        {
            var result = (await ParseUserId("abc"))
                .DefaultWith(error => -1);

            Assert.Equal(-1, result);
        }

        [Fact]
        public async Task Sequential_async_calls_with_intermediate_awaits()
        {
            var userId = await ParseUserId("42");
            var username = userId.IsSuccess()
                ? await LookupUsername(userId.SuccessValue())
                : Result<string, string>.Error(userId.ErrorValue());

            Assert.Equal("varmkorv", username.SuccessValue());
        }

        [Fact]
        public async Task Async_bind_enables_fluent_chaining()
        {
            var result = await ParseUserId("42")
                .BindAsync(LookupUsername);

            Assert.Equal("varmkorv", result.SuccessValue());
        }

        [Fact]
        public async Task Async_bind_short_circuits_on_error()
        {
            var result = await ParseUserId("abc")
                .BindAsync(LookupUsername);

            Assert.Equal("'abc' is not a valid id", result.ErrorValue());
        }

        [Fact]
        public async Task Async_bind_propagates_second_error()
        {
            var result = await ParseUserId("99")
                .BindAsync(LookupUsername);

            Assert.Equal("user 99 not found", result.ErrorValue());
        }

        [Fact]
        public async Task Async_map_transforms_value()
        {
            var result = await ParseUserId("42")
                .BindAsync(LookupUsername)
                .MapAsync(FormatGreeting);

            Assert.Equal("Hello, varmkorv!", result.SuccessValue());
        }

        [Fact]
        public async Task Async_chain_then_await_for_sync_operations()
        {
            var greeting = (await ParseUserId("42")
                    .BindAsync(LookupUsername))
                .Map(name => $"Welcome, {name}!")
                .DefaultWith(error => $"Error: {error}");

            Assert.Equal("Welcome, varmkorv!", greeting);
        }
    }
}
