using System;

namespace Svan.Monads
{
    /// <summary>
    /// A general-purpose discriminated union where both sides carry meaning.
    /// Right-biased for chainable pipelines, without the opinionated error/success semantics of <see cref="Result{TError, TSuccess}"/>.
    /// </summary>
    /// <example>
    /// <code>
    /// var result = Either&lt;string, int&gt;.FromRight(42)
    ///     .Map(n => n * 2)
    ///     .Bind(n => n > 50
    ///         ? Either&lt;string, int&gt;.FromRight(n)
    ///         : Either&lt;string, int&gt;.FromLeft("too small"))
    ///     .DefaultWith(_ => 0);
    /// // result == 84
    /// </code>
    /// </example>
    public class Either<TLeft, TRight> : Union<TLeft, TRight>
    {
        internal Either(Left<TLeft> value) : base(value) { }
        internal Either(Right<TRight> value) : base(value) { }

        /// <summary>Creates a Left either wrapping <paramref name="value"/>.</summary>
        public static Either<TLeft, TRight> FromLeft(TLeft value) => new(new Left<TLeft>(value));
        /// <summary>Creates a Right either wrapping <paramref name="value"/>.</summary>
        public static Either<TLeft, TRight> FromRight(TRight value) => new(new Right<TRight>(value));

        /// <summary>
        /// Returns the left value. Throws <see cref="NullReferenceException"/> if the value is Right.
        /// </summary>
        public TLeft LeftValue() => IsLeft ? AsLeft : throw new NullReferenceException("Cannot access Left when value is Right.");

        /// <summary>
        /// Returns the right value. Throws <see cref="NullReferenceException"/> if the value is Left.
        /// </summary>
        public TRight RightValue() => IsRight ? AsRight : throw new NullReferenceException("Cannot access Right when value is Left.");

        /// <summary>
        /// Map the right value to a new type. Short-circuits on Left.
        /// </summary>
        /// <example>
        /// <code>
        /// var doubled = Either&lt;string, int&gt;.FromRight(21).Map(x => x * 2); // Right(42)
        /// var left    = Either&lt;string, int&gt;.FromLeft("err").Map(x => x * 2); // Left("err")
        /// </code>
        /// </example>
        public Either<TLeft, TOut> Map<TOut>(Func<TRight, TOut> map)
            => Match(Either<TLeft, TOut>.FromLeft, r => Either<TLeft, TOut>.FromRight(map(r)));

        /// <summary>
        /// Bind the right value to a new <see cref="Either{TLeft, TRight}"/>. Short-circuits on Left.
        /// </summary>
        /// <example>
        /// <code>
        /// var result  = Either&lt;string, int&gt;.FromRight(10)
        ///     .Bind(n => n > 0
        ///         ? Either&lt;string, int&gt;.FromRight(n)
        ///         : Either&lt;string, int&gt;.FromLeft("non-positive")); // Right(10)
        ///
        /// var skipped = Either&lt;string, int&gt;.FromLeft("err")
        ///     .Bind(n => Either&lt;string, int&gt;.FromRight(n * 2)); // Left("err") — binder not called
        /// </code>
        /// </example>
        public Either<TLeft, TOut> Bind<TOut>(Func<TRight, Either<TLeft, TOut>> binder)
            => Match(Either<TLeft, TOut>.FromLeft, binder);

        /// <summary>
        /// Execute an action on the right value. Returns the current Either unchanged.
        /// </summary>
        /// <example>
        /// <code>
        /// Either&lt;string, int&gt;.FromRight(42).Do(n => Console.WriteLine(n));    // prints 42
        /// Either&lt;string, int&gt;.FromLeft("err").Do(n => Console.WriteLine(n)); // nothing printed
        /// </code>
        /// </example>
        public Either<TLeft, TRight> Do(Action<TRight> @do)
        {
            if (IsRight)
            {
                @do(RightValue());
            }

            return this;
        }

        /// <summary>
        /// Get the right value or a fallback derived from the left value.
        /// </summary>
        /// <example>
        /// <code>
        /// var value    = Either&lt;string, int&gt;.FromRight(42).DefaultWith(l => l.Length); // 42
        /// var fallback = Either&lt;string, int&gt;.FromLeft("err").DefaultWith(l => l.Length); // 3
        /// </code>
        /// </example>
        public TRight DefaultWith(Func<TLeft, TRight> fallback) => Match(fallback, r => r);

        /// <summary>
        /// Get the right value or a fallback value.
        /// </summary>
        public TRight DefaultWith(TRight fallback) => Match(_ => fallback, r => r);

        /// <summary>
        /// Get the right value or throw an <see cref="InvalidOperationException"/>.
        /// </summary>
        /// <example>
        /// <code>
        /// var value = Either&lt;string, int&gt;.FromRight(42).OrThrow(); // 42
        /// Either&lt;string, int&gt;.FromLeft("err").OrThrow();           // throws InvalidOperationException
        /// </code>
        /// </example>
        public TRight OrThrow()
            => Match(
                l => throw new InvalidOperationException($"Expected a right value of {typeof(TRight).Name} but was left: {l}."),
                r => r);

        /// <summary>
        /// Map the left value to a new type. Short-circuits on Right.
        /// </summary>
        /// <example>
        /// <code>
        /// var upper = Either&lt;string, int&gt;.FromLeft("err").MapLeft(s => s.ToUpper()); // Left("ERR")
        /// var right = Either&lt;string, int&gt;.FromRight(42).MapLeft(s => s.ToUpper());   // Right(42)
        /// </code>
        /// </example>
        public Either<TOut, TRight> MapLeft<TOut>(Func<TLeft, TOut> map)
            => Match(l => Either<TOut, TRight>.FromLeft(map(l)), Either<TOut, TRight>.FromRight);

        /// <summary>
        /// Bind the left value to a new <see cref="Either{TLeft, TRight}"/>. Short-circuits on Right.
        /// </summary>
        /// <example>
        /// <code>
        /// var result    = Either&lt;string, int&gt;.FromLeft("err")
        ///     .BindLeft(l => Either&lt;int, int&gt;.FromLeft(l.Length)); // Left(3)
        ///
        /// var passThrough = Either&lt;string, int&gt;.FromRight(42)
        ///     .BindLeft(l => Either&lt;int, int&gt;.FromLeft(l.Length)); // Right(42) — binder not called
        /// </code>
        /// </example>
        public Either<TOut, TRight> BindLeft<TOut>(Func<TLeft, Either<TOut, TRight>> binder)
            => Match(binder, Either<TOut, TRight>.FromRight);

        /// <summary>
        /// Execute an action on the left value. Returns the current Either unchanged.
        /// </summary>
        /// <example>
        /// <code>
        /// Either&lt;string, int&gt;.FromLeft("err").DoIfLeft(l => Console.WriteLine(l)); // prints "err"
        /// Either&lt;string, int&gt;.FromRight(42).DoIfLeft(l => Console.WriteLine(l));   // nothing printed
        /// </code>
        /// </example>
        public Either<TLeft, TRight> DoIfLeft(Action<TLeft> @do)
        {
            if (IsLeft)
            {
                @do(LeftValue());
            }

            return this;
        }

        /// <summary>
        /// Fold into a value of type <typeparamref name="TOut"/> with supplied functions for each case.
        /// </summary>
        /// <example>
        /// <code>
        /// var msg  = Either&lt;string, int&gt;.FromRight(42).Fold(l => $"left: {l}", r => $"right: {r}"); // "right: 42"
        /// var left = Either&lt;string, int&gt;.FromLeft("err").Fold(l => $"left: {l}", r => $"right: {r}"); // "left: err"
        /// </code>
        /// </example>
        public TOut Fold<TOut>(Func<TLeft, TOut> caseLeft, Func<TRight, TOut> caseRight)
            => Match(caseLeft, caseRight);

        /// <summary>
        /// Swap left and right sides.
        /// </summary>
        /// <example>
        /// <code>
        /// var swapped = Either&lt;string, int&gt;.FromRight(42).Swap(); // Left(42) as Either&lt;int, string&gt;
        /// var flipped = Either&lt;string, int&gt;.FromLeft("err").Swap(); // Right("err") as Either&lt;int, string&gt;
        /// </code>
        /// </example>
        public Either<TRight, TLeft> Swap()
            => Match(Either<TRight, TLeft>.FromRight, Either<TRight, TLeft>.FromLeft);

        /// <summary>
        /// Combine two eithers into a new right value using a combine function. Returns first Left encountered.
        /// </summary>
        public Either<TLeft, TOut> Zip<TOut, TOther>(
            Either<TLeft, TOther> other,
            Func<TRight, TOther, TOut> combine)
        {
            if (IsLeft) return Either<TLeft, TOut>.FromLeft(LeftValue());
            if (other.IsLeft) return Either<TLeft, TOut>.FromLeft(other.LeftValue());
            return Either<TLeft, TOut>.FromRight(combine(RightValue(), other.RightValue()));
        }

        /// <summary>
        /// Combine three eithers into a new right value using a combine function. Returns first Left encountered.
        /// </summary>
        public Either<TLeft, TOut> Zip<TOut, TFirstOther, TSecondOther>(
            Either<TLeft, TFirstOther> firstOther,
            Either<TLeft, TSecondOther> secondOther,
            Func<TRight, TFirstOther, TSecondOther, TOut> combine)
        {
            if (IsLeft) return Either<TLeft, TOut>.FromLeft(LeftValue());
            if (firstOther.IsLeft) return Either<TLeft, TOut>.FromLeft(firstOther.LeftValue());
            if (secondOther.IsLeft) return Either<TLeft, TOut>.FromLeft(secondOther.LeftValue());
            return Either<TLeft, TOut>.FromRight(combine(RightValue(), firstOther.RightValue(), secondOther.RightValue()));
        }

        /// <summary>
        /// Combine four eithers into a new right value using a combine function. Returns first Left encountered.
        /// </summary>
        public Either<TLeft, TOut> Zip<TOut, TFirstOther, TSecondOther, TThirdOther>(
            Either<TLeft, TFirstOther> firstOther,
            Either<TLeft, TSecondOther> secondOther,
            Either<TLeft, TThirdOther> thirdOther,
            Func<TRight, TFirstOther, TSecondOther, TThirdOther, TOut> combine)
        {
            if (IsLeft) return Either<TLeft, TOut>.FromLeft(LeftValue());
            if (firstOther.IsLeft) return Either<TLeft, TOut>.FromLeft(firstOther.LeftValue());
            if (secondOther.IsLeft) return Either<TLeft, TOut>.FromLeft(secondOther.LeftValue());
            if (thirdOther.IsLeft) return Either<TLeft, TOut>.FromLeft(thirdOther.LeftValue());
            return Either<TLeft, TOut>.FromRight(combine(RightValue(), firstOther.RightValue(), secondOther.RightValue(), thirdOther.RightValue()));
        }

        /// <summary>
        /// Combine five eithers into a new right value using a combine function. Returns first Left encountered.
        /// </summary>
        public Either<TLeft, TOut> Zip<TOut, TFirstOther, TSecondOther, TThirdOther, TFourthOther>(
            Either<TLeft, TFirstOther> firstOther,
            Either<TLeft, TSecondOther> secondOther,
            Either<TLeft, TThirdOther> thirdOther,
            Either<TLeft, TFourthOther> fourthOther,
            Func<TRight, TFirstOther, TSecondOther, TThirdOther, TFourthOther, TOut> combine)
        {
            if (IsLeft) return Either<TLeft, TOut>.FromLeft(LeftValue());
            if (firstOther.IsLeft) return Either<TLeft, TOut>.FromLeft(firstOther.LeftValue());
            if (secondOther.IsLeft) return Either<TLeft, TOut>.FromLeft(secondOther.LeftValue());
            if (thirdOther.IsLeft) return Either<TLeft, TOut>.FromLeft(thirdOther.LeftValue());
            if (fourthOther.IsLeft) return Either<TLeft, TOut>.FromLeft(fourthOther.LeftValue());
            return Either<TLeft, TOut>.FromRight(combine(RightValue(), firstOther.RightValue(), secondOther.RightValue(), thirdOther.RightValue(), fourthOther.RightValue()));
        }

        /// <summary>
        /// Convert to <see cref="Option{TRight}"/>. Left becomes None, Right becomes Some.
        /// </summary>
        /// <example>
        /// <code>
        /// var some = Either&lt;string, int&gt;.FromRight(42).ToOption(); // Some(42)
        /// var none = Either&lt;string, int&gt;.FromLeft("err").ToOption(); // None
        /// </code>
        /// </example>
        public Option<TRight> ToOption() => Match(_ => Option<TRight>.None(), Option<TRight>.Some);

        /// <summary>
        /// Convert to <see cref="Result{TLeft, TRight}"/>. Left becomes Error, Right becomes Success.
        /// </summary>
        /// <example>
        /// <code>
        /// var success = Either&lt;string, int&gt;.FromRight(42).ToResult(); // Success(42)
        /// var error   = Either&lt;string, int&gt;.FromLeft("err").ToResult(); // Error("err")
        /// </code>
        /// </example>
        public Result<TLeft, TRight> ToResult() => Match(Result<TLeft, TRight>.Error, Result<TLeft, TRight>.Success);
    }
}
