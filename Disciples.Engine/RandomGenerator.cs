using System;

namespace Disciples.Engine
{
    /// <summary>
    /// Генератор случайных чисел.
    /// </summary>
    public static class RandomGenerator
    {
        private static readonly Random Random = new Random();


        /// <summary>
        /// Получить следующее случайное целое число.
        /// </summary>
        public static int Next()
        {
            return Random.Next();
        }

        /// <summary>
        /// Получить следующее случайное целое число, ограниченное сверху.
        /// </summary>
        /// <param name="max">Верхняя граница случайного числа.</param>
        /// <returns></returns>
        public static int Next(int max)
        {
            return Random.Next(max);
        }

        /// <summary>
        /// Получить следующее случайное целое число, ограниченное сверху и снизу.
        /// </summary>
        /// <param name="min">Нижняя граница случайного числа.</param>
        /// <param name="max">Верхняя граница случайного числа.</param>
        public static int Next(int min, int max)
        {
            return Random.Next(min, max);
        }
    }
}