namespace Evntd.Bogus;

// EmailAddressVerifiedEvent
// PhoneNumberVerifiedEvent

// DOMAIN MODEL

public record Customer
{
    public Guid CustomerId { get; init; } = Guid.Empty;
    public string FirstName { get; init; } = String.Empty;
    public string LastName { get; init; } = String.Empty;
    public string EmailAddress { get; init; } = String.Empty;
    public string PhoneNumber { get; init; } = String.Empty;
    public DateTimeOffset? BirthDate { get; init; } = null;
}
