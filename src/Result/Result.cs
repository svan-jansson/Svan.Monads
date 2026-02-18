using System;

namespace Svan.Monads
{
    /// <summary>
    /// Union of <c>TError</c> and <c>TSuccess</c> with monad features for railway-oriented error handling.
    /// </summary>
    public class Result<TError, TSuccess> : Union<TError, TSuccess>
    {
        internal Result(Left<TError> value) : base(value) { }
        internal Result(Right<TSuccess> value) : base(value) { }

        public static Result<TError, TSuccess> Error(TError value) => new(new Left<TError>(value));
        public static Result<TError, TSuccess> Success(TSuccess value) => new(new Right<TSuccess>(value));

        /// <summary>
        /// Returns <c>true</c> if the result is in the error state.
        /// </summary>
        public bool IsError() => IsLeft;

        /// <summary>
        /// Returns <c>true</c> if the result is in the success state.
        /// </summary>
        public bool IsSuccess() => IsRight;

        /// <summary>
        /// Returns the error value. Throws <c>NullReferenceException</c> if the result is <c>Success</c>.
        /// </summary>
        public TError ErrorValue() => IsError() ? AsLeft : throw new NullReferenceException();

        /// <summary>
        /// Returns the success value. Throws <c>NullReferenceException</c> if the result is <c>Error</c>.
        /// </summary>
        public TSuccess SuccessValue() => IsSuccess() ? AsRight : throw new NullReferenceException();

        /// <summary>
        /// Bind the result to a new <c>Result&lt;TError, TOut&gt;</c> using a binder function.
        /// The binder is only called when <c>Success</c>; otherwise short-circuits with the existing error.
        /// </summary>
        public Result<TError, TOut> Bind<TOut>(Func<TSuccess, Result<TError, TOut>> binder)
            => Match(Result<TError, TOut>.Error, binder);

        /// <summary>
        /// Bind the error to a new <c>Result&lt;TOut, TSuccess&gt;</c> using a binder function.
        /// The binder is only called when <c>Error</c>; otherwise short-circuits with the existing success value.
        /// </summary>
        public Result<TOut, TSuccess> BindError<TOut>(Func<TError, Result<TOut, TSuccess>> binder)
            => Match(binder, Result<TOut, TSuccess>.Success);

        /// <summary>
        /// Recover from <c>TError</c> by providing a <c>TSuccess</c> or a new error <c>TOut</c>.
        /// </summary>
        public Result<TOut, TSuccess> Recover<TOut>(Func<TError, Result<TOut, TSuccess>> recover)
            => BindError(recover);

        /// <summary>
        /// Map the success value to a new type using a mapping function.
        /// The mapper is only called when <c>Success</c>; otherwise short-circuits with the existing error.
        /// </summary>
        public Result<TError, TOut> Map<TOut>(Func<TSuccess, TOut> mapSuccess)
            => Match(
                Result<TError, TOut>.Error,
                success => Result<TError, TOut>.Success(mapSuccess(success)));

        /// <summary>
        /// Map the error value to a new type using a mapping function.
        /// The mapper is only called when <c>Error</c>; otherwise short-circuits with the existing success value.
        /// </summary>
        public Result<TOut, TSuccess> MapError<TOut>(Func<TError, TOut> mapError)
            => Match(
                error => Result<TOut, TSuccess>.Error(mapError(error)),
                Result<TOut, TSuccess>.Success);

        /// <summary>
        /// Do lets you fire and forget an action that is executed only when the value is <see cref="TSuccess"/>
        /// </summary>
        /// <param name="do">An action that takes a single parameter of <see cref="TSuccess"/></param>
        /// <returns>The current state of the Result</returns>
        public Result<TError, TSuccess> Do(Action<TSuccess> @do)
        {
            if (IsSuccess())
            {
                @do(SuccessValue());
            }

            return this;
        }

        /// <summary>
        /// Do lets you fire and forget an action that is executed only when the value is <see cref="TError"/>
        /// </summary>
        /// <param name="do">An action that takes a single parameter of <see cref="TError"/></param>
        /// <returns>The current state of the Result</returns>
        public Result<TError, TSuccess> DoIfError(Action<TError> @do)
        {
            if (IsError())
            {
                @do(ErrorValue());
            }

            return this;
        }

        /// <summary>
        /// Get the value of <c>TSuccess</c> or a default value from the supplied function.
        /// </summary>
        public TSuccess DefaultWith(Func<TError, TSuccess> fallback)
            => Match(fallback, success => success);
        
        /// <summary>
        /// Get the value of <c>TSuccess</c> or a default value from the supplied value.
        /// </summary>
        public TSuccess DefaultWith(TSuccess fallback)
            => Match(_ => fallback, success => success);

        /// <summary>
        /// Get the value of <c>TSuccess</c> or throw a <see cref="NullReferenceException"/>.
        /// </summary>
        public TSuccess OrThrow()
            => Match(
                error => throw new InvalidOperationException($"Expected a successful value of {typeof(TSuccess).Name} but was {error}."),
                success => success);

        /// <summary>
        /// Fold into value of type <c>TOut</c> with supplied functions for case <c>TError</c> and case <c>TSuccess</c>.
        /// </summary>
        public TOut Fold<TOut>(Func<TError, TOut> caseError, Func<TSuccess, TOut> caseSuccess)
            => Match(caseError, caseSuccess);

        /// <summary>
        /// Combine several results into a new result of <c>TSuccessOut</c> or <c>TError</c> if any of the provided results has an error
        /// </summary>
        public Result<TError, TSuccessOut> Zip<TSuccessOut, TSuccessOther>(
            Result<TError, TSuccessOther> other,
            Func<TSuccess, TSuccessOther, TSuccessOut> combine)
        {
            if (IsError())
                return Result<TError, TSuccessOut>.Error(ErrorValue());
            if (other.IsError())
                return Result<TError, TSuccessOut>.Error(other.ErrorValue());

            return Result<TError, TSuccessOut>.Success(combine(SuccessValue(), other.SuccessValue()));
        }

        /// <summary>
        /// Combine several results into a new result of <c>TSuccessOut</c> or <c>TError</c> if any of the provided results has an error
        /// </summary>
        public Result<TError, TSuccessOut> Zip<TSuccessOut, TSuccessFirstOther, TSuccessSecondOther>(
            Result<TError, TSuccessFirstOther> firstOther,
            Result<TError, TSuccessSecondOther> secondOther,
            Func<TSuccess, TSuccessFirstOther, TSuccessSecondOther, TSuccessOut> combine)
        {
            if (IsError())
                return Result<TError, TSuccessOut>.Error(ErrorValue());
            if (firstOther.IsError())
                return Result<TError, TSuccessOut>.Error(firstOther.ErrorValue());
            if (secondOther.IsError())
                return Result<TError, TSuccessOut>.Error(secondOther.ErrorValue());

            return Result<TError, TSuccessOut>.Success(combine(SuccessValue(), firstOther.SuccessValue(), secondOther.SuccessValue()));
        }

        /// <summary>
        /// Combine several results into a new result of <c>TSuccessOut</c> or <c>TError</c> if any of the provided results has an error
        /// </summary>
        public Result<TError, TSuccessOut> Zip<TSuccessOut, TSuccessFirstOther, TSuccessSecondOther, TSuccessThirdOther>(
            Result<TError, TSuccessFirstOther> firstOther,
            Result<TError, TSuccessSecondOther> secondOther,
            Result<TError, TSuccessThirdOther> thirdOther,
            Func<TSuccess, TSuccessFirstOther, TSuccessSecondOther, TSuccessThirdOther, TSuccessOut> combine)
        {
            if (IsError())
                return Result<TError, TSuccessOut>.Error(ErrorValue());
            if (firstOther.IsError())
                return Result<TError, TSuccessOut>.Error(firstOther.ErrorValue());
            if (secondOther.IsError())
                return Result<TError, TSuccessOut>.Error(secondOther.ErrorValue());
            if (thirdOther.IsError())
                return Result<TError, TSuccessOut>.Error(thirdOther.ErrorValue());

            return Result<TError, TSuccessOut>.Success(combine(
                SuccessValue(),
                firstOther.SuccessValue(),
                secondOther.SuccessValue(),
                thirdOther.SuccessValue()));
        }

        /// <summary>
        /// Combine several results into a new result of <c>TSuccessOut</c> or <c>TError</c> if any of the provided results has an error
        /// </summary>
        public Result<TError, TSuccessOut> Zip<
            TSuccessOut,
            TSuccessFirstOther,
            TSuccessSecondOther,
            TSuccessThirdOther,
            TSuccessFourthOther>(
            Result<TError, TSuccessFirstOther> firstOther,
            Result<TError, TSuccessSecondOther> secondOther,
            Result<TError, TSuccessThirdOther> thirdOther,
            Result<TError, TSuccessFourthOther> fourthOther,
            Func<
                TSuccess,
                TSuccessFirstOther,
                TSuccessSecondOther,
                TSuccessThirdOther,
                TSuccessFourthOther,
                TSuccessOut> combine)
        {
            if (IsError())
                return Result<TError, TSuccessOut>.Error(ErrorValue());
            if (firstOther.IsError())
                return Result<TError, TSuccessOut>.Error(firstOther.ErrorValue());
            if (secondOther.IsError())
                return Result<TError, TSuccessOut>.Error(secondOther.ErrorValue());
            if (thirdOther.IsError())
                return Result<TError, TSuccessOut>.Error(thirdOther.ErrorValue());
            if (fourthOther.IsError())
                return Result<TError, TSuccessOut>.Error(fourthOther.ErrorValue());

            return Result<TError, TSuccessOut>.Success(combine(
                SuccessValue(),
                firstOther.SuccessValue(),
                secondOther.SuccessValue(),
                thirdOther.SuccessValue(),
                fourthOther.SuccessValue()));
        }

        /// <summary>
        /// Downcast to an <c>Option&lt;TSuccess&gt;</c>. When the result is <c>Error</c> it will return <c>None</c>.
        /// </summary>
        /// <returns></returns>
        public Option<TSuccess> ToOption()
            => Match(
                _ => Option<TSuccess>.None(),
                Option<TSuccess>.Some);
    }
}
