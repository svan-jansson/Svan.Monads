using System;
using System.Threading.Tasks;

namespace Svan.Monads
{
    /// <summary>Async extension methods for <see cref="Result{TError,TSuccess}"/> enabling fluent async pipelines.</summary>
    public static class AsyncResultExtensions
    {
        /// <summary>
        /// Flips a <see cref="Result{TError, TSuccess}"/> of <see cref="Task{TResult}"/> into a <see cref="Task{TResult}"/> of <see cref="Result{TError, TSuccess}"/>.
        /// Awaits the inner task when <c>Success</c>, returns the error immediately when <c>Error</c>.
        /// </summary>
        public static async Task<Result<TError, TSuccess>> Sequence<TError, TSuccess>(
            this Result<TError, Task<TSuccess>> resultTask)
        {
            if (resultTask.IsError())
            {
                return Result<TError, TSuccess>.Error(resultTask.ErrorValue());
            }

            var value = await resultTask.SuccessValue().ConfigureAwait(false);
            return Result<TError, TSuccess>.Success(value);
        }

        /// <summary>
        /// Binds an async function over a <see cref="Task{TResult}"/> of <see cref="Result{TError, TSuccess}"/>.
        /// The binder is only called when the result is <c>Success</c>; otherwise short-circuits with the existing error.
        /// </summary>
        /// <example>
        /// <code>
        /// async Task&lt;Result&lt;string, string&gt;&gt; LookupUsername(int userId) { ... }
        ///
        /// var username = await ParseUserId("42")
        ///     .BindAsync(id => LookupUsername(id));
        /// </code>
        /// </example>
        public static async Task<Result<TError, TOut>> BindAsync<TError, TSuccess, TOut>(
            this Task<Result<TError, TSuccess>> resultTask,
            Func<TSuccess, Task<Result<TError, TOut>>> binder)
        {
            var result = await resultTask.ConfigureAwait(false);
            return result.IsSuccess() ? await binder(result.SuccessValue()).ConfigureAwait(false) : Result<TError, TOut>.Error(result.ErrorValue());
        }

        /// <summary>
        /// Maps an async function over a <see cref="Task{TResult}"/> of <see cref="Result{TError, TSuccess}"/>.
        /// The mapper is only called when the result is <c>Success</c>; otherwise short-circuits with the existing error.
        /// </summary>
        /// <example>
        /// <code>
        /// async Task&lt;string&gt; FormatGreeting(string username) { ... }
        ///
        /// var greeting = await ParseUserId("42")
        ///     .BindAsync(id => LookupUsername(id))
        ///     .MapAsync(name => FormatGreeting(name));
        /// </code>
        /// </example>
        public static async Task<Result<TError, TOut>> MapAsync<TError, TSuccess, TOut>(
            this Task<Result<TError, TSuccess>> resultTask,
            Func<TSuccess, Task<TOut>> mapper)
        {
            var result = await resultTask.ConfigureAwait(false);
            return result.IsSuccess() ? Result<TError, TOut>.Success(await mapper(result.SuccessValue()).ConfigureAwait(false)) : Result<TError, TOut>.Error(result.ErrorValue());
        }
    }
}
