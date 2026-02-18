using Svan.Monads;
using Xunit;

namespace Svan.Monads.UnitTests;

public class OptionTests
{
    private Option<int> IsGreaterThan10(int i)
    {
        return i > 10 ? Option.Some(i) : Option.None<int>();
    }

    private Option<int> IsEven(int i)
    {
        return i % 2 == 0 ? Option.Some(i) : Option.None<int>();
    }

    [Theory]
    [InlineData(12)]
    [InlineData(24)]
    public void Conditional_execution_when_contract_is_fulfilled(int evenNumber)
    {
        var expected = evenNumber;
        var option = evenNumber.ToOption();

        var actual = option
            .Bind(IsGreaterThan10)
            .Bind(IsEven)
            .Fold(
                () => 0,
                some => some);

        Assert.Equal(expected, actual);
    }

    [Theory]
    [InlineData(5, "above two")]
    [InlineData(1, "below or equal to two")]
    public void Bind_to_different_data_type(int value, string expected)
    {
        var option = value.ToOption();

        var actual = option
            .Bind(i => i > 2
                ? Option<string>.Some("above two")
                : Option<string>.None())
            .Fold(
                () => "below or equal to two",
                some => some);

        Assert.Equal(expected, actual);

    }

    [Theory]
    [InlineData(11)]
    [InlineData(23)]
    public void Conditional_execution_when_contract_is_not_fulfilled(int oddNumber)
    {
        var expected = 0;
        var option = oddNumber.ToOption();

        var actual = option
            .Bind(IsGreaterThan10)
            .Bind(IsEven)
            .Fold(
                () => 0,
                some => some);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Convert_to_option_type_using_map()
    {
        var expected = "~20~";
        var option = 20.ToOption();

        var actual = option
            .Bind(IsGreaterThan10)
            .Bind(IsEven)
            .Map(i => i.ToString())
            .Map(s => $"~{s}~")
            .Fold(
                () => "could not convert number",
                some => some);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Pipeline_does_not_break_on_None()
    {
        var expected = "could not convert number";
        var option = 19.ToOption();

        var actual = option
            .Bind(IsGreaterThan10)
            .Bind(IsEven)
            .Map(i => i.ToString())
            .Map(s => $"~{s}~")
            .Fold(
                () => "could not convert number",
                some => some);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Use_filter_to_get_conditional_result()
    {
        Option<int>.Some(5)
            .Filter(i => i > 0)
            .Filter(i => i % 2 > 0)
            .AssertSome(5);

        Option<int>.Some(4)
            .Filter(i => i > 0)
            .Filter(i => i % 2 > 0)
            .AssertNone();
    }

    [Fact]
    public void Use_Do_to_execute_conditional_actions()
    {
        var option = 5.ToOption();

        option
            .DoIfNone(() => Assert.Fail("this should not be executed"))
            .Do(i => Assert.Equal(5, i));
    }

    [Fact]
    public void Use_DoIfNone_to_execute_conditional_actions()
    {
        var option = Option.None<int>();

        option
            .Do(i => Assert.Fail("this should not be executed"))
            .DoIfNone(() => Assert.True(true, "this should be executed"));
    }

    [Fact]
    public void ToOption_should_return_Some_for_value_types()
    {
        int i = default;
        i.ToOption()
            .AssertSome(default);

        i = 5;
        i.ToOption()
            .AssertSome(5);
    }

    [Fact]
    public void ToOption_should_return_Some_or_None_for_reference_types()
    {
        TestClass t = default;
        t.ToOption()
            .AssertNone();

        t = new TestClass();
        t.ToOption()
            .AssertSome();
    }

    [Fact]
    public void Combine_options_with_zip_when_all_are_some()
    {
        var option1 = 5.ToOption();
        var option2 = 8.ToOption();
        var expected = 13;

        var actual = option1
            .Zip(option2, (value1, value2) => value1 + value2)
            .DefaultWith(() => 0);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Combine_options_with_zip_when_none()
    {
        var option1 = 5.ToOption();
        var option2 = Option<string>.None();
        var expected = "this should happen";

        var actual = option1
            .Zip(option2, (value1, value2) => "this should not happen")
            .DefaultWith(() => "this should happen");

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Combine_five_options_with_zip_when_all_are_some()
    {
        var option1 = 1.ToOption();
        var option2 = 2.ToOption();
        var option3 = 3.ToOption();
        var option4 = 4.ToOption();
        var option5 = 5.ToOption();

        var expected = 1 + 2 + 3 + 4 + 5;

        var actual = option1
            .Zip(
                option2,
                option3,
                option4,
                option5,
                (value1, value2, value3, value4, value5)
                    => value1 + value2 + value3 + value4 + value5)
            .DefaultWith(() => 0);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Merge_can_combine_options()
    {
        var expected = 10 + 20 + 30 + 40 + 50;

        var result = 10.ToOption()
            .Merge(20.ToOption())
            .Merge(30.ToOption())
            .Merge(40.ToOption())
            .Merge(50.ToOption())
            .Fold(
                () => 0,
                (group) => group.Item1 + group.Item2 + group.Item3 + group.Item4 + group.Item5);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void ToResult_converts_an_option_to_a_successful_result()
    {
        var option = Option<int>.Some(111);
        var result = option.ToResult(() => new System.Exception("hello"));

        Assert.Equal(111, result.SuccessValue());
    }

    [Fact]
    public void ToResult_converts_an_option_to_an_error_result()
    {
        var option = Option<int>.None();
        var result = option.ToResult(() => new System.Exception("hello"));

        Assert.Equal("hello", result.ErrorValue().Message);
    }

    [Fact]
    public void OrThrow_throws_A_null_reference_exception()
    {
        var optionNone = Option<int>.None();
        Assert.Throws<NullReferenceException>(() => optionNone.OrThrow());

        var optionSome = Option<int>.Some(1);
        Assert.Equal(1, optionSome.OrThrow());
    }

    [Fact]
    public void Flatten_returns_inner_option_when_some()
    {
        var nested = Option<Option<int>>.Some(Option<int>.Some(42));
        nested.Flatten().AssertSome(42);
    }

    [Fact]
    public void Flatten_returns_none_when_inner_is_none()
    {
        var nested = Option<Option<int>>.Some(Option<int>.None());
        nested.Flatten().AssertNone();
    }

    [Fact]
    public void Flatten_returns_none_when_outer_is_none()
    {
        var nested = Option<Option<int>>.None();
        nested.Flatten().AssertNone();
    }

    class TestClass
    {

    }
}