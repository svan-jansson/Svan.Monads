using Xunit;

namespace Svan.Monads.UnitTests
{
    public class AsyncOptionTests
    {
        private async Task<Option<int>> FindUserId(string username)
        {
            await Task.Yield();
            return username == "varmkorv" ? Option<int>.Some(42) : Option<int>.None();
        }

        private async Task<Option<string>> FindUserEmail(int userId)
        {
            await Task.Yield();
            return userId == 42
                ? Option<string>.Some("varmkorv@example.com")
                : Option<string>.None();
        }

        private async Task<string> NormalizeEmail(string email)
        {
            await Task.Yield();
            return email.Trim().ToLowerInvariant();
        }

        [Fact]
        public async Task Await_then_map_filter_and_fold()
        {
            var result = (await FindUserId("varmkorv"))
                .Map(id => id * 2)
                .Filter(id => id > 0)
                .Fold(() => -1, id => id);

            Assert.Equal(84, result);
        }

        [Fact]
        public async Task Await_then_fold_on_none()
        {
            var result = (await FindUserId("unknown"))
                .Fold(
                    () => "not found",
                    id => $"found: {id}");

            Assert.Equal("not found", result);
        }

        [Fact]
        public async Task Await_then_default_with()
        {
            var result = (await FindUserId("unknown"))
                .DefaultWith(() => -1);

            Assert.Equal(-1, result);
        }

        [Fact]
        public async Task Sequential_async_calls_with_intermediate_awaits()
        {
            // Works, but requires manual short-circuit logic at each step
            var userId = await FindUserId("varmkorv");
            var email = userId.IsSome()
                ? await FindUserEmail(userId.Value())
                : Option<string>.None();

            Assert.Equal("varmkorv@example.com", email.Value());
        }

        [Fact]
        public async Task Async_bind_enables_fluent_chaining()
        {
            var result = await FindUserId("varmkorv")
                .BindAsync(FindUserEmail);

            Assert.Equal("varmkorv@example.com", result.Value());
        }

        [Fact]
        public async Task Async_bind_short_circuits_on_none()
        {
            var result = await FindUserId("unknown")
                .BindAsync(FindUserEmail);

            Assert.True(result.IsNone());
        }

        [Fact]
        public async Task Async_map_transforms_value()
        {
            var result = await FindUserId("varmkorv")
                .BindAsync(FindUserEmail)
                .MapAsync(NormalizeEmail);

            Assert.Equal("varmkorv@example.com", result.Value());
        }

        [Fact]
        public async Task Async_chain_then_await_for_sync_operations()
        {
            // Async extensions for async callbacks, then await to
            // use the full sync API for everything else
            var greeting = (await FindUserId("varmkorv")
                    .BindAsync(FindUserEmail))
                .Map(email => $"Welcome, {email}!")
                .DefaultWith(() => "Welcome, guest!");

            Assert.Equal("Welcome, varmkorv@example.com!", greeting);
        }

        [Fact]
        public async Task Sequence_enables_sync_map_with_async_function()
        {
            // When starting from a sync Option, Map with an async function
            // produces Option<Task<T>>. Sequence flips it so we can await.
            Option<string> email = Option<string>.Some("  VARMKORV@EXAMPLE.COM  ");

            var result = await email
                .Map(NormalizeEmail)
                .Sequence();

            Assert.Equal("varmkorv@example.com", result.Value());
        }

        [Fact]
        public async Task Sequence_skips_the_async_work_on_none()
        {
            Option<string> email = Option<string>.None();

            var result = await email
                .Map(NormalizeEmail)
                .Sequence();

            Assert.True(result.IsNone());
        }

        [Fact]
        public async Task Sequence_propagates_faulted_inner_task()
        {
            var option = Option<Task<string>>.Some(
                Task.FromException<string>(new InvalidOperationException("faulted")));

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => option.Sequence());

            Assert.Equal("faulted", ex.Message);
        }
    }
}
