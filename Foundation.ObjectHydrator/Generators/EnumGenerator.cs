﻿using System;
using Foundation.ObjectHydrator.Interfaces;

namespace Foundation.ObjectHydrator.Generators
{
    public class EnumGenerator : IGenerator<object>
    {
        private readonly Array EnumValues;
        private readonly Random random;

        public EnumGenerator(Array enumValues)
        {
            EnumValues = enumValues;
            random = RandomSingleton.Instance.Random;
        }

        public object Generate()
        {
            return EnumValues.GetValue(random.Next(0, EnumValues.Length - 1));
        }
    }
}