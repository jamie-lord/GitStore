using System;
using Foundation.ObjectHydrator.Interfaces;

namespace Foundation.ObjectHydrator.Generators
{
    public class ByteArrayGenerator : IGenerator<byte[]>
    {
        private readonly Random random;

        public ByteArrayGenerator()
            : this(8)
        {
        }

        public ByteArrayGenerator(int length)
        {
            random = RandomSingleton.Instance.Random;
            Length = length;
        }

        public int Length { get; set; }

        #region IGenerator Members

        public byte[] Generate()
        {
            byte[] toReturn = new byte[Length];

            random.NextBytes(toReturn);

            return toReturn;
        }

        #endregion
    }
}