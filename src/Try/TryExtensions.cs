using System;
using System.Collections.Generic;

namespace Svan.Monads
{
    /// <summary>Extension methods for combining and transforming <see cref="Try{TSuccess}"/> values.</summary>
    public static class TryExtensions
    {
        /// <summary>
        /// Combine a sequence of tries together. Returns <c>Success</c> with all values if every try is <c>Success</c>,
        /// or the first <c>Exception</c> encountered.
        /// </summary>
        /// <example>
        /// <code>
        /// var all  = new[] { Try.Success(1), Try.Success(2) }.Sequence(); // Success([1, 2])
        /// var fail = new[] { Try.Success(1), Try.Exception&lt;int&gt;(new Exception("oops")) }.Sequence();
        /// // Exception("oops")
        /// </code>
        /// </example>
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
        /// Flattens a nested <see cref="Try{TSuccess}"/> where the success is itself a <see cref="Try{TSuccess}"/> into a flat <see cref="Try{TSuccess}"/>.
        /// Returns the inner try when <c>Success</c>, or propagates the outer exception when <c>Error</c>.
        /// </summary>
        /// <example>
        /// <code>
        /// var flat     = Try.Success(Try.Success(42)).Flatten();  // Success(42)
        /// var innerErr = Try.Success(Try.Exception&lt;int&gt;(new Exception("inner"))).Flatten();
        /// // Exception("inner")
        /// </code>
        /// </example>
        public static Try<TSuccess> Flatten<TSuccess>(this Try<Try<TSuccess>> result)
            => result.DefaultWith(Try.Exception<TSuccess>);
    }
}
