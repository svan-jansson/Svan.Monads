using System;
using System.Collections.Generic;

namespace Svan.Monads
{
    public static class ResultExtensions
    {
        /// <summary>
        /// Merge two or more results together. Merge will only be performed if all involved results resolve to <c>TSuccess</c>.
        /// </summary>
        /// <returns>The success values merged into a result using a tuple</returns>
        public static Result<TError, Tuple<TFirst, TSecond>> Merge<TError, TFirst, TSecond>(
            this Result<TError, TFirst> first, Result<TError, TSecond> second) =>
                first.Zip(second, (f, s) => new Tuple<TFirst, TSecond>(f, s));

        /// <summary>
        /// Merge two or more results together. Merge will only be performed if all involved results resolve to <c>TSuccess</c>.
        /// </summary>
        /// <returns>The success values merged into a result using a tuple</returns>
        public static Result<TError, Tuple<TFirst, TSecond, TThird>> Merge<TError, TFirst, TSecond, TThird>(
            this Result<TError, Tuple<TFirst, TSecond>> group, Result<TError, TThird> other) =>
                group.Zip(other, (g, o) => new Tuple<TFirst, TSecond, TThird>(g.Item1, g.Item2, o));

        /// <summary>
        /// Merge two or more results together. Merge will only be performed if all involved results resolve to <c>TSuccess</c>.
        /// </summary>
        /// <returns>The success values merged into a result using a tuple</returns>
        public static Result<TError, Tuple<TFirst, TSecond, TThird, TFourth>> Merge<TError, TFirst, TSecond, TThird, TFourth>(
            this Result<TError, Tuple<TFirst, TSecond, TThird>> group, Result<TError, TFourth> other) =>
                group.Zip(other, (g, o) => new Tuple<TFirst, TSecond, TThird, TFourth>(g.Item1, g.Item2, g.Item3, o));

        /// <summary>
        /// Merge two or more results together. Merge will only be performed if all involved results resolve to <c>TSuccess</c>.
        /// </summary>
        /// <returns>The success values merged into a result using a tuple</returns>
        public static Result<TError, Tuple<TFirst, TSecond, TThird, TFourth, TFifth>> Merge<TError, TFirst, TSecond, TThird, TFourth, TFifth>(
            this Result<TError, Tuple<TFirst, TSecond, TThird, TFourth>> group, Result<TError, TFifth> other) =>
                group.Zip(other, (g, o) => new Tuple<TFirst, TSecond, TThird, TFourth, TFifth>(g.Item1, g.Item2, g.Item3, g.Item4, o));

        /// <summary>
        /// Combine a sequence of results together. Returns <c>Success</c> with all values if every result is <c>Success</c>,
        /// or the first <c>Error</c> encountered.
        /// </summary>
        public static Result<TError, IEnumerable<TSuccess>> Sequence<TError, TSuccess>(this IEnumerable<Result<TError, TSuccess>> results)
        {
            var values = new List<TSuccess>();

            foreach (var result in results)
            {
                if (result.IsError())
                {
                    return Result<TError, IEnumerable<TSuccess>>.Error(result.ErrorValue());
                }

                values.Add(result.SuccessValue());
            }

            return Result<TError, IEnumerable<TSuccess>>.Success(values);
        }

        /// <summary>
        /// Flattens a nested <c>Result&lt;TError, Result&lt;TError, TSuccess&gt;&gt;</c> into a <c>Result&lt;TError, TSuccess&gt;</c>.
        /// Returns the inner result when <c>Success</c>, or propagates the outer error when <c>Error</c>.
        /// </summary>
        public static Result<TError, TSuccess> Flatten<TError, TSuccess>(this Result<TError, Result<TError, TSuccess>> result)
            => result.DefaultWith(Result<TError, TSuccess>.Error);
    }
}
