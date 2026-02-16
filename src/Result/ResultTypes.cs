namespace Svan.Monads
{
    /// <summary>
    /// Wrapper representing an error value.
    /// </summary>
    public struct Error<T>
    {
        public T Value { get; }
        public Error(T value)
        {
            Value = value;
        }
    }

    /// <summary>
    /// Wrapper representing a success value.
    /// </summary>
    public struct Success<T>
    {
        public T Value { get; }
        public Success(T value)
        {
            Value = value;
        }
    }
}
