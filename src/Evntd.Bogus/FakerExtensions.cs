using Bogus;

namespace Evntd.Bogus
{
    public static class FakerExtensions
    {
        public static string[] PickMultipleChoice(this Faker faker, MultipleChoice multipleChoice)
        {
            return faker.Make(
                faker.Random.Int(multipleChoice.Min, multipleChoice.Max),
                () => faker.PickWeighted(multipleChoice.Choices)).Distinct().ToArray();
        }

        public static T PickWeighted<T>(this Faker faker, IDictionary<T, float> weights)
        {
            return faker.Random.WeightedRandom(weights.Keys.ToArray(), weights.Values.ToArray());
        }
    }
}
