using System;
using Xunit;

namespace Svan.Monads.UnitTests;

public class UnionTests
{
    [Fact]
    public void IsLeft_is_true_when_constructed_with_left()
    {
        Union<int, string> union = new Left<int>(42);

        Assert.True(union.IsLeft);
        Assert.False(union.IsRight);
    }

    [Fact]
    public void IsRight_is_true_when_constructed_with_right()
    {
        Union<int, string> union = new Right<string>("hello");

        Assert.True(union.IsRight);
        Assert.False(union.IsLeft);
    }

    [Fact]
    public void Match_invokes_left_function_when_left()
    {
        Union<int, string> union = new Left<int>(42);

        var result = union.Match(left => left * 2, right => -1);

        Assert.Equal(84, result);
    }

    [Fact]
    public void Match_invokes_right_function_when_right()
    {
        Union<int, string> union = new Right<string>("hello");

        var result = union.Match(left => "left", right => right.ToUpper());

        Assert.Equal("HELLO", result);
    }

    [Fact]
    public void Match_does_not_invoke_right_function_when_left()
    {
        Union<int, string> union = new Left<int>(1);

        union.Match<int>(
            left => left,
            right => throw new InvalidOperationException("should not be called"));
    }

    [Fact]
    public void Match_does_not_invoke_left_function_when_right()
    {
        Union<int, string> union = new Right<string>("x");

        union.Match<string>(
            left => throw new InvalidOperationException("should not be called"),
            right => right);
    }

    [Fact]
    public void Left_value_is_preserved_through_match()
    {
        var expected = new object();
        Union<object, string> union = new Left<object>(expected);

        var actual = union.Match(left => left, _ => new object());

        Assert.Same(expected, actual);
    }

    [Fact]
    public void Right_value_is_preserved_through_match()
    {
        var expected = new object();
        Union<string, object> union = new Right<object>(expected);

        var actual = union.Match(_ => new object(), right => right);

        Assert.Same(expected, actual);
    }
}
