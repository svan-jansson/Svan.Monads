using System;
using OneOf.Types;
using OneOf;

namespace Svan.Monads
{
    public static class Try
    {
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

    public class Try<TSuccess> : Result<Exception, TSuccess>
    {
        public Try(OneOf<Error<Exception>, Success<TSuccess>> _) : base(_) { }
        public static implicit operator Try<TSuccess>(Error<Exception> _) => new Try<TSuccess>(_);
        public static implicit operator Try<TSuccess>(Success<TSuccess> _) => new Try<TSuccess>(_);
        public static implicit operator Try<TSuccess>(TSuccess _) => new Success<TSuccess>(_);
        public static implicit operator Try<TSuccess>(Exception _) => new Error<Exception>(_);

        public Result<Exception, TSuccess> ToResult() => this;

        public Try<TOut> MapCatching<TOut>(Func<TSuccess, TOut> mapper)
            => Fold(
                 error => error,
                 success => Try.Catching(() => mapper(success)));

        public Try<TOut> Bind<TOut>(Func<TSuccess, Try<TOut>> binder)
            => base.Bind<TOut>(binder) as Try<TOut>;

        public new Try<TOut> Map<TOut>(Func<TSuccess, TOut> mapper)
            => base.Map<TOut>(mapper) as Try<TOut>;


        /// <summary>
        /// Do let's you fire and forget an action that is executed only when the value is <see cref="TSuccess"/> 
        /// </summary>
        /// <param name="do">An action that takes a single parameter of <see cref="TSuccess"/></param>
        /// <returns>The current state of the Result</returns>
        public new Try<TSuccess> Do(Action<TSuccess> @do)
            => base.Do(@do) as Try<TSuccess>;

        /// <summary>
        /// Do let's you fire and forget an action that is executed only when the value is <see cref="TError"/> 
        /// </summary>
        /// <param name="do">An action that takes a single parameter of <see cref="TError"/></param>
        /// <returns>The current state of the Result</returns>
        public new Try<TSuccess> DoIfError(Action<Exception> @do)
            => base.DoIfError(@do) as Try<TSuccess>;


        /// <summary>
        /// Combine several results into a new result of <c>TSuccessOut</c> or <c>TError</c> if any of the provided results has an error
        /// </summary>
        public Try<TSuccessOut> Zip<TSuccessOut, TSuccessOther>(
            Try<TSuccessOther> other,
            Func<TSuccess, TSuccessOther, TSuccessOut> combine)
                => base.Zip(other, combine) as Try<TSuccessOut>;

        /// <summary>
        /// Combine several results into a new result of <c>TSuccessOut</c> or <c>TError</c> if any of the provided results has an error
        /// </summary>
        public Try<TSuccessOut> Zip<TSuccessOut, TSuccessFirstOther, TSuccessSecondOther>(
            Try<TSuccessFirstOther> firstOther,
            Try<TSuccessSecondOther> secondOther,
            Func<TSuccess, TSuccessFirstOther, TSuccessSecondOther, TSuccessOut> combine)
                => base.Zip(firstOther, secondOther, combine) as Try<TSuccessOut>;

        /// <summary>
        /// Combine several results into a new result of <c>TSuccessOut</c> or <c>TError</c> if any of the provided results has an error
        /// </summary>
        public Try<TSuccessOut> Zip<TSuccessOut, TSuccessFirstOther, TSuccessSecondOther, TSuccessThirdOther>(
            Try<TSuccessFirstOther> firstOther,
            Try<TSuccessSecondOther> secondOther,
            Try<TSuccessThirdOther> thirdOther,
            Func<TSuccess, TSuccessFirstOther, TSuccessSecondOther, TSuccessThirdOther, TSuccessOut> combine)
                => base.Zip(firstOther, secondOther, thirdOther, combine) as Try<TSuccessOut>;

        /// <summary>
        /// Combine several results into a new result of <c>TSuccessOut</c> or <c>TError</c> if any of the provided results has an error
        /// </summary>
        public Try<TSuccessOut> Zip<
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
                    => base.Zip(firstOther, secondOther, thirdOther, fourthOther, combine) as Try<TSuccessOut>;
    }
}
