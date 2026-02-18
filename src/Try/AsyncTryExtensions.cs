using System;
using System.Threading.Tasks;

namespace Svan.Monads
{
    public static class AsyncTryExtensions
    {
        /// <summary>
        /// Flips a <c>Try&lt;Task&lt;TSuccess&gt;&gt;</c> into a <c>Task&lt;Try&lt;TSuccess&gt;&gt;</c>.
        /// Awaits the inner task when <c>Success</c>, returns the error immediately when <c>Error</c>.
        /// If the inner task faults, the exception is caught and returned as the error state.
        /// </summary>
        public static async Task<Try<TSuccess>> Sequence<TSuccess>(
            this Try<Task<TSuccess>> tryTask)
        {
            if (tryTask.IsError())
                return Try.Exception<TSuccess>(tryTask.ErrorValue());
            try
            {
                return Try.Success(await tryTask.SuccessValue().ConfigureAwait(false));
            }
            catch (Exception ex)
            {
                return Try.Exception<TSuccess>(ex);
            }
        }

        /// <summary>
        /// Binds an async function over a <c>Task&lt;Try&lt;TSuccess&gt;&gt;</c>.
        /// The binder is only called when the result is <c>Success</c>; otherwise short-circuits with the existing error.
        /// Exceptions thrown by the binder or faulted tasks will propagate as faulted tasks.
        /// Use <see cref="BindCatchingAsync{TSuccess, TOut}"/> to catch exceptions into the error state instead.
        /// </summary>
        public static async Task<Try<TOut>> BindAsync<TSuccess, TOut>(
            this Task<Try<TSuccess>> tryTask,
            Func<TSuccess, Task<Try<TOut>>> binder)
        {
            var result = await tryTask.ConfigureAwait(false);
            if (result.IsSuccess())
                return await binder(result.SuccessValue()).ConfigureAwait(false);
            return Try.Exception<TOut>(result.ErrorValue());
        }

        /// <summary>
        /// Maps an async function over a <c>Task&lt;Try&lt;TSuccess&gt;&gt;</c>.
        /// The mapper is only called when the result is <c>Success</c>; otherwise short-circuits with the existing error.
        /// Exceptions thrown by the mapper or faulted tasks will propagate as faulted tasks.
        /// Use <see cref="MapCatchingAsync{TSuccess, TOut}"/> to catch exceptions into the error state instead.
        /// </summary>
        public static async Task<Try<TOut>> MapAsync<TSuccess, TOut>(
            this Task<Try<TSuccess>> tryTask,
            Func<TSuccess, Task<TOut>> mapper)
        {
            var result = await tryTask.ConfigureAwait(false);
            return result.IsSuccess() ? Try.Success(await mapper(result.SuccessValue()).ConfigureAwait(false)) : Try.Exception<TOut>(result.ErrorValue());
        }

        /// <summary>
        /// Binds an async function over a <c>Task&lt;Try&lt;TSuccess&gt;&gt;</c>, catching any exceptions.
        /// Exceptions thrown by the binder or faulted tasks are caught and returned as the error state,
        /// keeping the chain inside the monad.
        /// </summary>
        public static async Task<Try<TOut>> BindCatchingAsync<TSuccess, TOut>(
            this Task<Try<TSuccess>> tryTask,
            Func<TSuccess, Task<Try<TOut>>> binder)
        {
            var result = await tryTask.ConfigureAwait(false);
            if (!result.IsSuccess()) return Try.Exception<TOut>(result.ErrorValue());
            try
            {
                return await binder(result.SuccessValue()).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                return Try.Exception<TOut>(ex);
            }
        }

        /// <summary>
        /// Maps an async function over a <c>Task&lt;Try&lt;TSuccess&gt;&gt;</c>, catching any exceptions.
        /// Exceptions thrown by the mapper or faulted tasks are caught and returned as the error state,
        /// keeping the chain inside the monad.
        /// </summary>
        public static async Task<Try<TOut>> MapCatchingAsync<TSuccess, TOut>(
            this Task<Try<TSuccess>> tryTask,
            Func<TSuccess, Task<TOut>> mapper)
        {
            var result = await tryTask.ConfigureAwait(false);
            if (!result.IsSuccess()) return Try.Exception<TOut>(result.ErrorValue());

            try
            {
                return Try.Success(await mapper(result.SuccessValue()).ConfigureAwait(false));
            }
            catch (Exception ex)
            {
                return Try.Exception<TOut>(ex);
            }
        }
    }
}
