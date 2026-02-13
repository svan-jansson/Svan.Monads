[![Build, Test & Publish](https://github.com/svan-jansson/Svan.Monads/actions/workflows/build-test-publish.yml/badge.svg)](https://github.com/svan-jansson/Svan.Monads/actions/workflows/build-test-publish.yml) [![Nuget](https://img.shields.io/nuget/v/Svan.Monads)](https://www.nuget.org/packages/Svan.Monads/)

# Monads Based on the OneOf Union Type

This library adds common monads, currently based on the [OneOf](https://github.com/mcintyre321/OneOf) union type library.

## Installation

```bash
dotnet add package Svan.Monads
```

## Code Examples

- [Using the Option monad to manipulate a stream of integers](examples/option-examples/Program.cs)
- [Using the Result monad to compose data from different API calls](examples/result-examples/Program.cs)
- [Using the Result monad with a OneOf type as error](examples/esoteric-examples/Program.cs)

## The Option Monad

The `Option<T>` monad extends `OneOf<None, Some<T>>` and is modeled after [F#'s Option Type](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/options). It is functionally similar to [Haskell's Maybe Monad](https://wiki.haskell.org/Maybe).

This monad provides a mechanism for conditional execution in a workflow/pipeline-style manner. Great for readability and error handling without `try`/`catch`.

### Option.Bind Example

`Bind` is used to create a contract that will resolve to `Some<T>` when all checks have passed. It will resolve to `None` if a `Bind` statement returns `None`. All subsequent `Bind` statements will be skipped.

In this example we create a contract that guarantees that a given number is greater than 10 and is even. We then pipe the result into a `Match` to conditionally execute for both cases.

```csharp
using Svan.Monads

Option<int> IsGreaterThan10(int i)
        => i > 10 ? i : new None();

Option<int> IsEven(int i)
    => i % 2 == 0 ? i : new None();

[Theory]
[InlineData(12)]
[InlineData(24)]
void Conditional_execution_when_contract_is_fulfilled(int evenNumber)
{
    var expected = evenNumber;
    Option<int> option = evenNumber;

    var actual = option
                    .Bind(IsGreaterThan10)
                    .Bind(IsEven)
                    .Match(
                        none => 0,
                        some => some.Value);

    Assert.Equal(expected, actual);
}
```

### Option.Map Example

`Map` is used to map a regular value of type `T` to an `Option<T>`. In the example below it is combined with the `Bind` and `Match` functions to apply type and format conversions. For instance it takes the string output of `int.ToString()` and returns an `Option<string>` that can be used to continue the pipeline. `Map` will not execute if the current value is `None`, instead it will simply resolve to `None`, meaning that the pipeline will not break.

```csharp
using Svan.Monads

[Fact]
public void Convert_to_option_type_using_map()
{
    var expected = "~20~";
    Option<int> option = 20;

    var actual = option
                    .Bind(IsGreaterThan10)
                    .Bind(IsEven)
                    .Map(i => i.ToString())
                    .Map(s => $"~{s}~")
                    .Match(
                        none => "could not convert number",
                        some => some.Value);

    Assert.Equal(expected, actual);
}
```

### Option.Filter Example

`Filter` allows you to filter the current value of the option monad by passing a filter function. If the filter function returns `false` it will resolve to `None`. Giving you a convenient way of conditionally controlling a flow.

```csharp
using Svan.Monads

[Fact]
public void Use_filter_to_create_a_conditional_pipeline()
{
    var expected = 0;
    Option<int> option = 20;

    var actual = option
                    .Filter(i => i > 15) // True
                    .Filter(i => i > 20) // False
                    .Filter(i => i > 30) // Skipped
                    .Match(
                        none => 0,
                        some => some.Value);

    Assert.Equal(expected, actual);
}
```

## The Result Monad

The `Result<TError, TSuccess>` monad is similar to the `Option<T>` monad, but it also defines a value for the negative case, expressed as `TError`. It also has the `Unwrap` function that will return the encapsulated value or a provdided fallback. This monad can help you create readable data transformation pipelines and monadic error handling.

### Result.Bind Example

Here is an example of a control flow that uses `Bind` in combination with `Map` and `Switch`. `Bind` can be chained and will not execute if the previous step returns the error case `Error<TError>`.

```csharp
const string ErrorMessage = "division by zero";
private Result<string, int> Divide(int number, int by)
    => by == 0
        ? new Error<string>(ErrorMessage)
        : new Success<int>(number / by);

[Fact]
public void Success_and_error_both_have_values()
{
    var expectedSuccess = (12 / 2 / 2) * 2;
    var expectedError = ErrorMessage;

    Action<int> doMath = (divideBy)
        => Divide(12, divideBy)
            .Bind(result => Divide(result, 2))
            .Map(result => result * 2)
            .Switch(
                error => throw new DivideByZeroException(error.Value),
                success => Assert.Equal(expectedSuccess, success.Value));

    doMath(2);

    var exception = Record.Exception(() => doMath(0));
    Assert.IsType<DivideByZeroException>(exception);
    Assert.Equal(expectedError, exception.Message);
}
```

### Result.DefaultWith Example

This example demonstrates both how to define a fallback function and how to use the `TError` value, to provide logic on failure.

```csharp
[Fact]
public void DefaultWith_lets_you_define_fallback_values()
{
    var maxLimitException = new Exception();
    maxLimitException.Data.Add("max", 25);

    Func<int, Result<Exception, int>> add5 = (val) => new Success<int>(val + 5);
    Func<int, Result<Exception, int>> checkIsBelow25 = (val) =>
            val > 25
            ? new Error<Exception>(maxLimitException)
            : new Success<int>(val);

    Func<int, int> add10ReturnMax25 = (start)
        => Result<Exception, int>.Success(start)
            .Bind(add5)
            .Bind(add5)
            .Bind(checkIsBelow25)
            .DefaultWith(exception => (int)exception.Data["max"]);

    Assert.Equal(20, add10ReturnMax25(10));
    Assert.Equal(25, add10ReturnMax25(15));
    Assert.Equal(25, add10ReturnMax25(20));
}
```

## The Try Monad

The `Try<TSuccess>` monad implements `Result<Exception, TSuccess>` together with a `Catching` constructor.

```csharp
public void Try_can_wrap_caught_exceptions()
{
    Try.Catching<string>(() =>
        {
            throw new Exception("Error that should be caught");
        })
        .DoIfError((actual) => Assert.IsType<Exception>(actual))
        .Do((_) => Assert.False(true));

    Try.Catching(() => "a string")
        .DoIfError((_) => Assert.True(false))
        .Do((actual) => Assert.Equal("a string", actual));
}
```

## Async Support

Support of async workflows is added as a small set of extension methods. Sync operations work naturally after an `await`, and only callbacks that are themselves async need async-specific methods.

### Awaiting then chaining sync operations

When an async function returns an `Option<T>`, `Result<TError, T>`, or `Try<T>`, you can `await` it and then chain any sync operation as usual:

```csharp
async Task<Option<int>> FindUserId(string username) { ... }

var result = (await FindUserId("malin"))
    .Map(id => id * 2)
    .Filter(id => id > 0)
    .DefaultWith(() => -1);
```

### BindAsync and MapAsync

When the callback itself is async, `BindAsync` and `MapAsync` enable fluent chaining without intermediate awaits:

```csharp
async Task<Option<string>> FindUserEmail(int userId) { ... }
async Task<string> NormalizeEmail(string email) { ... }

var result = await FindUserId("malin")
    .BindAsync(id => FindUserEmail(id))
    .MapAsync(email => NormalizeEmail(email));
```

These work the same way on `Result` and `Try`:

```csharp
async Task<Result<string, int>> ParseUserId(string input) { ... }
async Task<Result<string, string>> LookupUsername(int userId) { ... }

var greeting = (await ParseUserId("42")
        .BindAsync(id => LookupUsername(id)))
    .Map(name => $"Welcome, {name}!")
    .DefaultWith(error => $"Error: {error}");
```

### Sequence

When you have a sync monad and `Map` it with an async function, you get e.g. `Option<Task<T>>`. `Sequence` flips this into `Task<Option<T>>` so you can `await` it:

```csharp
Option<string> email = Option<string>.Some("  ALICE@EXAMPLE.COM  ");

var result = await email
    .Map(e => NormalizeEmail(e))
    .Sequence();
```

This also works on `Result` and `Try`, skipping the async work when in the error/none state.
