namespace Evntd.Bogus
{
    public class MultipleChoice
    {
        public int Min { get; set; }
        public int Max { get; set; }
        public Dictionary<string, float> Choices { get; set; }
    }
}