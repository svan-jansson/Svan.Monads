using System;

namespace Svan.Monads
{
    /// <summary>
    /// Union of <typeparamref name="TError"/> and <typeparamref name="TSuccess"/> with monad features for railway-oriented error handling.
    /// </summary>
    /// <example>
    /// <code>
    /// Result&lt;string, int&gt; Divide(int a, int b) =>
    ///     b == 0 ? Result&lt;string, int&gt;.Error("division by zero") : Result&lt;string, int&gt;.Success(a / b);
    ///
    /// var result = Divide(10, 2)
    ///     .Bind(n => Divide(n, 1))
    ///     .Map(n => n * 3)
    ///     .DefaultWith(_ => 0);
    /// // result == 15
    /// </code>
    /// </example>
    public class Result<TError, TSuccess> : Union<TError, TSuccess>
    {
        internal Result(Left<TError> value) : base(value) { }
        internal Result(Right<TSuccess> value) : base(value) { }

        /// <summary>Creates a <typeparamref name="TError"/> result with <paramref name="value"/>.</summary>
        public static Result<TError, TSuccess> Error(TError value) => new(new Left<TError>(value));
        /// <summary>Creates a <typeparamref name="TSuccess"/> result with <paramref name="value"/>.</summary>
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
        /// Bind the result to a new <see cref="Result{TError, TSuccess}"/> using a binder function.
        /// The binder is only called when <c>Success</c>; otherwise short-circuits with the existing error.
        /// </summary>
        /// <example>
        /// <code>
        /// Result&lt;string, int&gt; Halve(int n) =>
        ///     n % 2 == 0 ? Result&lt;string, int&gt;.Success(n / 2) : Result&lt;string, int&gt;.Error("odd");
        ///
        /// var result  = Result&lt;string, int&gt;.Success(10).Bind(Halve); // Success(5)
        /// var error   = Result&lt;string, int&gt;.Success(3).Bind(Halve);  // Error("odd")
        /// var skipped = Result&lt;string, int&gt;.Error("fail").Bind(Halve); // Error("fail") — binder not called
        /// </code>
        /// </example>
        public Result<TError, TOut> Bind<TOut>(Func<TSuccess, Result<TError, TOut>> binder)
            => Match(Result<TError, TOut>.Error, binder);

        /// <summary>
        /// Bind the error to a new <see cref="Result{TError, TSuccess}"/> using a binder function.
        /// The binder is only called when <c>Error</c>; otherwise short-circuits with the existing success value.
        /// </summary>
        public Result<TOut, TSuccess> BindError<TOut>(Func<TError, Result<TOut, TSuccess>> binder)
            => Match(binder, Result<TOut, TSuccess>.Success);

        /// <summary>
        /// Recover from <typeparamref name="TError"/> by providing a <typeparamref name="TSuccess"/> or a new error <typeparamref name="TOut"/>.
        /// </summary>
        /// <example>
        /// <code>
        /// var recovered = Result&lt;int, int&gt;.Error(0)
        ///     .Recover(e => Result&lt;string, int&gt;.Success(e + 1)); // Success(1)
        ///
        /// var reErrored = Result&lt;int, int&gt;.Error(0)
        ///     .Recover(e => Result&lt;string, int&gt;.Error("still bad")); // Error("still bad")
        /// </code>
        /// </example>
        public Result<TOut, TSuccess> Recover<TOut>(Func<TError, Result<TOut, TSuccess>> recover)
            => BindError(recover);

        /// <summary>
        /// Map the success value to a new type using a mapping function.
        /// The mapper is only called when <c>Success</c>; otherwise short-circuits with the existing error.
        /// </summary>
        /// <example>
        /// <code>
        /// var doubled = Result&lt;string, int&gt;.Success(21).Map(x => x * 2); // Success(42)
        /// var err     = Result&lt;string, int&gt;.Error("fail").Map(x => x * 2); // Error("fail")
        /// </code>
        /// </example>
        public Result<TError, TOut> Map<TOut>(Func<TSuccess, TOut> mapSuccess)
            => Match(
                Result<TError, TOut>.Error,
                success => Result<TError, TOut>.Success(mapSuccess(success)));

        /// <summary>
        /// Map the error value to a new type using a mapping function.
        /// The mapper is only called when <c>Error</c>; otherwise short-circuits with the existing success value.
        /// </summary>
        /// <example>
        /// <code>
        /// var upper   = Result&lt;string, int&gt;.Error("fail").MapError(e => e.ToUpper()); // Error("FAIL")
        /// var success = Result&lt;string, int&gt;.Success(42).MapError(e => e.ToUpper());   // Success(42)
        /// </code>
        /// </example>
        public Result<TOut, TSuccess> MapError<TOut>(Func<TError, TOut> mapError)
            => Match(
                error => Result<TOut, TSuccess>.Error(mapError(error)),
                Result<TOut, TSuccess>.Success);

        /// <summary>
        /// Do lets you fire and forget an action that is executed only when the value is <typeparamref name="TSuccess"/>
        /// </summary>
        /// <param name="do">An action that takes a single parameter of <typeparamref name="TSuccess"/></param>
        /// <returns>The current state of the Result</returns>
        /// <example>
        /// <code>
        /// Result&lt;string, int&gt;.Success(42).Do(n => Console.WriteLine(n));    // prints 42
        /// Result&lt;string, int&gt;.Error("fail").Do(n => Console.WriteLine(n)); // nothing printed
        /// </code>
        /// </example>
        public Result<TError, TSuccess> Do(Action<TSuccess> @do)
        {
            if (IsSuccess())
            {
                @do(SuccessValue());
            }

            return this;
        }

        /// <summary>
        /// Do lets you fire and forget an action that is executed only when the value is <typeparamref name="TError"/>
        /// </summary>
        /// <param name="do">An action that takes a single parameter of <typeparamref name="TError"/></param>
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
        /// Get the value of <typeparamref name="TSuccess"/> or a default value from the supplied function.
        /// </summary>
        /// <example>
        /// <code>
        /// var value    = Result&lt;string, int&gt;.Success(42).DefaultWith(_ => 0); // 42
        /// var fallback = Result&lt;string, int&gt;.Error("fail").DefaultWith(_ => 0); // 0
        /// </code>
        /// </example>
        public TSuccess DefaultWith(Func<TError, TSuccess> fallback)
            => Match(fallback, success => success);

        /// <summary>
        /// Get the value of <typeparamref name="TSuccess"/> or a default value from the supplied value.
        /// </summary>
        public TSuccess DefaultWith(TSuccess fallback)
            => Match(_ => fallback, success => success);

        /// <summary>
        /// Get the value of <typeparamref name="TSuccess"/> or throw a <see cref="NullReferenceException"/>.
        /// </summary>
        /// <example>
        /// <code>
        /// var value = Result&lt;string, int&gt;.Success(42).OrThrow(); // 42
        /// Result&lt;string, int&gt;.Error("fail").OrThrow();           // throws InvalidOperationException
        /// </code>
        /// </example>
        public TSuccess OrThrow()
            => Match(
                error => throw new InvalidOperationException($"Expected a successful value of {typeof(TSuccess).Name} but was {error}."),
                success => success);

        /// <summary>
        /// Fold into value of type <typeparamref name="TOut"/> with supplied functions for case <typeparamref name="TError"/> and case <typeparamref name="TSuccess"/>.
        /// </summary>
        /// <example>
        /// <code>
        /// var msg = Result&lt;string, int&gt;.Success(42).Fold(e => $"error: {e}", n => $"got {n}"); // "got 42"
        /// var err = Result&lt;string, int&gt;.Error("oops").Fold(e => $"error: {e}", n => $"got {n}"); // "error: oops"
        /// </code>
        /// </example>
        public TOut Fold<TOut>(Func<TError, TOut> caseError, Func<TSuccess, TOut> caseSuccess)
            => Match(caseError, caseSuccess);

        /// <summary>
        /// Combine several results into a new result of <typeparamref name="TSuccessOut"/> or <typeparamref name="TError"/> if any of the provided results has an error
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
        /// Combine several results into a new result of <typeparamref name="TSuccessOut"/> or <typeparamref name="TError"/> if any of the provided results has an error
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
        /// Combine several results into a new result of <typeparamref name="TSuccessOut"/> or <typeparamref name="TError"/> if any of the provided results has an error
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
        /// Combine several results into a new result of <typeparamref name="TSuccessOut"/> or <typeparamref name="TError"/> if any of the provided results has an error
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
        /// Downcast to an <see cref="Option{T}"/>. When the result is <c>Error</c> it will return <c>None</c>.
        /// </summary>
        /// <example>
        /// <code>
        /// var some = Result&lt;string, int&gt;.Success(42).ToOption(); // Some(42)
        /// var none = Result&lt;string, int&gt;.Error("fail").ToOption(); // None
        /// </code>
        /// </example>
        public Option<TSuccess> ToOption()
            => Match(
                _ => Option<TSuccess>.None(),
                Option<TSuccess>.Some);
    }
}
