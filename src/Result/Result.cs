using System;
using OneOf.Types;
using OneOf;

namespace Svan.Monads
{
    public class Result<TError, TSuccess> : OneOfBase<Error<TError>, Success<TSuccess>>
    {
        public Result(OneOf<Error<TError>, Success<TSuccess>> _) : base(_) { }
        public static implicit operator Result<TError, TSuccess>(Error<TError> _) => new Result<TError, TSuccess>(_);
        public static implicit operator Result<TError, TSuccess>(Success<TSuccess> _) => new Result<TError, TSuccess>(_);

        public static implicit operator Result<TError, TSuccess>(TSuccess value) => new Success<TSuccess>(value);
        public static implicit operator Result<TError, TSuccess>(TError value) => new Error<TError>(value);

        public static Result<TError, TSuccess> Error(TError value) => new Error<TError>(value);
        public static Result<TError, TSuccess> Success(TSuccess value) => new Success<TSuccess>(value);

        public bool IsError() => this.IsT0;
        public bool IsSuccess() => this.IsT1;
        public TError ErrorValue() => IsError() ? this.AsT0.Value : throw new NullReferenceException();
        public TSuccess SuccessValue() => IsSuccess() ? this.AsT1.Value : throw new NullReferenceException();

        public Result<TError, TOut> Bind<TOut>(Func<TSuccess, Result<TError, TOut>> binder)
            => Match(
                error => Result<TError, TOut>.Error(error.Value),
                success => binder(success.Value));

        public Result<TOut, TSuccess> BindError<TOut>(Func<TError, Result<TOut, TSuccess>> binder)
            => Match(
                error => binder(error.Value),
                success => Result<TOut, TSuccess>.Success(success.Value));
        
        /// <summary>
        /// Recover from <c>TError</c> by providing a <c>TSuccess</c> or a new error <c>TOut</c>.
        /// </summary>
        public Result<TOut, TSuccess> Recover<TOut>(Func<TError, Result<TOut, TSuccess>> recover)
            => Match(
                error => recover(error.Value),
                success => success.Value);
        
        public Result<TError, TOut> Map<TOut>(Func<TSuccess, TOut> mapSuccess)
            => Match(
                error => Result<TError, TOut>.Error(error.Value),
                success => Result<TError, TOut>.Success(mapSuccess(success.Value)));

        public Result<TOut, TSuccess> MapError<TOut>(Func<TError, TOut> mapError)
            => Match(
                error => Result<TOut, TSuccess>.Error(mapError(error.Value)),
                success => Result<TOut, TSuccess>.Success(success.Value));

        /// <summary>
        /// Do let's you fire and forget an action that is executed only when the value is <see cref="TSuccess"/> 
        /// </summary>
        /// <param name="do">An action that takes a single parameter of <see cref="TSuccess"/></param>
        /// <returns>The current state of the Result</returns>
        public Result<TError, TSuccess> Do(Action<TSuccess> @do)
        {
            if (IsSuccess())
            {
                @do(this.SuccessValue());
            }

            return this;
        }

        /// <summary>
        /// Do let's you fire and forget an action that is executed only when the value is <see cref="TError"/> 
        /// </summary>
        /// <param name="do">An action that takes a single parameter of <see cref="TError"/></param>
        /// <returns>The current state of the Result</returns>
        public Result<TError, TSuccess> DoIfError(Action<TError> @do)
        {
            if (IsError())
            {
                @do(this.ErrorValue());
            }

            return this;
        }

        /// <summary>
        /// Get the value of <c>TSuccess</c> or a default value from the supplied function.
        /// </summary>
        public TSuccess DefaultWith(Func<TError, TSuccess> fallback)
            => Match(
                error => fallback(error.Value),
                success => success.Value);
        
        /// <summary>
        /// Get the value of <c>TSuccess</c> or throw a <see cref="NullReferenceException"/>.
        /// </summary>
        public TSuccess OrThrow()
            => Match(
                error => throw new InvalidOperationException($"Expected a successful value of {typeof(TSuccess).Name} but was {error}."),
                success => success.Value);
        
        /// <summary>
        /// Fold into value of type <c>TOut</c> with supplied functions for case <c>TError</c> and case <c>TSuccess</c>.
        /// </summary>
        public TOut Fold<TOut>(Func<TError, TOut> caseError, Func<TSuccess, TOut> caseSuccess)
            => Match(
                error => caseError(error.Value),
                success => caseSuccess(success.Value));

        /// <summary>
        /// Combine several results into a new result of <c>TSuccessOut</c> or <c>TError</c> if any of the provided results has an error
        /// </summary>
        public Result<TError, TSuccessOut> Zip<TSuccessOut, TSuccessOther>(
            Result<TError, TSuccessOther> other,
            Func<TSuccess, TSuccessOther, TSuccessOut> combine)
        {
            TError error = default;
            bool allSuccess = false;

            if (this.IsSuccess())
            {
                allSuccess = true;
            }
            else
            {
                error = this.ErrorValue();
            }

            if (allSuccess && !other.IsSuccess())
            {
                error = other.ErrorValue();
                allSuccess = false;
            }


            if (allSuccess)
            {
                return combine(this.SuccessValue(), other.SuccessValue());
            }

            return Result<TError, TSuccessOut>.Error(error);
        }

        /// <summary>
        /// Combine several results into a new result of <c>TSuccessOut</c> or <c>TError</c> if any of the provided results has an error
        /// </summary>
        public Result<TError, TSuccessOut> Zip<TSuccessOut, TSuccessFirstOther, TSuccessSecondOther>(
            Result<TError, TSuccessFirstOther> firstOther,
            Result<TError, TSuccessSecondOther> secondOther,
            Func<TSuccess, TSuccessFirstOther, TSuccessSecondOther, TSuccessOut> combine)
        {
            TError error = default;
            bool allSuccess = false;

            if (this.IsSuccess())
            {
                allSuccess = true;
            }
            else
            {
                error = this.ErrorValue();
            }

            if (allSuccess && !firstOther.IsSuccess())
            {
                allSuccess = false;
                error = firstOther.ErrorValue();
            }

            if (allSuccess && !secondOther.IsSuccess())
            {
                allSuccess = false;
                error = secondOther.ErrorValue();
            }


            if (allSuccess)
            {
                return combine(this.SuccessValue(), firstOther.SuccessValue(), secondOther.SuccessValue());
            }

            return Result<TError, TSuccessOut>.Error(error);
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
            TError error = default;
            bool allSuccess = false;

            if (this.IsSuccess())
            {
                allSuccess = true;
            }
            else
            {
                error = this.ErrorValue();
            }

            if (allSuccess && !firstOther.IsSuccess())
            {
                allSuccess = false;
                error = firstOther.ErrorValue();
            }

            if (allSuccess && !secondOther.IsSuccess())
            {
                allSuccess = false;
                error = secondOther.ErrorValue();
            }

            if (allSuccess && !thirdOther.IsSuccess())
            {
                allSuccess = false;
                error = thirdOther.ErrorValue();
            }


            if (allSuccess)
            {
                return combine(
                    this.SuccessValue(),
                    firstOther.SuccessValue(),
                    secondOther.SuccessValue(),
                    thirdOther.SuccessValue());
            }

            return Result<TError, TSuccessOut>.Error(error);
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
            TError error = default;
            bool allSuccess = false;

            if (this.IsSuccess())
            {
                allSuccess = true;
            }
            else
            {
                error = this.ErrorValue();
            }

            if (allSuccess && !firstOther.IsSuccess())
            {
                allSuccess = false;
                error = firstOther.ErrorValue();
            }

            if (allSuccess && !secondOther.IsSuccess())
            {
                allSuccess = false;
                error = secondOther.ErrorValue();
            }

            if (allSuccess && !thirdOther.IsSuccess())
            {
                allSuccess = false;
                error = thirdOther.ErrorValue();
            }

            if (allSuccess && !fourthOther.IsSuccess())
            {
                allSuccess = false;
                error = fourthOther.ErrorValue();
            }

            if (allSuccess)
            {
                return combine(
                    this.SuccessValue(),
                    firstOther.SuccessValue(),
                    secondOther.SuccessValue(),
                    thirdOther.SuccessValue(),
                    fourthOther.SuccessValue());
            }

            return Result<TError, TSuccessOut>.Error(error);
        }

        /// <summary>
        /// Downcast to an <see cref="Option<TSuccess>"/>. When result state <see cref="TError"/> it will cast to <see cref="None"/>.
        /// </summary>
        /// <returns></returns>
        public Option<TSuccess> ToOption()
            => Match(
                error => Option<TSuccess>.None(),
                success => Option<TSuccess>.Some(success.Value));
    }
}
