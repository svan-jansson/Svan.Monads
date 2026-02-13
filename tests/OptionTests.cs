using Xunit;

namespace Svan.Monads.UnitTests;

public class OptionTests
{
    private Option<int> IsGreaterThan10(int i)
    {
        if (i > 10)
        {
            return i;
        }
        else
        {
            return new None();
        }
    }

    private Option<int> IsEven(int i)
    {
        if (i % 2 == 0)
        {
            return i;
        }
        else
        {
            return new None();
        }
    }

    [Theory]
    [InlineData(12)]
    [InlineData(24)]
    public void Conditional_execution_when_contract_is_fulfilled(int evenNumber)
    {
        var expected = evenNumber;
        Option<int> option = evenNumber;

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
        Option<int> option = value;

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
        Option<int> option = oddNumber;

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
        Option<int> option = 20;

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
        Option<int> option = 19;

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
        var expected = 5;
        Option<int> option = 5;
        var actual = option
            .Filter(i => i > 0)
            .Filter(i => i % 2 > 0)
            .Match(
                none => 0,
                some => some.Value);

        Assert.Equal(expected, actual);

        expected = 0;
        option = Option<int>.Some(4);
        actual = option
            .Filter(i => i > 0)
            .Filter(i => i % 2 > 0)
            .Match(
                none => 0,
                some => some.Value);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Use_Do_to_execute_conditional_actions()
    {
        Option<int> option = 5;

        option
            .DoIfNone(() => Assert.Fail("this should not be executed"))
            .Do(i => Assert.Equal(5, i));
    }

    [Fact]
    public void Use_DoIfNone_to_execute_conditional_actions()
    {
        Option<int> option = new None();

        option
            .Do(i => Assert.Fail("this should not be executed"))
            .DoIfNone(() => Assert.True(true, "this should be executed"));
    }

    [Fact]
    public void ToOption_should_return_Some_for_value_types()
    {
        int i = default;
        i.ToOption()
            .Switch(
                none => Assert.Fail("should not be None"),
                some => Assert.True(true, "should be Some<int>"));

        i = 5;
        i.ToOption()
            .Switch(
                none => Assert.Fail("should not be None"),
                some => Assert.True(true, "should be Some<int>"));
    }

    [Fact]
    public void ToOption_should_return_Some_or_None_for_reference_types()
    {
        TestClass t = default;
        t.ToOption()
            .Switch(
                none => Assert.True(true, "should be None"),
                some => Assert.Fail("should not be Some<TestClass>"));

        t = new TestClass();
        t.ToOption()
            .Switch(
                none => Assert.Fail("should not be None"),
                some => Assert.True(true, "should be Some<TestClass>"));
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

    class TestClass
    {

    }
}