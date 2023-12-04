namespace Evntd.Bogus;

public record Pizza
{
    public string Size { get; init; } = String.Empty;
    public string Sauce { get; init; } = String.Empty;
    public string CheeseQuantity { get; init; } = String.Empty;
    public string[] Cheeses { get; init; } = new string[0];
    public string[] Meats { get; init; } = new string[0];
    public string[] Vegetables { get; init; } = new string[0];
}
