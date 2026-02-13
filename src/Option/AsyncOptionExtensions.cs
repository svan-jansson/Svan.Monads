using System;
using System.Threading.Tasks;

namespace Svan.Monads
{
    public static class AsyncOptionExtensions
    {
        /// <summary>
        /// Flips an <c>Option&lt;Task&lt;T&gt;&gt;</c> into a <c>Task&lt;Option&lt;T&gt;&gt;</c>.
        /// Awaits the inner task when <c>Some</c>, returns <c>None</c> immediately when <c>None</c>.
        /// </summary>
        public static async Task<Option<T>> Sequence<T>(this Option<Task<T>> optionTask)
        {
            if (optionTask.IsNone())
            {
                return Option<T>.None();
            }

            var task = optionTask.Value();
            var result = await task.ConfigureAwait(false);
            return Option<T>.Some(result);
        }

        /// <summary>
        /// Binds an async function over a <c>Task&lt;Option&lt;T&gt;&gt;</c>.
        /// The binder is only called when the option is <c>Some</c>; otherwise short-circuits to <c>None</c>.
        /// </summary>
        public static async Task<Option<TOut>> BindAsync<T, TOut>(
            this Task<Option<T>> optionTask,
            Func<T, Task<Option<TOut>>> binder)
        {
            var option = await optionTask.ConfigureAwait(false);
            return option.IsSome() ? await binder(option.Value()).ConfigureAwait(false) : Option<TOut>.None();
        }

        /// <summary>
        /// Maps an async function over a <c>Task&lt;Option&lt;T&gt;&gt;</c>.
        /// The mapper is only called when the option is <c>Some</c>; otherwise short-circuits to <c>None</c>.
        /// </summary>
        public static async Task<Option<TOut>> MapAsync<T, TOut>(
            this Task<Option<T>> optionTask,
            Func<T, Task<TOut>> mapper)
        {
            var option = await optionTask.ConfigureAwait(false);
            return option.IsSome() ? Option<TOut>.Some(await mapper(option.Value()).ConfigureAwait(false)) : Option<TOut>.None();
        }
    }
}
