using System;
using System.Threading.Tasks;

namespace Svan.Monads
{
    public static class AsyncResultExtensions
    {
        /// <summary>
        /// Flips a <c>Result&lt;TError, Task&lt;TSuccess&gt;&gt;</c> into a <c>Task&lt;Result&lt;TError, TSuccess&gt;&gt;</c>.
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
        /// Binds an async function over a <c>Task&lt;Result&lt;TError, TSuccess&gt;&gt;</c>.
        /// The binder is only called when the result is <c>Success</c>; otherwise short-circuits with the existing error.
        /// </summary>
        public static async Task<Result<TError, TOut>> BindAsync<TError, TSuccess, TOut>(
            this Task<Result<TError, TSuccess>> resultTask,
            Func<TSuccess, Task<Result<TError, TOut>>> binder)
        {
            var result = await resultTask.ConfigureAwait(false);
            return result.IsSuccess() ? await binder(result.SuccessValue()).ConfigureAwait(false) : Result<TError, TOut>.Error(result.ErrorValue());
        }

        /// <summary>
        /// Maps an async function over a <c>Task&lt;Result&lt;TError, TSuccess&gt;&gt;</c>.
        /// The mapper is only called when the result is <c>Success</c>; otherwise short-circuits with the existing error.
        /// </summary>
        public static async Task<Result<TError, TOut>> MapAsync<TError, TSuccess, TOut>(
            this Task<Result<TError, TSuccess>> resultTask,
            Func<TSuccess, Task<TOut>> mapper)
        {
            var result = await resultTask.ConfigureAwait(false);
            return result.IsSuccess() ? Result<TError, TOut>.Success(await mapper(result.SuccessValue()).ConfigureAwait(false)) : Result<TError, TOut>.Error(result.ErrorValue());
        }
    }
}
