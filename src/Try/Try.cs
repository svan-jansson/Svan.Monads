using System;

namespace Svan.Monads
{
    /// <summary>
    /// A specialization of <see cref="Result{TError, TSuccess}"/> that provides exception-catching operations.
    /// </summary>
    /// <example>
    /// <code>
    /// var result = Try.Catching(() => int.Parse("42"))
    ///     .MapCatching(n => n * 2)
    ///     .DefaultWith(_ => 0);
    /// // result == 84
    ///
    /// var failed = Try.Catching(() => int.Parse("abc"))
    ///     .MapCatching(n => n * 2)
    ///     .DefaultWith(_ => 0);
    /// // failed == 0 (FormatException caught at each step)
    /// </code>
    /// </example>
    public class Try<TSuccess> : Result<Exception, TSuccess>
    {
        internal Try(Left<Exception> value) : base(value) { }
        internal Try(Right<TSuccess> value) : base(value) { }

        /// <summary>Creates a <c>Try</c> in the error state wrapping <paramref name="value"/>.</summary>
        public static Try<TSuccess> Exception(Exception value) => new(new Left<Exception>(value));
        /// <summary>Creates a <c>Try</c> in the success state wrapping <paramref name="value"/>.</summary>
        public new static Try<TSuccess> Success(TSuccess value) => new(new Right<TSuccess>(value));

        /// <summary>
        /// Upcast to <see cref="Result{TError, TSuccess}"/>.
        /// </summary>
        public Result<Exception, TSuccess> ToResult() => this;

        /// <summary>
        /// Map the success value using a mapping function, catching any exception thrown by the mapper.
        /// </summary>
        /// <example>
        /// <code>
        /// var result = Try.Catching(() => "42").MapCatching(int.Parse); // Success(42)
        /// var failed = Try.Catching(() => "abc").MapCatching(int.Parse); // Exception(FormatException)
        /// // Unlike Map, exceptions thrown by the mapper are caught rather than propagated.
        /// </code>
        /// </example>
        public Try<TOut> MapCatching<TOut>(Func<TSuccess, TOut> mapper)
            => Fold<Try<TOut>>(
                 Try.Exception<TOut>,
                 success => Try.Catching(() => mapper(success)));

        /// <summary>
        /// Bind using a binder function, catching any exception thrown by the binder.
        /// The binder is only called when <c>Success</c>; otherwise short-circuits with the existing error.
        /// </summary>
        /// <example>
        /// <code>
        /// Try&lt;int&gt; ParsePositive(string s)
        /// {
        ///     var n = int.Parse(s); // throws if invalid
        ///     return n > 0 ? Try.Success(n) : throw new ArgumentException("must be positive");
        /// }
        ///
        /// var result = Try.Catching(() => "42").BindCatching(ParsePositive); // Success(42)
        /// var failed = Try.Catching(() => "abc").BindCatching(ParsePositive); // Exception(FormatException)
        /// </code>
        /// </example>
        public Try<TOut> BindCatching<TOut>(Func<TSuccess, Try<TOut>> binder)
        {
            if (!IsSuccess()) return Try.Exception<TOut>(ErrorValue());
            try
            {
                return binder(SuccessValue());
            }
            catch (Exception ex)
            {
                return Try.Exception<TOut>(ex);
            }
        }

        /// <summary>
        /// Bind using a binder function.
        /// The binder is only called when <c>Success</c>; otherwise short-circuits with the existing error.
        /// Exceptions thrown by the binder will propagate. Use <see cref="BindCatching{TOut}"/> to catch them instead.
        /// </summary>
        public Try<TOut> Bind<TOut>(Func<TSuccess, Try<TOut>> binder)
        {
            return IsSuccess() ? binder(SuccessValue()) : Try.Exception<TOut>(ErrorValue());
        }

        /// <summary>
        /// Map the success value to a new type using a mapping function.
        /// The mapper is only called when <c>Success</c>; otherwise short-circuits with the existing error.
        /// Exceptions thrown by the mapper will propagate. Use <see cref="MapCatching{TOut}"/> to catch them instead.
        /// </summary>
        public new Try<TOut> Map<TOut>(Func<TSuccess, TOut> mapper)
        {
            return IsSuccess() ? Try.Success(mapper(SuccessValue())) : Try.Exception<TOut>(ErrorValue());
        }

        /// <summary>
        /// Do lets you fire and forget an action that is executed only when the value is <typeparamref name="TSuccess"/>
        /// </summary>
        /// <param name="do">An action that takes a single parameter of <typeparamref name="TSuccess"/></param>
        /// <returns>The current state of the Result</returns>
        public new Try<TSuccess> Do(Action<TSuccess> @do)
        {
            if (IsSuccess())
                @do(SuccessValue());
            return this;
        }

        /// <summary>
        /// Do lets you fire and forget an action that is executed only when the value is <see cref="Exception"/>
        /// </summary>
        /// <param name="do">An action that takes a single parameter of <see cref="Exception"/></param>
        /// <returns>The current state of the Result</returns>
        public new Try<TSuccess> DoIfError(Action<Exception> @do)
        {
            if (IsError())
                @do(ErrorValue());
            return this;
        }

        /// <summary>
        /// Combine several results into a new result of <typeparamref name="TSuccessOut"/> or <see cref="Exception"/> if any of the provided results has an error
        /// </summary>
        public Try<TSuccessOut> Zip<TSuccessOut, TSuccessOther>(
            Try<TSuccessOther> other,
            Func<TSuccess, TSuccessOther, TSuccessOut> combine)
        {
            var result = base.Zip(other, combine);
            return result.IsSuccess() ? Try.Success(result.SuccessValue()) : Try.Exception<TSuccessOut>(result.ErrorValue());
        }

        /// <summary>
        /// Combine several results into a new result of <typeparamref name="TSuccessOut"/> or <see cref="Exception"/> if any of the provided results has an error
        /// </summary>
        public Try<TSuccessOut> Zip<TSuccessOut, TSuccessFirstOther, TSuccessSecondOther>(
            Try<TSuccessFirstOther> firstOther,
            Try<TSuccessSecondOther> secondOther,
            Func<TSuccess, TSuccessFirstOther, TSuccessSecondOther, TSuccessOut> combine)
        {
            var result = base.Zip(firstOther, secondOther, combine);
            return result.IsSuccess() ? Try.Success(result.SuccessValue()) : Try.Exception<TSuccessOut>(result.ErrorValue());
        }

        /// <summary>
        /// Combine several results into a new result of <typeparamref name="TSuccessOut"/> or <see cref="Exception"/> if any of the provided results has an error
        /// </summary>
        public Try<TSuccessOut> Zip<TSuccessOut, TSuccessFirstOther, TSuccessSecondOther, TSuccessThirdOther>(
            Try<TSuccessFirstOther> firstOther,
            Try<TSuccessSecondOther> secondOther,
            Try<TSuccessThirdOther> thirdOther,
            Func<TSuccess, TSuccessFirstOther, TSuccessSecondOther, TSuccessThirdOther, TSuccessOut> combine)
        {
            var result = base.Zip(firstOther, secondOther, thirdOther, combine);
            return result.IsSuccess() ? Try.Success(result.SuccessValue()) : Try.Exception<TSuccessOut>(result.ErrorValue());
        }

        /// <summary>
        /// Combine several results into a new result of <typeparamref name="TSuccessOut"/> or <see cref="Exception"/> if any of the provided results has an error
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
        {
            var result = base.Zip(firstOther, secondOther, thirdOther, fourthOther, combine);
            return result.IsSuccess() ? Try.Success(result.SuccessValue()) : Try.Exception<TSuccessOut>(result.ErrorValue());
        }
    }
}
