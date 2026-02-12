using System;
using System.Threading.Tasks;

namespace Svan.Monads
{
    public static class AsyncOptionExtensions
    {
        public static async Task<Option<T>> Sequence<T>(this Option<Task<T>> optionTask)
        {
            if (optionTask.IsNone())
            {
                return Option<T>.None();
            }

            var task = optionTask.Value();
            var result = await task;
            return Option<T>.Some(result);
        }
        
        public static async Task<Option<TOut>> BindAsync<T, TOut>(
            this Task<Option<T>> optionTask,
            Func<T, Task<Option<TOut>>> binder)
        {
            var option = await optionTask;
            return option.IsSome() ? await binder(option.Value()) : Option<TOut>.None();
        }

        public static async Task<Option<TOut>> MapAsync<T, TOut>(
            this Task<Option<T>> optionTask,
            Func<T, Task<TOut>> mapper)
        {
            var option = await optionTask;
            return option.IsSome() ? Option<TOut>.Some(await mapper(option.Value())) : Option<TOut>.None();
        }
    }
}
