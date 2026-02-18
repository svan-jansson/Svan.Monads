using System;

namespace Svan.Monads
{
    /// <summary>
    /// A general-purpose discriminated union where both sides carry meaning.
    /// Right-biased for chainable pipelines, without the opinionated error/success semantics of <see cref="Result{TError, TSuccess}"/>.
    /// </summary>
    public class Either<TLeft, TRight> : Union<TLeft, TRight>
    {
        internal Either(Left<TLeft> value) : base(value) { }
        internal Either(Right<TRight> value) : base(value) { }

        public static Either<TLeft, TRight> FromLeft(TLeft value) => new(new Left<TLeft>(value));
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
        public Either<TLeft, TOut> Map<TOut>(Func<TRight, TOut> map)
            => Match(Either<TLeft, TOut>.FromLeft, r => Either<TLeft, TOut>.FromRight(map(r)));

        /// <summary>
        /// Bind the right value to a new <c>Either&lt;TLeft, TOut&gt;</c>. Short-circuits on Left.
        /// </summary>
        public Either<TLeft, TOut> Bind<TOut>(Func<TRight, Either<TLeft, TOut>> binder)
            => Match(Either<TLeft, TOut>.FromLeft, binder);

        /// <summary>
        /// Execute an action on the right value. Returns the current Either unchanged.
        /// </summary>
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
        public TRight DefaultWith(Func<TLeft, TRight> fallback) => Match(fallback, r => r);

        /// <summary>
        /// Get the right value or a fallback value.
        /// </summary>
        public TRight DefaultWith(TRight fallback) => Match(_ => fallback, r => r);

        /// <summary>
        /// Get the right value or throw an <see cref="InvalidOperationException"/>.
        /// </summary>
        public TRight OrThrow()
            => Match(
                l => throw new InvalidOperationException($"Expected a right value of {typeof(TRight).Name} but was left: {l}."),
                r => r);

        /// <summary>
        /// Map the left value to a new type. Short-circuits on Right.
        /// </summary>
        public Either<TOut, TRight> MapLeft<TOut>(Func<TLeft, TOut> map)
            => Match(l => Either<TOut, TRight>.FromLeft(map(l)), Either<TOut, TRight>.FromRight);

        /// <summary>
        /// Bind the left value to a new <c>Either&lt;TOut, TRight&gt;</c>. Short-circuits on Right.
        /// </summary>
        public Either<TOut, TRight> BindLeft<TOut>(Func<TLeft, Either<TOut, TRight>> binder)
            => Match(binder, Either<TOut, TRight>.FromRight);

        /// <summary>
        /// Execute an action on the left value. Returns the current Either unchanged.
        /// </summary>
        public Either<TLeft, TRight> DoIfLeft(Action<TLeft> @do)
        {
            if (IsLeft)
            {
                @do(LeftValue());
            }

            return this;
        }

        /// <summary>
        /// Fold into a value of type <c>TOut</c> with supplied functions for each case.
        /// </summary>
        public TOut Fold<TOut>(Func<TLeft, TOut> caseLeft, Func<TRight, TOut> caseRight)
            => Match(caseLeft, caseRight);

        /// <summary>
        /// Swap left and right sides.
        /// </summary>
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
        public Option<TRight> ToOption() => Match(_ => Option<TRight>.None(), Option<TRight>.Some);

        /// <summary>
        /// Convert to <see cref="Result{TLeft, TRight}"/>. Left becomes Error, Right becomes Success.
        /// </summary>
        public Result<TLeft, TRight> ToResult() => Match(Result<TLeft, TRight>.Error, Result<TLeft, TRight>.Success);
    }
}
