using System;
using System.Threading.Tasks;

namespace Svan.Monads
{
    /// <summary>Async extension methods for <see cref="Option{T}"/> enabling fluent async pipelines.</summary>
    public static class AsyncOptionExtensions
    {
        /// <summary>
        /// Flips an <see cref="Option{T}"/> of <see cref="Task{TResult}"/> into a <see cref="Task{TResult}"/> of <see cref="Option{T}"/>.
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
        /// Binds an async function over a <see cref="Task{TResult}"/> of <see cref="Option{T}"/>.
        /// The binder is only called when the option is <c>Some</c>; otherwise short-circuits to <c>None</c>.
        /// </summary>
        /// <example>
        /// <code>
        /// async Task&lt;Option&lt;string&gt;&gt; FindEmail(int userId) { ... }
        ///
        /// var email = await FindUserId("malin")
        ///     .BindAsync(id => FindEmail(id));
        /// </code>
        /// </example>
        public static async Task<Option<TOut>> BindAsync<T, TOut>(
            this Task<Option<T>> optionTask,
            Func<T, Task<Option<TOut>>> binder)
        {
            var option = await optionTask.ConfigureAwait(false);
            return option.IsSome() ? await binder(option.Value()).ConfigureAwait(false) : Option<TOut>.None();
        }

        /// <summary>
        /// Maps an async function over a <see cref="Task{TResult}"/> of <see cref="Option{T}"/>.
        /// The mapper is only called when the option is <c>Some</c>; otherwise short-circuits to <c>None</c>.
        /// </summary>
        /// <example>
        /// <code>
        /// async Task&lt;string&gt; NormalizeEmail(string email) { ... }
        ///
        /// var normalized = await FindUserId("malin")
        ///     .BindAsync(id => FindEmail(id))
        ///     .MapAsync(email => NormalizeEmail(email));
        /// </code>
        /// </example>
        public static async Task<Option<TOut>> MapAsync<T, TOut>(
            this Task<Option<T>> optionTask,
            Func<T, Task<TOut>> mapper)
        {
            var option = await optionTask.ConfigureAwait(false);
            return option.IsSome() ? Option<TOut>.Some(await mapper(option.Value()).ConfigureAwait(false)) : Option<TOut>.None();
        }
    }
}
