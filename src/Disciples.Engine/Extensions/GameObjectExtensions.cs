using System;
using System.Collections.Generic;
using System.Linq;
using Disciples.Engine.Common.GameObjects;

namespace Disciples.Engine.Extensions;

/// <summary>
/// Расширения для работы с игровыми объектами.
/// </summary>
public static class GameObjectExtensions
{
    /// <summary>
    /// Получить объект указанного типа и имени.
    /// </summary>
    /// <typeparam name="TGameObject">Тип объекта.</typeparam>
    /// <param name="gameObjects">Список всех объектов.</param>
    /// <param name="gameObjectName">Имя объекта.</param>
    /// <returns>Объект.</returns>
    public static TGameObject Get<TGameObject>(this IReadOnlyCollection<GameObject> gameObjects, string gameObjectName)
        where TGameObject : GameObject
    {
        return gameObjects
            .OfType<TGameObject>()
            .First(go => go.Name == gameObjectName);
    }

    /// <summary>
    /// Получить объекты указанного типа по условию.
    /// </summary>
    /// <typeparam name="TGameObject">Тип объекта.</typeparam>
    /// <param name="gameObjects">Список всех объектов.</param>
    /// <param name="filter">Условия для отбора объектов.</param>
    /// <returns>Объект.</returns>
    public static IReadOnlyList<TGameObject> Get<TGameObject>(this IReadOnlyCollection<GameObject> gameObjects, Func<TGameObject, bool> filter)
        where TGameObject : GameObject
    {
        return gameObjects
            .OfType<TGameObject>()
            .Where(filter)
            .ToArray();
    }
}