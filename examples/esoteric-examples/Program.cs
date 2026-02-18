/*
This example shows usage of the result monad in combination with the core OneOf library.
*/

using OneOf;
using Svan.Monads;

var inputs = new[]
{
    new CreateCustomerInput("valid@email.com", "4612345"),
    new CreateCustomerInput("invalidemail.com", "4612345"),
    new CreateCustomerInput("valid@email.com", "invalid phone number"),
    new CreateCustomerInput("another@email.com", "4612345"),
};

inputs.Select(input
    => CreateCustomer(input)
        .Fold(
            error => error.Match(
                invalidEmail => $"Invalid email: {invalidEmail.Email}",
                phoneNumber => $"Invalid phone: {phoneNumber.PhoneNumber}",
                databaseNotReachable => $"Could not save to database: {databaseNotReachable.Reason}"
            ),
            customer => $"Customer created: {customer.CustomerId}"))
      .ToList()
      .ForEach(Console.WriteLine);

#region Functions

Result<CustomerError, CustomerCreated> CreateCustomer(CreateCustomerInput input)
    => ValidateEmail(input)
        .Bind(ValidatePhoneNumber)
        .Bind(SaveCustomer);


Result<CustomerError, CreateCustomerInput> ValidateEmail(CreateCustomerInput input)
    => input
        .ToOption()
        .Filter(i => i.Email.Contains('@'))
        .ToResult<CustomerError>(() => new InvalidEmail(input.Email));

Result<CustomerError, CreateCustomerInput> ValidatePhoneNumber(CreateCustomerInput input)
    => input
        .ToOption()
        .Filter(i => i.PhoneNumber.All(c => char.IsNumber(c)))
        .ToResult<CustomerError>(() => new InvalidPhoneNumber(input.PhoneNumber));

Result<CustomerError, CustomerCreated> SaveCustomer(CreateCustomerInput input)
    => Randomizer.ShouldFail()
        ? new CustomerError(new DatabaseNotReachable("Random database error occurred")).ToError<CustomerError, CustomerCreated>()
        : new CustomerCreated(Guid.NewGuid(), input.Email, input.PhoneNumber).ToSuccess<CustomerError, CustomerCreated>();

#endregion

#region SupportingTypes

record CreateCustomerInput(string Email, string PhoneNumber);
record CustomerCreated(Guid CustomerId, string Email, string PhoneNumber);
record InvalidPhoneNumber(string PhoneNumber);
record DatabaseNotReachable(string Reason);
record InvalidEmail(string Email);

class CustomerError : OneOfBase<
    InvalidEmail,
    InvalidPhoneNumber,
    DatabaseNotReachable>
{
    public CustomerError(OneOf<InvalidEmail, InvalidPhoneNumber, DatabaseNotReachable> input) : base(input) { }
    public static implicit operator CustomerError(InvalidEmail _) => new(_);
    public static implicit operator CustomerError(InvalidPhoneNumber _) => new(_);
    public static implicit operator CustomerError(DatabaseNotReachable _) => new(_);
}

static class Randomizer
{
    private static readonly Random _rng = new(Seed: 100);
    public static bool ShouldFail() => _rng.Next(10) > 5;
}


#endregion