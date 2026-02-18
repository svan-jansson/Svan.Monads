using Xunit;

namespace Svan.Monads.UnitTests
{
    public class AsyncEitherTests
    {
        // Simulated async services
        private async Task<Either<string, int>> ParseId(string input)
        {
            await Task.Yield();
            return int.TryParse(input, out var id)
                ? Either<string, int>.FromRight(id)
                : Either<string, int>.FromLeft($"'{input}' is not a valid id");
        }

        private async Task<Either<string, string>> LookupName(int id)
        {
            await Task.Yield();
            return id == 42
                ? Either<string, string>.FromRight("varmkorv")
                : Either<string, string>.FromLeft($"id {id} not found");
        }

        private async Task<string> FormatGreeting(string name)
        {
            await Task.Yield();
            return $"Hello, {name}!";
        }

        [Fact]
        public async Task Await_then_map_and_fold()
        {
            var result = (await ParseId("42"))
                .Map(id => id * 2)
                .Fold(left => -1, right => right);

            Assert.Equal(84, result);
        }

        [Fact]
        public async Task Await_then_fold_on_left()
        {
            var result = (await ParseId("abc"))
                .Fold(
                    left => left,
                    right => "ok");

            Assert.Equal("'abc' is not a valid id", result);
        }

        [Fact]
        public async Task Await_then_default_with()
        {
            var result = (await ParseId("abc"))
                .DefaultWith(left => -1);

            Assert.Equal(-1, result);
        }

        [Fact]
        public async Task Async_bind_enables_fluent_chaining()
        {
            var result = await ParseId("42")
                .BindAsync(LookupName);

            Assert.Equal("varmkorv", result.RightValue());
        }

        [Fact]
        public async Task Async_bind_short_circuits_on_left()
        {
            var result = await ParseId("abc")
                .BindAsync(LookupName);

            Assert.Equal("'abc' is not a valid id", result.LeftValue());
        }

        [Fact]
        public async Task Async_bind_propagates_second_left()
        {
            var result = await ParseId("99")
                .BindAsync(LookupName);

            Assert.Equal("id 99 not found", result.LeftValue());
        }

        [Fact]
        public async Task Async_map_transforms_right_value()
        {
            var result = await ParseId("42")
                .BindAsync(LookupName)
                .MapAsync(FormatGreeting);

            Assert.Equal("Hello, varmkorv!", result.RightValue());
        }

        [Fact]
        public async Task Async_chain_then_await_for_sync_operations()
        {
            var greeting = (await ParseId("42")
                    .BindAsync(LookupName))
                .Map(name => $"Welcome, {name}!")
                .DefaultWith(left => $"Left: {left}");

            Assert.Equal("Welcome, varmkorv!", greeting);
        }

        [Fact]
        public async Task Sequence_enables_sync_map_with_async_function()
        {
            Either<string, string> name = Either<string, string>.FromRight("varmkorv");

            var result = await name
                .Map(FormatGreeting)
                .Sequence();

            Assert.Equal("Hello, varmkorv!", result.RightValue());
        }

        [Fact]
        public async Task Sequence_skips_async_work_on_left()
        {
            Either<string, string> name = Either<string, string>.FromLeft("not found");

            var result = await name
                .Map(FormatGreeting)
                .Sequence();

            Assert.Equal("not found", result.LeftValue());
        }

        [Fact]
        public async Task Sequence_propagates_faulted_inner_task()
        {
            var either = Either<string, Task<string>>.FromRight(
                Task.FromException<string>(new InvalidOperationException("faulted")));

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => either.Sequence());

            Assert.Equal("faulted", ex.Message);
        }
    }
}
