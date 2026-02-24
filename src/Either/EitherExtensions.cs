using System;
using System.Collections.Generic;

namespace Svan.Monads
{
    /// <summary>Extension methods for combining and transforming <see cref="Either{TLeft,TRight}"/> values.</summary>
    public static class EitherExtensions
    {
        /// <summary>
        /// Merge two eithers together. Only merges if both are Right.
        /// </summary>
        /// <example>
        /// <code>
        /// var merged = Either&lt;string, int&gt;.FromRight(1).Merge(Either&lt;string, string&gt;.FromRight("a"));
        /// // Right(Tuple(1, "a"))
        ///
        /// // Chain further Merge calls to collect up to five values:
        /// var triple = Either&lt;string, int&gt;.FromRight(1)
        ///     .Merge(Either&lt;string, int&gt;.FromRight(2))
        ///     .Merge(Either&lt;string, int&gt;.FromRight(3));
        /// </code>
        /// </example>
        public static Either<TLeft, Tuple<TFirst, TSecond>> Merge<TLeft, TFirst, TSecond>(
            this Either<TLeft, TFirst> first, Either<TLeft, TSecond> second) =>
                first.Zip(second, (f, s) => new Tuple<TFirst, TSecond>(f, s));

        /// <summary>
        /// Merge a third either into an existing merged pair. Only merges if all are Right.
        /// </summary>
        public static Either<TLeft, Tuple<TFirst, TSecond, TThird>> Merge<TLeft, TFirst, TSecond, TThird>(
            this Either<TLeft, Tuple<TFirst, TSecond>> group, Either<TLeft, TThird> other) =>
                group.Zip(other, (g, o) => new Tuple<TFirst, TSecond, TThird>(g.Item1, g.Item2, o));

        /// <summary>
        /// Merge a fourth either into an existing merged triple. Only merges if all are Right.
        /// </summary>
        public static Either<TLeft, Tuple<TFirst, TSecond, TThird, TFourth>> Merge<TLeft, TFirst, TSecond, TThird, TFourth>(
            this Either<TLeft, Tuple<TFirst, TSecond, TThird>> group, Either<TLeft, TFourth> other) =>
                group.Zip(other, (g, o) => new Tuple<TFirst, TSecond, TThird, TFourth>(g.Item1, g.Item2, g.Item3, o));

        /// <summary>
        /// Merge a fifth either into an existing merged quad. Only merges if all are Right.
        /// </summary>
        public static Either<TLeft, Tuple<TFirst, TSecond, TThird, TFourth, TFifth>> Merge<TLeft, TFirst, TSecond, TThird, TFourth, TFifth>(
            this Either<TLeft, Tuple<TFirst, TSecond, TThird, TFourth>> group, Either<TLeft, TFifth> other) =>
                group.Zip(other, (g, o) => new Tuple<TFirst, TSecond, TThird, TFourth, TFifth>(g.Item1, g.Item2, g.Item3, g.Item4, o));

        /// <summary>
        /// Combine a sequence of eithers. Returns Right with all values if every either is Right,
        /// or the first Left encountered.
        /// </summary>
        /// <example>
        /// <code>
        /// var all  = new[] { Either&lt;string, int&gt;.FromRight(1), Either&lt;string, int&gt;.FromRight(2) }.Sequence();
        /// // Right([1, 2])
        ///
        /// var fail = new[] { Either&lt;string, int&gt;.FromRight(1), Either&lt;string, int&gt;.FromLeft("oops") }.Sequence();
        /// // Left("oops")
        /// </code>
        /// </example>
        public static Either<TLeft, IEnumerable<TRight>> Sequence<TLeft, TRight>(
            this IEnumerable<Either<TLeft, TRight>> eithers)
        {
            var values = new List<TRight>();

            foreach (var either in eithers)
            {
                if (either.IsLeft)
                {
                    return Either<TLeft, IEnumerable<TRight>>.FromLeft(either.LeftValue());
                }

                values.Add(either.RightValue());
            }

            return Either<TLeft, IEnumerable<TRight>>.FromRight(values);
        }

        /// <summary>
        /// Flattens a nested <see cref="Either{TLeft, TRight}"/> where the right is itself an <see cref="Either{TLeft, TRight}"/> into a flat <see cref="Either{TLeft, TRight}"/>.
        /// Returns the inner either when Right, or propagates the outer Left.
        /// </summary>
        /// <example>
        /// <code>
        /// var flat     = Either&lt;string, Either&lt;string, int&gt;&gt;.FromRight(Either&lt;string, int&gt;.FromRight(42)).Flatten();
        /// // Right(42)
        ///
        /// var innerLeft = Either&lt;string, Either&lt;string, int&gt;&gt;.FromRight(Either&lt;string, int&gt;.FromLeft("inner")).Flatten();
        /// // Left("inner")
        ///
        /// var outerLeft = Either&lt;string, Either&lt;string, int&gt;&gt;.FromLeft("outer").Flatten();
        /// // Left("outer")
        /// </code>
        /// </example>
        public static Either<TLeft, TRight> Flatten<TLeft, TRight>(
            this Either<TLeft, Either<TLeft, TRight>> either)
            => either.DefaultWith(Either<TLeft, TRight>.FromLeft);
    }
}
