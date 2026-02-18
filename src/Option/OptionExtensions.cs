using System;
using System.Collections.Generic;

namespace Svan.Monads
{
    public static class OptionExtensions
    {
        /// <summary>
        /// Merge two or more options together. Merge will only be performed if all involved options resolve to Some.
        /// </summary>
        /// <returns>The values merged into an option of a tuple</returns>
        public static Option<Tuple<TFirst, TSecond>> Merge<TFirst, TSecond>(
            this Option<TFirst> first, Option<TSecond> second) =>
            first.Zip(second, (f, s) => new Tuple<TFirst, TSecond>(f, s));

        /// <summary>
        /// Merge two or more options together. Merge will only be performed if all involved options resolve to Some.
        /// </summary>
        /// <returns>The values merged into an option of a tuple</returns>
        public static Option<Tuple<TFirst, TSecond, TThird>> Merge<TFirst, TSecond, TThird>(
            this Option<Tuple<TFirst, TSecond>> group, Option<TThird> other) =>
            group.Zip(other, (g, o) => new Tuple<TFirst, TSecond, TThird>(g.Item1, g.Item2, o));

        /// <summary>
        /// Merge two or more options together. Merge will only be performed if all involved options resolve to Some.
        /// </summary>
        /// <returns>The values merged into an option of a tuple</returns>
        public static Option<Tuple<TFirst, TSecond, TThird, TFourth>> Merge<TFirst, TSecond, TThird, TFourth>(
            this Option<Tuple<TFirst, TSecond, TThird>> group, Option<TFourth> other) =>
            group.Zip(other, (g, o) => new Tuple<TFirst, TSecond, TThird, TFourth>(g.Item1, g.Item2, g.Item3, o));

        /// <summary>
        /// Merge two or more options together. Merge will only be performed if all involved options resolve to Some.
        /// </summary>
        /// <returns>The values merged into an option of a tuple</returns>
        public static Option<Tuple<TFirst, TSecond, TThird, TFourth, TFifth>> Merge<TFirst, TSecond, TThird, TFourth,
            TFifth>(
            this Option<Tuple<TFirst, TSecond, TThird, TFourth>> group, Option<TFifth> other) =>
            group.Zip(other,
                (g, o) => new Tuple<TFirst, TSecond, TThird, TFourth, TFifth>(g.Item1, g.Item2, g.Item3, g.Item4, o));

        /// <summary>
        /// Combine a sequence of options together. Combine will only be performed if all involved options resolve to Some.
        /// </summary>
        /// <returns>The values merged into an option of an Enumerable</returns>
        public static Option<IEnumerable<T>> Sequence<T>(this IEnumerable<Option<T>> options)
        {
            var result = new List<T>();

            foreach (var option in options)
            {
                if (option.IsNone())
                {
                    return Option<IEnumerable<T>>.None();
                }

                result.Add(option.Value());
            }

            return Option<IEnumerable<T>>.Some(result);
        }

        /// <summary>
        /// Flattens a nested <c>Option&lt;Option&lt;T&gt;&gt;</c> into an <c>Option&lt;T&gt;</c>.
        /// Returns the inner option when <c>Some</c>, or <c>None</c> when the outer option is <c>None</c>.
        /// </summary>
        public static Option<T> Flatten<T>(this Option<Option<T>> option)
            => option.DefaultWith(Option<T>.None);
    }
}
