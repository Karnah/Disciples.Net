using System;

namespace Engine
{
    public static class RandomGenerator
    {
        private static readonly Random Random = new Random();


        public static int Next()
        {
            return Random.Next();
        }

        public static int Next(int max)
        {
            return Random.Next(max);
        }

        public static int Next(int min, int max)
        {
            return Random.Next(min, max);
        }
    }
}
