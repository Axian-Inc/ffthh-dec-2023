namespace Evntd.Bogus;

public class DeliveryOrderPlacedEvent
{
    public Guid CustomerId { get; set; }
    public Address DeliveryAddress { get; set; }
    public List<OrderedPizza> Pizzas { get; set; }
}
