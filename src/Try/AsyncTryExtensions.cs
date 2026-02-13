using System;
using System.Threading.Tasks;

namespace Svan.Monads
{
    public static class AsyncTryExtensions
    {
        public static async Task<Try<TSuccess>> Sequence<TSuccess>(
            this Try<Task<TSuccess>> tryTask)
        {
            if (tryTask.IsError())
                return tryTask.ErrorValue();
            return await tryTask.SuccessValue().ConfigureAwait(false);
        }

        public static async Task<Try<TOut>> BindAsync<TSuccess, TOut>(
            this Task<Try<TSuccess>> tryTask,
            Func<TSuccess, Task<Try<TOut>>> binder)
        {
            var result = await tryTask.ConfigureAwait(false);
            if (result.IsSuccess())
                return await binder(result.SuccessValue()).ConfigureAwait(false);
            return result.ErrorValue();
        }

        public static async Task<Try<TOut>> MapAsync<TSuccess, TOut>(
            this Task<Try<TSuccess>> tryTask,
            Func<TSuccess, Task<TOut>> mapper)
        {
            var result = await tryTask.ConfigureAwait(false);
            if (result.IsSuccess())
                return await mapper(result.SuccessValue()).ConfigureAwait(false);
            return result.ErrorValue();
        }

        public static async Task<Try<TOut>> BindCatchingAsync<TSuccess, TOut>(
            this Task<Try<TSuccess>> tryTask,
            Func<TSuccess, Task<Try<TOut>>> binder)
        {
            var result = await tryTask.ConfigureAwait(false);
            if (result.IsSuccess())
            {
                try
                {
                    return await binder(result.SuccessValue()).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    return ex;
                }
            }
            return result.ErrorValue();
        }

        public static async Task<Try<TOut>> MapCatchingAsync<TSuccess, TOut>(
            this Task<Try<TSuccess>> tryTask,
            Func<TSuccess, Task<TOut>> mapper)
        {
            var result = await tryTask.ConfigureAwait(false);
            if (result.IsSuccess())
            {
                try
                {
                    return await mapper(result.SuccessValue()).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    return ex;
                }
            }
            return result.ErrorValue();
        }
    }
}
