using System;
using System.Collections.Generic;

namespace Svan.Monads
{
    public static class TryExtensions
    {
        /// <summary>
        /// Combine a sequence of tries together. Returns <c>Success</c> with all values if every try is <c>Success</c>,
        /// or the first <c>Exception</c> encountered.
        /// </summary>
        public static Try<IEnumerable<TSuccess>> Sequence<TSuccess>(this IEnumerable<Try<TSuccess>> tries)
        {
            var values = new List<TSuccess>();

            foreach (var @try in tries)
            {
                if (@try.IsError())
                {
                    return Try<IEnumerable<TSuccess>>.Exception(@try.ErrorValue());
                }

                values.Add(@try.SuccessValue());
            }

            return Try<IEnumerable<TSuccess>>.Success(values);
        }

        /// <summary>
        /// Flattens a nested <c>Try&lt;Try&lt;TSuccess&gt;&gt;</c> into a <c>Try&lt;TSuccess&gt;</c>.
        /// Returns the inner try when <c>Success</c>, or propagates the outer exception when <c>Error</c>.
        /// </summary>
        public static Try<TSuccess> Flatten<TSuccess>(this Try<Try<TSuccess>> result)
            => result.DefaultWith(Try.Exception<TSuccess>);
    }
}
