namespace Evntd.Bogus;

public record Address
{
    public string Street { get; init; } = String.Empty;
    public string City { get; init; } = String.Empty;
    public string State { get; init; } = String.Empty;
    public string ZipCode { get; init; } = String.Empty;
}
