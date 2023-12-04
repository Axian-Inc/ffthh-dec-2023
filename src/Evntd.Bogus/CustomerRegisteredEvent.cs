namespace Evntd.Bogus;

public class CustomerRegisteredEvent
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string EmailAddress { get; set; }
    public string PhoneNumber { get; set; }
    public DateTimeOffset? BirthDate { get; set; }
    public string PasswordHash { get; set; }
    public string PasswordSalt { get; set; }
}
