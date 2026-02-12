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
            return await tryTask.SuccessValue();
        }

        public static async Task<Try<TOut>> BindAsync<TSuccess, TOut>(
            this Task<Try<TSuccess>> tryTask,
            Func<TSuccess, Task<Try<TOut>>> binder)
        {
            var result = await tryTask;
            if (result.IsSuccess())
                return await binder(result.SuccessValue());
            return result.ErrorValue();
        }

        public static async Task<Try<TOut>> MapAsync<TSuccess, TOut>(
            this Task<Try<TSuccess>> tryTask,
            Func<TSuccess, Task<TOut>> mapper)
        {
            var result = await tryTask;
            if (result.IsSuccess())
                return await mapper(result.SuccessValue());
            return result.ErrorValue();
        }
    }
}
