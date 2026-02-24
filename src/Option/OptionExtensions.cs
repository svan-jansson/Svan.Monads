using System;
using System.Collections.Generic;

namespace Svan.Monads
{
    /// <summary>Extension methods for combining and transforming <see cref="Option{T}"/> values.</summary>
    public static class OptionExtensions
    {
        /// <summary>
        /// Merge two or more options together. Merge will only be performed if all involved options resolve to Some.
        /// </summary>
        /// <returns>The values merged into an option of a tuple</returns>
        /// <example>
        /// <code>
        /// var merged = Option.Some(1).Merge(Option.Some("a")); // Some(Tuple(1, "a"))
        /// var none   = Option.Some(1).Merge(Option&lt;string&gt;.None()); // None
        ///
        /// // Chain further Merge calls to collect up to five values:
        /// var triple = Option.Some(1).Merge(Option.Some(2)).Merge(Option.Some(3));
        /// </code>
        /// </example>
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
        /// <example>
        /// <code>
        /// var some = new[] { Option.Some(1), Option.Some(2), Option.Some(3) }.Sequence(); // Some([1, 2, 3])
        /// var none = new[] { Option.Some(1), Option&lt;int&gt;.None() }.Sequence();            // None
        /// </code>
        /// </example>
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
        /// Flattens a nested <see cref="Option{T}"/> where the value is itself an <see cref="Option{T}"/> into a flat <see cref="Option{T}"/>.
        /// Returns the inner option when <c>Some</c>, or <c>None</c> when the outer option is <c>None</c>.
        /// </summary>
        /// <example>
        /// <code>
        /// var flat = Option.Some(Option.Some(42)).Flatten();           // Some(42)
        /// var none = Option.Some(Option&lt;int&gt;.None()).Flatten();     // None
        /// var outerNone = Option&lt;Option&lt;int&gt;&gt;.None().Flatten(); // None
        /// </code>
        /// </example>
        public static Option<T> Flatten<T>(this Option<Option<T>> option)
            => option.DefaultWith(Option<T>.None);
    }
}
