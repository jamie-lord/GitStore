using System;
using Foundation.ObjectHydrator.Interfaces;

namespace Foundation.ObjectHydrator.Generators
{
    public class CCVGenerator : IGenerator<string>
    {
        private readonly Random random;

        public CCVGenerator(string ccvtype)
        {
            random = RandomSingleton.Instance.Random;
            CCVType = ccvtype;
        }

        public string CCVType { get; set; }

        public string Generate()
        {
            int ccv = random.Next(0, 999);
            if (ccv < 100)
            {
                ccv += 100;
            }

            return ccv.ToString();
        }
    }
}