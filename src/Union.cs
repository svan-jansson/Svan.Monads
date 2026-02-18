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
        private readonly bool _isLeft;
        private readonly TLeft _left;
        private readonly TRight _right;

        protected Union(Left<TLeft> value)
        {
            _isLeft = true;
            _left = value.Value;
            _right = default!;
        }

        protected Union(Right<TRight> value)
        {
            _isLeft = false;
            _left = default!;
            _right = value.Value;
        }

        public bool IsLeft => _isLeft;

        public bool IsRight => !_isLeft;

        protected TLeft AsLeft => _isLeft
            ? _left
            : throw new NullReferenceException("Cannot access Left when value is Right.");

        protected TRight AsRight => !_isLeft
            ? _right
            : throw new NullReferenceException("Cannot access Right when value is Left.");

        public TOut Match<TOut>(Func<TLeft, TOut> f0, Func<TRight, TOut> f1)
            => _isLeft ? f0(_left) : f1(_right);

        public static implicit operator Union<TLeft, TRight>(Left<TLeft> value) => new(value);
        public static implicit operator Union<TLeft, TRight>(Right<TRight> value) => new(value);
    }
}
