namespace Svan.Monads
{
    public static class Either
    {
        public static Either<TLeft, TRight> FromLeft<TLeft, TRight>(TLeft value) => Either<TLeft, TRight>.FromLeft(value);
        public static Either<TLeft, TRight> FromRight<TLeft, TRight>(TRight value) => Either<TLeft, TRight>.FromRight(value);
        public static Either<TLeft, TRight> ToLeft<TLeft, TRight>(this TLeft value) => FromLeft<TLeft, TRight>(value);
        public static Either<TLeft, TRight> ToRight<TLeft, TRight>(this TRight value) => FromRight<TLeft, TRight>(value);
    }
}
