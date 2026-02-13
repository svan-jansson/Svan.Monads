using System;
using OneOf.Types;
using OneOf;

namespace Svan.Monads
{
    /// <summary>
    /// Factory for creating <c>Try&lt;TSuccess&gt;</c> instances by catching exceptions.
    /// </summary>
    public static class Try
    {
        /// <summary>
        /// Execute <paramref name="codeBlock"/> and return its result as <c>Success</c>.
        /// If the code block throws, the exception is caught and returned as the error state.
        /// </summary>
        public static Try<TSuccess> Catching<TSuccess>(Func<TSuccess> codeBlock)
        {
            try
            {
                return codeBlock();
            }
            catch (Exception ex)
            {
                return ex;
            }
        }
    }

    /// <summary>
    /// A specialization of <c>Result&lt;Exception, TSuccess&gt;</c> that provides exception-catching operations.
    /// </summary>
    public class Try<TSuccess> : Result<Exception, TSuccess>
    {
        public Try(OneOf<Error<Exception>, Success<TSuccess>> _) : base(_) { }
        public static implicit operator Try<TSuccess>(Error<Exception> _) => new Try<TSuccess>(_);
        public static implicit operator Try<TSuccess>(Success<TSuccess> _) => new Try<TSuccess>(_);
        public static implicit operator Try<TSuccess>(TSuccess _) => new Success<TSuccess>(_);
        public static implicit operator Try<TSuccess>(Exception _) => new Error<Exception>(_);

        /// <summary>
        /// Upcast to <c>Result&lt;Exception, TSuccess&gt;</c>.
        /// </summary>
        public Result<Exception, TSuccess> ToResult() => this;

        /// <summary>
        /// Map the success value using a mapping function, catching any exception thrown by the mapper.
        /// </summary>
        public Try<TOut> MapCatching<TOut>(Func<TSuccess, TOut> mapper)
            => Fold(
                 error => error,
                 success => Try.Catching(() => mapper(success)));

        /// <summary>
        /// Bind using a binder function, catching any exception thrown by the binder.
        /// The binder is only called when <c>Success</c>; otherwise short-circuits with the existing error.
        /// </summary>
        public Try<TOut> BindCatching<TOut>(Func<TSuccess, Try<TOut>> binder)
        {
            if (IsSuccess())
            {
                try
                {
                    return binder(SuccessValue());
                }
                catch (Exception ex)
                {
                    return ex;
                }
            }
            return ErrorValue();
        }

        /// <summary>
        /// Bind using a binder function.
        /// The binder is only called when <c>Success</c>; otherwise short-circuits with the existing error.
        /// Exceptions thrown by the binder will propagate. Use <see cref="BindCatching{TOut}"/> to catch them instead.
        /// </summary>
        public new Try<TOut> Bind<TOut>(Func<TSuccess, Try<TOut>> binder)
        {
            if (IsSuccess())
                return binder(SuccessValue());
            return ErrorValue();
        }

        /// <summary>
        /// Map the success value to a new type using a mapping function.
        /// The mapper is only called when <c>Success</c>; otherwise short-circuits with the existing error.
        /// Exceptions thrown by the mapper will propagate. Use <see cref="MapCatching{TOut}"/> to catch them instead.
        /// </summary>
        public new Try<TOut> Map<TOut>(Func<TSuccess, TOut> mapper)
        {
            if (IsSuccess())
                return mapper(SuccessValue());
            return ErrorValue();
        }

        /// <summary>
        /// Do let's you fire and forget an action that is executed only when the value is <see cref="TSuccess"/>
        /// </summary>
        /// <param name="do">An action that takes a single parameter of <see cref="TSuccess"/></param>
        /// <returns>The current state of the Result</returns>
        public new Try<TSuccess> Do(Action<TSuccess> @do)
        {
            if (IsSuccess())
                @do(SuccessValue());
            return this;
        }

        /// <summary>
        /// Do let's you fire and forget an action that is executed only when the value is <see cref="TError"/>
        /// </summary>
        /// <param name="do">An action that takes a single parameter of <see cref="TError"/></param>
        /// <returns>The current state of the Result</returns>
        public new Try<TSuccess> DoIfError(Action<Exception> @do)
        {
            if (IsError())
                @do(ErrorValue());
            return this;
        }

        /// <summary>
        /// Combine several results into a new result of <c>TSuccessOut</c> or <c>TError</c> if any of the provided results has an error
        /// </summary>
        public new Try<TSuccessOut> Zip<TSuccessOut, TSuccessOther>(
            Try<TSuccessOther> other,
            Func<TSuccess, TSuccessOther, TSuccessOut> combine)
        {
            var result = base.Zip(other, combine);
            if (result.IsSuccess())
                return result.SuccessValue();
            return result.ErrorValue();
        }

        /// <summary>
        /// Combine several results into a new result of <c>TSuccessOut</c> or <c>TError</c> if any of the provided results has an error
        /// </summary>
        public new Try<TSuccessOut> Zip<TSuccessOut, TSuccessFirstOther, TSuccessSecondOther>(
            Try<TSuccessFirstOther> firstOther,
            Try<TSuccessSecondOther> secondOther,
            Func<TSuccess, TSuccessFirstOther, TSuccessSecondOther, TSuccessOut> combine)
        {
            var result = base.Zip(firstOther, secondOther, combine);
            if (result.IsSuccess())
                return result.SuccessValue();
            return result.ErrorValue();
        }

        /// <summary>
        /// Combine several results into a new result of <c>TSuccessOut</c> or <c>TError</c> if any of the provided results has an error
        /// </summary>
        public new Try<TSuccessOut> Zip<TSuccessOut, TSuccessFirstOther, TSuccessSecondOther, TSuccessThirdOther>(
            Try<TSuccessFirstOther> firstOther,
            Try<TSuccessSecondOther> secondOther,
            Try<TSuccessThirdOther> thirdOther,
            Func<TSuccess, TSuccessFirstOther, TSuccessSecondOther, TSuccessThirdOther, TSuccessOut> combine)
        {
            var result = base.Zip(firstOther, secondOther, thirdOther, combine);
            if (result.IsSuccess())
                return result.SuccessValue();
            return result.ErrorValue();
        }

        /// <summary>
        /// Combine several results into a new result of <c>TSuccessOut</c> or <c>TError</c> if any of the provided results has an error
        /// </summary>
        public new Try<TSuccessOut> Zip<
            TSuccessOut,
            TSuccessFirstOther,
            TSuccessSecondOther,
            TSuccessThirdOther,
            TSuccessFourthOther>(
            Try<TSuccessFirstOther> firstOther,
            Try<TSuccessSecondOther> secondOther,
            Try<TSuccessThirdOther> thirdOther,
            Try<TSuccessFourthOther> fourthOther,
            Func<
                TSuccess,
                TSuccessFirstOther,
                TSuccessSecondOther,
                TSuccessThirdOther,
                TSuccessFourthOther,
                TSuccessOut> combine)
        {
            var result = base.Zip(firstOther, secondOther, thirdOther, fourthOther, combine);
            if (result.IsSuccess())
                return result.SuccessValue();
            return result.ErrorValue();
        }
    }
}
