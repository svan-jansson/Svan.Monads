using System;
using System.Threading.Tasks;

namespace Svan.Monads
{
    public static class AsyncResultExtensions
    {
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

        public static async Task<Result<TError, TOut>> BindAsync<TError, TSuccess, TOut>(
            this Task<Result<TError, TSuccess>> resultTask,
            Func<TSuccess, Task<Result<TError, TOut>>> binder)
        {
            var result = await resultTask.ConfigureAwait(false);
            return result.IsSuccess() ? await binder(result.SuccessValue()).ConfigureAwait(false) : Result<TError, TOut>.Error(result.ErrorValue());
        }

        public static async Task<Result<TError, TOut>> MapAsync<TError, TSuccess, TOut>(
            this Task<Result<TError, TSuccess>> resultTask,
            Func<TSuccess, Task<TOut>> mapper)
        {
            var result = await resultTask.ConfigureAwait(false);
            return result.IsSuccess() ? Result<TError, TOut>.Success(await mapper(result.SuccessValue()).ConfigureAwait(false)) : Result<TError, TOut>.Error(result.ErrorValue());
        }
    }
}
