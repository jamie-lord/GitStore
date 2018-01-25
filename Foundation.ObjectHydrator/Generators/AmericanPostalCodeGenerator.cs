using System;
using Foundation.ObjectHydrator.Interfaces;

namespace Foundation.ObjectHydrator.Generators
{
    public class AmericanPostalCodeGenerator : IGenerator<string>
    {
        private readonly Random random;

        public AmericanPostalCodeGenerator(int percentageWithPlusFour)
        {
            PercentageWithPlusFour = percentageWithPlusFour;

            random = RandomSingleton.Instance.Random;
        }

        public int PercentageWithPlusFour { get; }

        public string Generate()
        {
            string plusFour = string.Empty;

            if (PercentageWithPlusFour > 0 && random.Next(0, 100) % (100 / PercentageWithPlusFour) == 0)
            {
                plusFour = string.Format("-{0:0000}", random.Next(1, 9999));
            }

            return string.Format("{0:00000}{1}",
                random.Next(501, 99950),
                plusFour);
        }
    }
}