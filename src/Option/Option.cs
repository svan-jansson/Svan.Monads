using System;

namespace Svan.Monads
{
    /// <summary>
    /// Union of <c>None</c> and <c>T</c> with monad features for the Maybe flow control
    /// </summary>
    /// <example>
    /// <code>
    /// var port = GetConfigValue("PORT")
    ///     .Bind(s => int.TryParse(s, out var n) ? Option.Some(n) : Option&lt;int&gt;.None())
    ///     .Filter(n => n is > 0 and &lt; 65536)
    ///     .DefaultWith(8080);
    /// </code>
    /// </example>
    public class Option<T> : Union<None, T>
    {
        internal Option(None value) : base(new Left<None>(value)) { }
        internal Option(T value) : base(new Right<T>(value)) { }

        /// <summary>Creates a <c>None</c> option.</summary>
        public static Option<T> None() => new(new None());
        /// <summary>Creates a <c>Some</c> option wrapping <paramref name="value"/>.</summary>
        public static Option<T> Some(T value) => new(value);

        /// <summary>
        /// Returns <c>true</c> if the option is <c>None</c>.
        /// </summary>
        public bool IsNone() => IsLeft;

        /// <summary>
        /// Returns <c>true</c> if the option is <c>Some</c>.
        /// </summary>
        public bool IsSome() => IsRight;

        /// <summary>
        /// Returns the current value. Will throw <c>NullReferenceException</c> if current option state is None.
        /// </summary>
        public T Value() => IsSome() ? AsRight : throw new NullReferenceException($"Expected some {typeof(T).Name} but was none.");

        /// <summary>
        /// Bind the <see cref="Option{T}"/> to an <see cref="Option{T}"/> using a binder function. The binder function will not be executed if the current state of the option is <c>None</c>.
        /// </summary>
        /// <param name="binder">A function that returns an <see cref="Option{T}"/></param>
        /// <returns>An option of the output type of the binder. </returns>
        /// <example>
        /// <code>
        /// Option&lt;int&gt; ParsePositive(string s) =>
        ///     int.TryParse(s, out var n) &amp;&amp; n > 0 ? Option.Some(n) : Option&lt;int&gt;.None();
        ///
        /// var some    = Option.Some("42").Bind(ParsePositive); // Some(42)
        /// var none    = Option.Some("-1").Bind(ParsePositive); // None
        /// var skipped = Option&lt;string&gt;.None().Bind(ParsePositive); // None — binder not called
        /// </code>
        /// </example>
        public Option<TOut> Bind<TOut>(Func<T, Option<TOut>> binder)
            => Match(
                _ => Option<TOut>.None(),
                binder);

        /// <summary>
        /// Map the value of the option to an <see cref="Option{T}"/> using a mapping function. The mapping function will not be executed if the current state of the option is <c>None</c>.
        /// </summary>
        /// <param name="mapping">A function that returns a value of <typeparamref name="TOut"/></param>
        /// <typeparam name="TOut"></typeparam>
        /// <returns>An option of the output type of the mapping</returns>
        /// <example>
        /// <code>
        /// var doubled = Option.Some(21).Map(x => x * 2);       // Some(42)
        /// var none    = Option&lt;int&gt;.None().Map(x => x * 2); // None — mapper not called
        /// </code>
        /// </example>
        public Option<TOut> Map<TOut>(Func<T, TOut> mapping)
            => Match(
                _ => Option<TOut>.None(),
                value => Option<TOut>.Some(mapping(value)));

        /// <summary>
        /// Filter the value using a filter function. The filter function will not be executed if the current state of the option is <c>None</c>.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns><c>Some</c> when filter returns true. <c>None</c> when filter returns false or current state of option is <c>None</c></returns>
        /// <example>
        /// <code>
        /// var even = Option.Some(4).Filter(n => n % 2 == 0); // Some(4)
        /// var odd  = Option.Some(3).Filter(n => n % 2 == 0); // None
        /// </code>
        /// </example>
        public Option<T> Filter(Func<T, bool> filter)
            => Match(
                _ => None(),
                value => filter(value) ? Some(value) : None());

        /// <summary>
        /// Do lets you fire and forget an action that is executed only when the value is <c>Some</c>
        /// </summary>
        /// <param name="do">An action that takes a single parameter of T</param>
        /// <returns>The current state of the Option</returns>
        /// <example>
        /// <code>
        /// Option.Some(42).Do(n => Console.WriteLine(n));       // prints 42
        /// Option&lt;int&gt;.None().Do(n => Console.WriteLine(n)); // nothing printed
        /// </code>
        /// </example>
        public Option<T> Do(Action<T> @do)
        {
            if (IsSome())
            {
                @do(Value());
            }

            return this;
        }

        /// <summary>
        /// Do lets you fire and forget an action that is executed only when the value is None
        /// </summary>
        /// <param name="do">An action that takes no parameters</param>
        /// <returns>The current state of the Option</returns>
        public Option<T> DoIfNone(Action @do)
        {
            if (IsNone())
            {
                @do();
            }

            return this;
        }

        /// <summary>
        /// Fold into value of type <typeparamref name="TOut"/> with supplied functions for case <c>None</c> and case <c>Some</c>.
        /// </summary>
        /// <example>
        /// <code>
        /// var msg = Option.Some(42).Fold(() => "nothing", n => $"got {n}"); // "got 42"
        /// var def = Option&lt;int&gt;.None().Fold(() => "nothing", n => $"got {n}"); // "nothing"
        /// </code>
        /// </example>
        public TOut Fold<TOut>(Func<TOut> caseNone, Func<T, TOut> caseSome)
            => Match(
                _ => caseNone(),
                caseSome);

        /// <summary>
        /// Get the value of <c>Some</c> or a default value from the supplied function.
        /// </summary>
        /// <example>
        /// <code>
        /// var value    = Option.Some(42).DefaultWith(() => 0);    // 42
        /// var fallback = Option&lt;int&gt;.None().DefaultWith(() => 0); // 0
        /// </code>
        /// </example>
        public T DefaultWith(Func<T> defaultNone)
            => Fold(
                defaultNone,
                value => value);

        /// <summary>
        /// Get the value of <c>Some</c> or a default value from the supplied function.
        /// </summary>
        public T DefaultWith(T defaultNone)
            => Fold(
                () => defaultNone,
                value => value);

        /// <summary>
        /// Get the value of <c>Some</c> or throw a <see cref="NullReferenceException"/>.
        /// </summary>
        /// <example>
        /// <code>
        /// var value = Option.Some(42).OrThrow(); // 42
        /// Option&lt;int&gt;.None().OrThrow();         // throws NullReferenceException
        /// </code>
        /// </example>
        public T OrThrow()
            => Fold(
                () => throw new NullReferenceException($"Expected some {typeof(T).Name} but was none."),
                value => value);

        /// <summary>
        /// Combine several options into a new option or <c>None</c> if any of the provided options are <c>None</c>
        /// </summary>
        public Option<TOut> Zip<TOut, TOther>(Option<TOther> other, Func<T, TOther, TOut> combine)
        {
            if (IsSome() && other.IsSome())
            {
                return Option<TOut>.Some(combine(Value(), other.Value()));
            }

            return Option<TOut>.None();
        }

        /// <summary>
        /// Combine several options into a new option or <c>None</c> if any of the provided options are <c>None</c>
        /// </summary>
        public Option<TOut> Zip<TOut, TFirstOther, TSecondOther>(
            Option<TFirstOther> firstOther,
            Option<TSecondOther> secondOther,
            Func<T, TFirstOther, TSecondOther, TOut> combine)
        {
            if (IsSome() && firstOther.IsSome() && secondOther.IsSome())
            {
                return Option<TOut>.Some(combine(Value(), firstOther.Value(), secondOther.Value()));
            }

            return Option<TOut>.None();
        }

        /// <summary>
        /// Combine several options into a new option or <c>None</c> if any of the provided options are <c>None</c>
        /// </summary>
        public Option<TOut> Zip<TOut, TFirstOther, TSecondOther, TThirdOther>(
            Option<TFirstOther> firstOther,
            Option<TSecondOther> secondOther,
            Option<TThirdOther> thirdOther,
            Func<T, TFirstOther, TSecondOther, TThirdOther, TOut> combine)
        {
            if (IsSome()
                && firstOther.IsSome()
                && secondOther.IsSome()
                && thirdOther.IsSome())
            {
                return Option<TOut>.Some(combine(
                    Value(),
                    firstOther.Value(),
                    secondOther.Value(),
                    thirdOther.Value()));
            }

            return Option<TOut>.None();
        }

        /// <summary>
        /// Combine several options into a new option or <c>None</c> if any of the provided options are <c>None</c>
        /// </summary>
        public Option<TOut> Zip<TOut, TFirstOther, TSecondOther, TThirdOther, TFourthOther>(
            Option<TFirstOther> firstOther,
            Option<TSecondOther> secondOther,
            Option<TThirdOther> thirdOther,
            Option<TFourthOther> fourthOther,
            Func<T, TFirstOther, TSecondOther, TThirdOther, TFourthOther, TOut> combine)
        {
            if (IsSome()
                && firstOther.IsSome()
                && secondOther.IsSome()
                && thirdOther.IsSome()
                && fourthOther.IsSome())
            {
                return Option<TOut>.Some(combine(
                    Value(),
                    firstOther.Value(),
                    secondOther.Value(),
                    thirdOther.Value(),
                    fourthOther.Value()));
            }

            return Option<TOut>.None();
        }

        /// <summary>
        /// Create a <see cref="Result{TError, TSuccess}"/> from an <see cref="Option{T}"/> by supplying a mapper for the error case
        /// </summary>
        /// <example>
        /// <code>
        /// var success = Option.Some(42).ToResult(() => "was none");      // Success(42)
        /// var error   = Option&lt;int&gt;.None().ToResult(() => "was none"); // Error("was none")
        /// </code>
        /// </example>
        public Result<TError, T> ToResult<TError>(Func<TError> defaultError)
         => Fold(
                () => Result<TError, T>.Error(defaultError()),
                Result<TError, T>.Success
            );
    }
}
