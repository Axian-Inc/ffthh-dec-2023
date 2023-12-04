namespace Evntd.Bogus;

public class CarryoutOrderPlacedEvent
{
    public Guid CustomerId { get; set; }
    public List<OrderedPizza> Pizzas { get; set; }
}
