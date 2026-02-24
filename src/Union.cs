using System;

namespace Svan.Monads
{
    /// <summary>Wrapper that marks a value as the left case of a <see cref="Union{TLeft,TRight}"/>.</summary>
    public readonly struct Left<T>(T value)
    {
        /// <summary>The wrapped left value.</summary>
        public T Value { get; } = value;
    }

    /// <summary>Wrapper that marks a value as the right case of a <see cref="Union{TLeft,TRight}"/>.</summary>
    public readonly struct Right<T>(T value)
    {
        /// <summary>The wrapped right value.</summary>
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

        /// <summary>Initializes the union with a <typeparamref name="TLeft"/> value.</summary>
        protected Union(Left<TLeft> value)
        {
            _isLeft = true;
            _left = value.Value;
            _right = default!;
        }

        /// <summary>Initializes the union with a <typeparamref name="TRight"/> value.</summary>
        protected Union(Right<TRight> value)
        {
            _isLeft = false;
            _left = default!;
            _right = value.Value;
        }

        /// <summary><see langword="true"/> when the union holds a <typeparamref name="TLeft"/> value.</summary>
        public bool IsLeft => _isLeft;

        /// <summary><see langword="true"/> when the union holds a <typeparamref name="TRight"/> value.</summary>
        public bool IsRight => !_isLeft;

        /// <summary>Returns the <typeparamref name="TLeft"/> value. Throws if the union holds a <typeparamref name="TRight"/> value.</summary>
        protected TLeft AsLeft => _isLeft
            ? _left
            : throw new NullReferenceException("Cannot access Left when value is Right.");

        /// <summary>Returns the <typeparamref name="TRight"/> value. Throws if the union holds a <typeparamref name="TLeft"/> value.</summary>
        protected TRight AsRight => !_isLeft
            ? _right
            : throw new NullReferenceException("Cannot access Right when value is Left.");

        /// <summary>Projects the union to <typeparamref name="TOut"/> by applying <paramref name="f0"/> for the left case or <paramref name="f1"/> for the right case.</summary>
        public TOut Match<TOut>(Func<TLeft, TOut> f0, Func<TRight, TOut> f1)
            => _isLeft ? f0(_left) : f1(_right);

        /// <summary>Implicitly wraps a <see cref="Left{TLeft}"/> as a <see cref="Union{TLeft,TRight}"/>.</summary>
        public static implicit operator Union<TLeft, TRight>(Left<TLeft> value) => new(value);
        /// <summary>Implicitly wraps a <see cref="Right{TRight}"/> as a <see cref="Union{TLeft,TRight}"/>.</summary>
        public static implicit operator Union<TLeft, TRight>(Right<TRight> value) => new(value);
    }
}
