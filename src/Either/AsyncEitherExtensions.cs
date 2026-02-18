using System;
using System.Threading.Tasks;

namespace Svan.Monads
{
    public static class AsyncEitherExtensions
    {
        /// <summary>
        /// Flips an <c>Either&lt;TLeft, Task&lt;TRight&gt;&gt;</c> into a <c>Task&lt;Either&lt;TLeft, TRight&gt;&gt;</c>.
        /// Awaits the inner task when <c>Right</c>, returns the left value immediately when <c>Left</c>.
        /// </summary>
        public static async Task<Either<TLeft, TRight>> Sequence<TLeft, TRight>(
            this Either<TLeft, Task<TRight>> eitherTask)
        {
            if (eitherTask.IsLeft)
            {
                return Either<TLeft, TRight>.FromLeft(eitherTask.LeftValue());
            }

            var value = await eitherTask.RightValue().ConfigureAwait(false);
            return Either<TLeft, TRight>.FromRight(value);
        }

        /// <summary>
        /// Binds an async function over a <c>Task&lt;Either&lt;TLeft, TRight&gt;&gt;</c>.
        /// The binder is only called when the either is <c>Right</c>; otherwise short-circuits with the existing left value.
        /// </summary>
        public static async Task<Either<TLeft, TOut>> BindAsync<TLeft, TRight, TOut>(
            this Task<Either<TLeft, TRight>> eitherTask,
            Func<TRight, Task<Either<TLeft, TOut>>> binder)
        {
            var either = await eitherTask.ConfigureAwait(false);
            return either.IsRight
                ? await binder(either.RightValue()).ConfigureAwait(false)
                : Either<TLeft, TOut>.FromLeft(either.LeftValue());
        }

        /// <summary>
        /// Maps an async function over a <c>Task&lt;Either&lt;TLeft, TRight&gt;&gt;</c>.
        /// The mapper is only called when the either is <c>Right</c>; otherwise short-circuits with the existing left value.
        /// </summary>
        public static async Task<Either<TLeft, TOut>> MapAsync<TLeft, TRight, TOut>(
            this Task<Either<TLeft, TRight>> eitherTask,
            Func<TRight, Task<TOut>> mapper)
        {
            var either = await eitherTask.ConfigureAwait(false);
            return either.IsRight
                ? Either<TLeft, TOut>.FromRight(await mapper(either.RightValue()).ConfigureAwait(false))
                : Either<TLeft, TOut>.FromLeft(either.LeftValue());
        }
    }
}
