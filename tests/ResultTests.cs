using Xunit;
using OneOf.Types;
using OneOf;

namespace Svan.Monads.UnitTests
{
    public class ResultTests
    {
        const string ErrorMessage = "division by zero";
        private Result<string, int> Divide(int number, int by)
        {
            if (by == 0)
            {
                return new Error<string>(ErrorMessage);
            }
            else
            {
                return new Success<int>(number / by);
            }
        }

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

        [Fact]
        public void DefaultWith_lets_you_define_fallback_values()
        {
            var maxLimitException = new Exception();
            maxLimitException.Data.Add("max", 25);

            Result<Exception, int> add5(int val) => new Success<int>(val + 5);
            Result<Exception, int> checkIsBelow25(int val)
            {
                if (val > 25)
                {
                    return new Error<Exception>(maxLimitException);
                }
                else
                {
                    return new Success<int>(val);
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
            Result<Exception, int> result = 5;

            result
                .DoIfError(_ => Assert.Fail("this should not be executed"))
                .Do(i => Assert.Equal(5, i));
        }

        [Fact]
        public void Use_DoIfError_to_execute_conditional_actions()
        {
            Result<Exception, int> result = new Exception("this is an error");

            result
                .DoIfError(error => Assert.Equal("this is an error", error.Message))
                .Do(i => Assert.Fail("this should not be executed"));
        }

        [Fact]
        public void Result_can_be_downcasted_to_option()
        {
            Result<Exception, int> result = new Exception("this is an error");
            result.ToOption().Switch(
                none => Assert.True(true, "Error should map to None"),
                some => Assert.Fail("this should not be executed"));

            result = 5;
            result.ToOption().Switch(
                none => Assert.Fail("this should not be executed"),
                some => Assert.Equal(5, some.Value));
        }

        [Fact]
        public void Result_can_be_folded_into_a_single_value()
        {
            Result<Exception, int> result = 5;

            var actual = result
                .Fold(
                    error => 0,
                    success => success * 2);

            Assert.Equal(10, actual);
        }

        [Fact]
        public void Combine_results_with_zip_all_are_success()
        {
            Result<Exception, int> result1 = 5;
            Result<Exception, int> result2 = 8;
            var expected = 13;

            var actual = result1
                .Zip(result2, (value1, value2) => value1 + value2)
                .DefaultWith((_) => 0);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Combine_results_with_zip_when_errors_exist()
        {
            Result<Exception, int> result1 = 5;
            Result<Exception, int> result2 = new Exception("an error");
            var expected = "this should happen";

            var actual = result1
                .Zip(result2, (value1, value2) => "this should not happen")
                .DefaultWith((_) => "this should happen");

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Combine_five_results_with_zip_all_are_success()
        {
            Result<Exception, int> result1 = 1;
            Result<Exception, int> result2 = 2;
            Result<Exception, int> result3 = 3;
            Result<Exception, int> result4 = 4;
            Result<Exception, int> result5 = 5;

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
            Result<string, int> result1 = 1;
            Result<string, int> result2 = "first error";
            Result<string, int> result3 = 3;
            Result<string, int> result4 = "second error";
            Result<string, int> result5 = 5;

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
        public void Pattern_using_OneOf_as_error_type()
        {
            Result<CustomerError, Customer> ValidateEmail(Customer customer)
            {
                if (customer.Email.Contains("@")) return customer;
                else return new CustomerError(new InvalidEmail());
            }

            Result<CustomerError, Customer> ValidatePhoneNumber(Customer customer)
            {
                if (customer.PhoneNumber.Contains("+")) return customer;
                else return new CustomerError(new InvalidPhoneNumber());
            }

            var input = new Customer() { Email = "valid@email.com", PhoneNumber = "invalid" };

            var expected = "invalid phone number";

            var actual = ValidateEmail(input)
                .Bind(ValidatePhoneNumber)
                .Map(customer => $"valid customer: ${customer.Email} - ${customer.PhoneNumber}")
                .MapError(error => error.Match(
                    invalidEmail => "invalid email",
                    invalidPhoneNumber => "invalid phone number"
                ));

            Assert.Equal(expected, actual.ErrorValue());
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

        class Customer
        {
            public string Email { get; set; }
            public string PhoneNumber { get; set; }
        }

        class InvalidEmail { }
        class InvalidPhoneNumber { }

        class CustomerError : OneOfBase<InvalidEmail, InvalidPhoneNumber>
        {
            public CustomerError(OneOf<InvalidEmail, InvalidPhoneNumber> input) : base(input)
            {
            }
        }

    }
}