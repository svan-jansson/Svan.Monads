using Xunit;

namespace Svan.Monads.UnitTests
{
    public class ResultTests
    {
        const string ErrorMessage = "division by zero";
        private Result<string, int> Divide(int number, int by)
        {
            if (by == 0)
            {
                return Result<string, int>.Error(ErrorMessage);
            }
            else
            {
                return Result<string, int>.Success(number / by);
            }
        }

        [Fact]
        public void Success_and_error_both_have_values()
        {
            var expectedSuccess = (12 / 2 / 2) * 2;

            Divide(12, 2)
                .Bind(result => Divide(result, 2))
                .Map(result => result * 2)
                .AssertSuccess(expectedSuccess);

            Divide(12, 0)
                .Bind(result => Divide(result, 2))
                .Map(result => result * 2)
                .AssertError(ErrorMessage);
        }

        [Fact]
        public void DefaultWith_lets_you_define_fallback_values()
        {
            var maxLimitException = new Exception();
            maxLimitException.Data.Add("max", 25);

            Result<Exception, int> add5(int val) => Result<Exception, int>.Success(val + 5);
            Result<Exception, int> checkIsBelow25(int val)
            {
                if (val > 25)
                {
                    return Result<Exception, int>.Error(maxLimitException);
                }
                else
                {
                    return Result<Exception, int>.Success(val);
                }
            }


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

        [Fact]
        public void Use_Do_to_execute_conditional_actions()
        {
            var result = Result<Exception, int>.Success(5);

            result
                .DoIfError(_ => Assert.Fail("this should not be executed"))
                .Do(i => Assert.Equal(5, i));
        }

        [Fact]
        public void Use_DoIfError_to_execute_conditional_actions()
        {
            var result = Result<Exception, int>.Error(new Exception("this is an error"));

            result
                .DoIfError(error => Assert.Equal("this is an error", error.Message))
                .Do(i => Assert.Fail("this should not be executed"));
        }

        [Fact]
        public void Result_can_be_downcasted_to_option()
        {
            var result = Result<Exception, int>.Error(new Exception("this is an error"));
            result.ToOption().AssertNone();

            result = Result<Exception, int>.Success(5);
            result.ToOption().AssertSome(5);
        }

        [Fact]
        public void Result_can_be_folded_into_a_single_value()
        {
            var result = Result<Exception, int>.Success(5);

            var actual = result
                .Fold(
                    error => 0,
                    success => success * 2);

            Assert.Equal(10, actual);
        }

        [Fact]
        public void Combine_results_with_zip_all_are_success()
        {
            var result1 = Result<Exception, int>.Success(5);
            var result2 = Result<Exception, int>.Success(8);
            var expected = 13;

            var actual = result1
                .Zip(result2, (value1, value2) => value1 + value2)
                .DefaultWith((_) => 0);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Combine_results_with_zip_when_errors_exist()
        {
            var result1 = Result<Exception, int>.Success(5);
            var result2 = Result<Exception, int>.Error(new Exception("an error"));
            var expected = "this should happen";

            var actual = result1
                .Zip(result2, (value1, value2) => "this should not happen")
                .DefaultWith((_) => "this should happen");

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Combine_five_results_with_zip_all_are_success()
        {
            var result1 = Result<Exception, int>.Success(1);
            var result2 = Result<Exception, int>.Success(2);
            var result3 = Result<Exception, int>.Success(3);
            var result4 = Result<Exception, int>.Success(4);
            var result5 = Result<Exception, int>.Success(5);

            var expected = 1 + 2 + 3 + 4 + 5;

            var actual = result1
                .Zip(
                    result2,
                    result3,
                    result4,
                    result5,
                    (value1, value2, value3, value4, value5)
                        => value1 + value2 + value3 + value4 + value5)
                .DefaultWith((_) => 0);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Zip_returns_the_first_encountered_error()
        {
            var result1 = Result<string, int>.Success(1);
            var result2 = Result<string, int>.Error("first error");
            var result3 = Result<string, int>.Success(3);
            var result4 = Result<string, int>.Error("second error");
            var result5 = Result<string, int>.Success(5);

            var expected = "first error";

            var actual = result1
                .Zip(
                    result2,
                    result3,
                    result4,
                    result5,
                    (value1, value2, value3, value4, value5)
                        => value1 + value2 + value3 + value4 + value5)
                .ErrorValue();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Merge_can_combine_results()
        {
            var result = Result<string, int>.Success(10)
                .Merge(Result<string, int>.Success(20))
                .Merge(Result<string, int>.Success(30))
                .Merge(Result<string, int>.Success(40))
                .Merge(Result<string, int>.Success(50))
                .Fold(
                    (error) => 0,
                    (group) => group.Item1 + group.Item2 + group.Item3 + group.Item4 + group.Item5);

            Assert.Equal(150, result);
        }

        [Fact]
        public void OrThrow_throws_an_invalid_operation_exception()
        {
            var resultError = Result<int, int>.Error(0);
            Assert.Throws<InvalidOperationException>(() => resultError.OrThrow());

            var resultSuccess = Result<int, int>.Success(1);
            Assert.Equal(1, resultSuccess.OrThrow());
        }
        [Fact]
        public void Recover_from_error()
        {
            var resultError = Result<int, int>.Error(0);
            var actualRecoverSuccess = resultError
                .Recover((error) => Result<string, int>.Success(error + 1))
                .OrThrow();

            Assert.Equal(1, actualRecoverSuccess);

            var actualRecoverError = resultError
                .Recover((error) => Result<string, int>.Error("error"));
            Assert.Equal("error", actualRecoverError.ErrorValue());
        }

        [Fact]
        public void Flatten_returns_inner_result_when_success()
        {
            var nested = Result<string, Result<string, int>>.Success(Result<string, int>.Success(42));
            nested.Flatten().AssertSuccess(42);
        }

        [Fact]
        public void Flatten_returns_inner_error_when_inner_is_error()
        {
            var nested = Result<string, Result<string, int>>.Success(Result<string, int>.Error("inner error"));
            nested.Flatten().AssertError("inner error");
        }

        [Fact]
        public void Flatten_returns_outer_error_when_outer_is_error()
        {
            var nested = Result<string, Result<string, int>>.Error("outer error");
            nested.Flatten().AssertError("outer error");
        }

        [Fact]
        public void Sequence_returns_all_values_when_all_are_success()
        {
            var results = new[]
            {
                Result<string, int>.Success(1),
                Result<string, int>.Success(2),
                Result<string, int>.Success(3),
            };

            var sequenced = results.Sequence();
            Assert.True(sequenced.IsSuccess());
            Assert.Equal(new[] { 1, 2, 3 }, sequenced.SuccessValue());
        }

        [Fact]
        public void Sequence_returns_first_error_when_any_is_error()
        {
            var results = new[]
            {
                Result<string, int>.Success(1),
                Result<string, int>.Error("first error"),
                Result<string, int>.Error("second error"),
            };

            var sequenced = results.Sequence();
            sequenced.AssertError("first error");
        }

        [Fact]
        public void Sequence_returns_success_with_empty_list_when_empty()
        {
            var results = Array.Empty<Result<string, int>>();

            var sequenced = results.Sequence();
            Assert.True(sequenced.IsSuccess());
            Assert.Empty(sequenced.SuccessValue());
        }
    }
}