using System;
using Foundation.ObjectHydrator.Interfaces;

namespace Foundation.ObjectHydrator.Generators
{
    public class EmailAddressGenerator : IGenerator<string>
    {
        private readonly Random random;

        public EmailAddressGenerator()
        {
            random = RandomSingleton.Instance.Random;
        }

        public string Generate()
        {
            IGenerator<string> fng = new FirstNameGenerator();
            IGenerator<string> lng = new LastNameGenerator();
            IGenerator<string> cng = new CompanyNameGenerator();

            string prefix = GetPrefix(fng, lng);
            string bizname = GetBizname(cng);

            string[] suffix = new string[4] {".com", ".net", ".org", ".info"};
            int num = random.Next(0, suffix.Length - 1);
            string domaintype = suffix[num];

            return string.Format("{0}@{1}{2}", prefix, bizname, domaintype);
        }

        private string GetPrefix(IGenerator<string> fng, IGenerator<string> lng)
        {
            int prefixtype = random.Next(0, 1);
            string prefix;
            if (prefixtype == 0)
            {
                prefix = string.Format("{0}_{1}", fng.Generate(), lng.Generate());
            }
            else
            {
                prefix = fng.Generate();
            }

            return prefix;
        }

        private static string GetBizname(IGenerator<string> cng)
        {
            string bizname = cng.Generate();
            bizname = bizname.Replace(".", "");
            bizname = bizname.Replace(" ", "");
            bizname = bizname.Replace(",", "");
            return bizname;
        }
    }
}