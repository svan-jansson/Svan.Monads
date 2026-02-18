using System;

namespace Svan.Monads
{
    public readonly struct Left<T>(T value)
    {
        public T Value { get; } = value;
    }

    public readonly struct Right<T>(T value)
    {
        public T Value { get; } = value;
    }

    /// <summary>
    /// A discriminated union base class that holds either a <typeparamref name="TLeft"/> or a <typeparamref name="TRight"/> value.
    /// </summary>
    public class Union<TLeft, TRight>
    {
        private readonly object _value;

        protected Union(Left<TLeft> value) { _value = value; }
        protected Union(Right<TRight> value) { _value = value; }

        internal bool IsLeft => _value is Left<TLeft>;

        internal bool IsRight => _value is Right<TRight>;

        internal TLeft AsLeft => _value is Left<TLeft> left
            ? left.Value
            : throw new NullReferenceException("Cannot access Left when value is Right.");

        internal TRight AsRight => _value is Right<TRight> right
            ? right.Value
            : throw new NullReferenceException("Cannot access Right when value is Left.");

        internal TOut Match<TOut>(Func<TLeft, TOut> f0, Func<TRight, TOut> f1)
            => _value switch
            {
                Left<TLeft> left => f0(left.Value),
                Right<TRight> right => f1(right.Value),
                _ => throw new InvalidOperationException("Union is in an invalid state.")
            };

        public static implicit operator Union<TLeft, TRight>(Left<TLeft> value) => new(value);
        public static implicit operator Union<TLeft, TRight>(Right<TRight> value) => new(value);
    }
}
