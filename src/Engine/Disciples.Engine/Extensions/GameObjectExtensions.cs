using System;
using System.Collections.Generic;
using System.Linq;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Models;

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
    /// <param name="isHidden">Признак, что необходимо сразу спрятать объект.</param>
    /// <returns>Объект.</returns>
    public static TGameObject Get<TGameObject>(this IReadOnlyCollection<GameObject> gameObjects, string gameObjectName, bool isHidden = false)
        where TGameObject : GameObject
    {
        var gameObject = gameObjects
            .OfType<TGameObject>()
            .First(go => go.Name == gameObjectName);
        gameObject.IsHidden = isHidden;
        return gameObject;
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

    /// <summary>
    /// Получить кнопку по имени и инициализировать метод для клика по ней.
    /// </summary>
    /// <param name="gameObjects">Список всех объектов.</param>
    /// <param name="buttonObjectName">Имя объекта.</param>
    /// <param name="clickedAction">Действие при клике.</param>
    /// <param name="isHidden">Признак, что необходимо сразу спрятать кнопку.</param>
    /// <returns>Кнопка.</returns>
    public static ButtonObject GetButton(this IReadOnlyCollection<GameObject> gameObjects, string buttonObjectName, Action? clickedAction = null, bool isHidden = false)
    {
        var button = gameObjects.Get<ButtonObject>(buttonObjectName, isHidden);
        button.ClickedAction = clickedAction;
        return button;
    }

    /// <summary>
    /// Получить кнопку по имени и инициализировать метод для клика по ней.
    /// </summary>
    /// <param name="gameObjects">Список всех объектов.</param>
    /// <param name="buttonObjectName">Имя объекта.</param>
    /// <param name="clickedAction">Действие при клике.</param>
    /// <param name="isHidden">Признак, что необходимо сразу спрятать кнопку.</param>
    /// <returns>Кнопка.</returns>
    public static ToggleButtonObject GetToggleButton(this IReadOnlyCollection<GameObject> gameObjects, string buttonObjectName, Action? clickedAction = null, bool isHidden = false)
    {
        var button = gameObjects.Get<ToggleButtonObject>(buttonObjectName, isHidden);
        button.ClickedAction = clickedAction;
        return button;
    }

    /// <summary>
    /// Получить текстовый блок по имени и инициализировать текст в нём.
    /// </summary>
    /// <param name="gameObjects">Список всех объектов.</param>
    /// <param name="textBlockObjectName">Имя объекта.</param>
    /// <param name="textContainer">Текст для размещения.</param>
    /// <returns>Текстовый блок.</returns>
    public static TextBlockObject GetTextBlock(this IReadOnlyCollection<GameObject> gameObjects, string textBlockObjectName, TextContainer textContainer)
    {
        var textBlock = gameObjects.Get<TextBlockObject>(textBlockObjectName);
        textBlock.Text = textContainer;
        return textBlock;
    }
}