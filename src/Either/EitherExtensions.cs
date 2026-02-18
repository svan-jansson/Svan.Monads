using System;
using System.Collections.Generic;

namespace Svan.Monads
{
    public static class EitherExtensions
    {
        /// <summary>
        /// Merge two eithers together. Only merges if both are Right.
        /// </summary>
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
        /// Flattens a nested <c>Either&lt;TLeft, Either&lt;TLeft, TRight&gt;&gt;</c> into an <c>Either&lt;TLeft, TRight&gt;</c>.
        /// Returns the inner either when Right, or propagates the outer Left.
        /// </summary>
        public static Either<TLeft, TRight> Flatten<TLeft, TRight>(
            this Either<TLeft, Either<TLeft, TRight>> either)
            => either.DefaultWith(Either<TLeft, TRight>.FromLeft);
    }
}
