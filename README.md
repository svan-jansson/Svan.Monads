[![Build, Test & Publish](https://github.com/svan-jansson/Svan.Monads/actions/workflows/build-test-publish.yml/badge.svg)](https://github.com/svan-jansson/Svan.Monads/actions/workflows/build-test-publish.yml) [![Nuget](https://img.shields.io/nuget/v/Svan.Monads)](https://www.nuget.org/packages/Svan.Monads/)

# Svan.Monads

This library adds some common monads to C#, enabling "Railway Oriented Programming" and functional programming styles. It includes the `Option<T>`, `Result<TError, TSuccess>`, and `Try<TSuccess>` monads, along with extension methods for fluent chaining, async, and error handling.

## Installation

```bash
dotnet add package Svan.Monads
```

## Code Examples

- [Using the Option monad to manipulate a stream of integers](examples/option-examples/Program.cs)
- [Using the Result monad to compose data from different API calls](examples/result-examples/Program.cs)
- [Using the Result monad with a OneOf type as error](examples/esoteric-examples/Program.cs)

## Breaking changes in version 2.x

The library no longer depends on `OneOf`. The monads now inherit from a built-in `Union<TLeft, TRight>` base class that is discriminated using `Left<T>` and `Right<T>` wrapper types.

### `OneOfBase` is no longer the base class

The `Match` and `Switch` methods from OneOf (which received wrapper types like `Error<T>`, `Success<T>`, `Some<T>`) are replaced by `Fold`, which receives the unwrapped values directly.

```diff
  // Option: Match/Switch took None and Some<T>
- option.Match(
-     none => "nothing",
-     some => $"got {some.Value}");
+ option.Fold(
+     () => "nothing",
+     value => $"got {value}");

  // Option: void side-effects used Switch
- option.Switch(
-     none => Console.WriteLine("nothing"),
-     some => Console.WriteLine(some.Value));
+ option.Do(value => Console.WriteLine(value))
+       .DoIfNone(() => Console.WriteLine("nothing"));

  // Result: Match/Switch took Error<TError> and Success<TSuccess>
- result.Match(
-     error => $"failed: {error.Value}",
-     success => $"got {success.Value}");
+ result.Fold(
+     error => $"failed: {error}",
+     success => $"got {success}");

  // Result: void side-effects used Switch
- result.Switch(
-     error => Console.WriteLine(error.Value),
-     success => Console.WriteLine(success.Value));
+ result.Do(success => Console.WriteLine(success))
+       .DoIfError(error => Console.WriteLine(error));
```

### `Error<T>`, `Success<T>` and `Some<T>` have been removed

These OneOf wrapper types are no longer needed. `Union` now uses `Left<T>` and `Right<T>` internally for discrimination, and all public APIs work with the unwrapped values directly.

```diff
- Option<int> option = new Some<int>(42);
+ Option<int> option = Option<int>.Some(42);

- Result<string, int> result = new Success<int>(42);
- Result<string, int> error = new Error<string>("fail");
+ var result = Result<string, int>.Success(42);
+ var error = Result<string, int>.Error("fail");
```

### `Result` and `Try` no longer have implicit conversions

Construction now uses explicit factory methods instead of implicit operators.

```diff
- Result<string, int> result = 42;
- Result<string, int> error = "something went wrong";
+ var result = Result<string, int>.Success(42);
+ var error = Result<string, int>.Error("something went wrong");

- Try<int> tried = 42;
- Try<int> failed = new Exception("boom");
+ var tried = Try<int>.Success(42);
+ var failed = Try<int>.Exception(new Exception("boom"));
```

## The Option Monad

The `Option<T>` monad represents a value that may or may not exist. It is modeled after [F#'s Option Type](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/options) and is functionally similar to [Haskell's Maybe Monad](https://wiki.haskell.org/Maybe).

### Constructing

```csharp
// Using factory methods
var some = Option<int>.Some(42);
var none = Option<int>.None();

// Using the non-generic static class
var some2 = Option.Some(42);
var none2 = Option.None<int>();

// Using the extension method (null becomes None for reference types)
var fromValue = 42.ToOption();
```

### Monadic operations

```csharp
var option = Option.Some(42);

var result = option
    .Filter(i => i > 10)
    .Map(i => i * 2)
    .Bind(i => i % 2 == 0
        ? Option<string>.Some($"even: {i}")
        : Option<string>.None())
    .Do(Console.WriteLine);
```

### Unboxing

```csharp
var option = Option.Some(42);

// Fold with handlers for both cases
string message = option.Fold(
    () => "nothing here",
    value => $"got {value}");

// Get the value or a default
int value = option.DefaultWith(() => -1);

// Get the value or throw
int certain = option.OrThrow();

// Convert to Result
Result<string, int> result = option.ToResult(() => "was none");
```

## The Result Monad

The `Result<TError, TSuccess>` monad represents an operation that can either succeed with a `TSuccess` value or fail with a `TError` value. It enables railway-oriented error handling and readable data transformation pipelines.

### Constructing

```csharp
// Using factory methods
var success = Result<string, int>.Success(42);
var error = Result<string, int>.Error("something went wrong");

// Using the non-generic static class
var success2 = Result.Success<string, int>(42);
var error2 = Result.Error<string, int>("something went wrong");

// Using extension methods
var success3 = 42.ToSuccess<string, int>();
var error3 = "something went wrong".ToError<string, int>();
```

### Monadic operations

```csharp
Result<string, int> Divide(int number, int by)
    => by == 0
        ? Result<string, int>.Error("division by zero")
        : Result<string, int>.Success(number / by);

var result = Divide(12, 2)
    .Bind(n => Divide(n, 2))
    .Map(n => n * 2)
    .MapError(e => e.ToUpper())
    .Do(n => Console.WriteLine($"Result: {n}"))
    .DoIfError(e => Console.WriteLine($"Error: {e}"));
```

### Unboxing

```csharp
var result = Result<string, int>.Success(42);

// Fold with handlers for both cases
string message = result.Fold(
    error => $"failed: {error}",
    success => $"got {success}");

// Get the value or a default derived from the error
int value = result.DefaultWith(error => -1);

// Get the value or throw
int certain = result.OrThrow();

// Convert to Option (error becomes None)
Option<int> option = result.ToOption();
```

## The Try Monad

The `Try<TSuccess>` monad is a specialization of `Result<Exception, TSuccess>` that catches exceptions automatically via its `Try.Catching` constructor.

### Constructing

```csharp
// Catching exceptions automatically
var tried = Try.Catching(() => int.Parse("42"));
var failed = Try.Catching<int>(() => throw new Exception("boom"));

// Using factory methods
var success = Try<int>.Success(42);
var error = Try<int>.Exception(new InvalidOperationException("oops"));

// Using the non-generic static class
var success2 = Try.Success(42);
var error2 = Try.Exception<int>(new InvalidOperationException("oops"));

// Using extension methods
var success3 = 42.ToSuccess<int>();
var error3 = new InvalidOperationException("oops").ToException<int>();
```

### Monadic operations

```csharp
var result = Try.Catching(() => "42")
    .Map(int.Parse)
    .Bind(n => n > 0
        ? Try<int>.Success(n)
        : Try<int>.Exception(new Exception("must be positive")))
    .Do(n => Console.WriteLine($"Result: {n}"))
    .DoIfError(ex => Console.WriteLine($"Error: {ex.Message}"));

// MapCatching and BindCatching catch exceptions thrown by the callback
var safe = Try.Catching(() => "hello")
    .MapCatching(s => int.Parse(s))
    .BindCatching(n => Try<string>.Success(n.ToString()));
```

### Unboxing

`Try<TSuccess>` inherits all unboxing methods from `Result<Exception, TSuccess>`:

```csharp
var tried = Try.Catching(() => int.Parse("42"));

string message = tried.Fold(
    ex => $"failed: {ex.Message}",
    value => $"got {value}");

int value = tried.DefaultWith(ex => -1);
int certain = tried.OrThrow();
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
