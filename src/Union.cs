using System;

namespace Svan.Monads
{
    /// <summary>
    /// A discriminated union base class that holds either a <typeparamref name="TLeft"/> or a <typeparamref name="TRight"/> value.
    /// </summary>
    public class Union<TLeft, TRight>
    {
        private readonly object _value;
        private readonly int _index;

        protected Union(TLeft value)
        {
            _value = value!;
            _index = 0;
        }

        protected Union(TRight value)
        {
            _value = value!;
            _index = 1;
        }

        public bool IsLeft => _index == 0;
        public bool IsRight => _index == 1;

        public TLeft AsLeft => _index == 0
            ? (TLeft)_value
            : throw new InvalidOperationException("Cannot access Left when value is Right.");

        public TRight AsRight => _index == 1
            ? (TRight)_value
            : throw new InvalidOperationException("Cannot access Right when value is Left.");

        public TOut Fold<TOut>(Func<TLeft, TOut> f0, Func<TRight, TOut> f1)
            => _index == 0 ? f0((TLeft)_value) : f1((TRight)_value);

        public void Iter(Action<TLeft> f0, Action<TRight> f1)
        {
            if (_index == 0)
                f0((TLeft)_value);
            else
                f1((TRight)_value);
        }

        public static implicit operator Union<TLeft, TRight>(TLeft value) => new (value);
        public static implicit operator Union<TLeft, TRight>(TRight value) => new (value);
    }
}
