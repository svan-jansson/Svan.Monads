using System;

namespace Svan.Monads
{
    /// <summary>
    /// A discriminated union base class that holds either a <typeparamref name="TLeft"/> or a <typeparamref name="TRight"/> value.
    /// </summary>
    public class Union<TLeft, TRight>
    {
        private readonly object _value;

        protected Union(TLeft value)
        {
            _value = value!;
            IsLeft = true;
        }

        protected Union(TRight value)
        {
            _value = value!;
            IsLeft = false;
        }

        internal bool IsLeft { get; }

        internal bool IsRight => !IsLeft;

        internal TLeft AsLeft => IsLeft
            ? (TLeft)_value
            : throw new InvalidOperationException("Cannot access Left when value is Right.");

        internal TRight AsRight => !IsLeft
            ? (TRight)_value
            : throw new InvalidOperationException("Cannot access Right when value is Left.");

        internal TOut Match<TOut>(Func<TLeft, TOut> f0, Func<TRight, TOut> f1)
            => IsLeft ? f0((TLeft)_value) : f1((TRight)_value);

        public static implicit operator Union<TLeft, TRight>(TLeft value) => new (value);
        public static implicit operator Union<TLeft, TRight>(TRight value) => new (value);
    }
}
