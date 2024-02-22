using System;
using System.Collections.Generic;

namespace Disciples.Engine.Extensions;

/// <summary>
/// Расширения для коллекций.
/// </summary>
public static class CollectionExtensions
{
    /// <summary>
    /// Получить случайный элемент из коллекции.
    /// </summary>
    /// <typeparam name="T">Тип элементов коллекции.</typeparam>
    /// <param name="collection">Коллекция.</param>
    /// <returns>Случайный элемент коллекции.</returns>
    public static T GetRandomElement<T>(this IReadOnlyList<T> collection)
    {
        if (collection.Count == 0)
            throw new ArgumentException($"Коллекция не имеет элементов", nameof(collection));

        var randomIndex = RandomGenerator.Get(collection.Count);
        return collection[randomIndex];
    }

    /// <summary>
    /// Получить случайный элемент из коллекции или значение по умолчанию, если коллекция пуста.
    /// </summary>
    /// <typeparam name="T">Тип элементов коллекции.</typeparam>
    /// <param name="collection">Коллекция.</param>
    /// <returns>Случайный элемент коллекции.</returns>
    public static T? TryGetRandomElement<T>(this IReadOnlyList<T> collection)
    {
        if (collection.Count == 0)
            return default;

        return collection.GetRandomElement();
    }
}