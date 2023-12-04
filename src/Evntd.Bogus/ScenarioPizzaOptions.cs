namespace Evntd.Bogus
{
    public class ScenarioPizzaOptions
    {
        public Dictionary<string, float> Sizes { get; set; }
        public Dictionary<string, float> Sauces { get; set; }
        public Dictionary<string, float> CheeseQuantities { get; set; }
        public MultipleChoice CheeseOptions { get; set; }
        public MultipleChoice MeatOptions { get; set; }
        public MultipleChoice VegetableOptions { get; set; }
    }
}