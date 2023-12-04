namespace Evntd.Bogus
{
    public class ScenarioOptions
    {
        public int Days { get; set; }
        public ScenarioCustomerOptions Customer { get; set; }
        public ScenarioEmployeeOptions Employee { get; set; }
        public ScenarioOrderOptions Orders { get; set; }
        public ScenarioPizzaOptions Pizza { get; set; }
    }
}