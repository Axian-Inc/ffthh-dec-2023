namespace Evntd.Bogus;

public class PizzaPreparedEvent
{
    public int PizzaId { get; set; }
    public Guid PreparedBy { get; set; }
}
