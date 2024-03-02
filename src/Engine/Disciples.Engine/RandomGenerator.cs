using System;

namespace Disciples.Engine;

/// <summary>
/// Генератор случайных чисел.
/// </summary>
public static class RandomGenerator
{
    private static readonly Random Random = new();

    /// <summary>
    /// Получить случайное число от 0 до <see cref="int.MaxValue" />.
    /// </summary>
    public static int Get()
    {
        return Random.Next();
    }

    /// <summary>
    /// Получить случайное число от 0 до <paramref name="max" /> (не включительно).
    /// </summary>
    public static int Get(int max)
    {
        return Random.Next(max);
    }

    /// <summary>
    /// Получить случайное число от <paramref name="min" /> до <paramref name="max" /> (не включительно).
    /// </summary>
    public static int Get(int min, int max)
    {
        return Random.Next(min, max);
    }

    /// <summary>
    /// Получить уникальный идентификатор юнита.
    /// </summary>
    public static string GetUnitId()
    {
        return Guid.NewGuid().ToString("N");
    }
}