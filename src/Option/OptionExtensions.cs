using System;

namespace Svan.Monads
{
    public static class OptionExtensions
    {
        /// <summary>
        /// Converts any type <c>T</c> to <c>Option&lt;T&gt;</c>. 
        /// </summary>
        /// <returns>Returns <c>Some&lt;T&gt;</c> for value types.
        /// Returns <c>Some&lt;T&gt;</c> for reference types that are not <c>null</c> and <c>None</c> for reference types that are <c>null</c>.</returns>
        public static Option<T> ToOption<T>(this T value)
        {
            Option<T> result;

            if (typeof(T).IsValueType)
            {
                result = Option<T>.Some(value);
            }
            else
            {
                result = value == null
                    ? Option<T>.None()
                    : Option<T>.Some(value);
            }

            return result;
        }

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
        public static Option<Tuple<TFirst, TSecond, TThird, TFourth, TFifth>> Merge<TFirst, TSecond, TThird, TFourth, TFifth>(
            this Option<Tuple<TFirst, TSecond, TThird, TFourth>> group, Option<TFifth> other) =>
                group.Zip(other, (g, o) => new Tuple<TFirst, TSecond, TThird, TFourth, TFifth>(g.Item1, g.Item2, g.Item3, g.Item4, o));
    }
}
