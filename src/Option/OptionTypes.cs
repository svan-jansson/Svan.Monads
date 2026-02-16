namespace Svan.Monads;

/// <summary>
/// Represents some value of type T
/// </summary>
public readonly struct Some<T>(T value)
{
    public T Value { get; } = value;
}

/// <summary>
/// Represents no value
/// </summary>
public readonly struct None;