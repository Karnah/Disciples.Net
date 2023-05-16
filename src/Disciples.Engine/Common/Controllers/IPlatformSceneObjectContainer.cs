using System.Collections.Generic;
using System.Drawing;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.SceneObjects;

namespace Disciples.Engine.Common.Controllers;

/// <summary>
/// Платформозависимая реализация контейнера объектов на сцене.
/// </summary>
public interface IPlatformSceneObjectContainer
{
    /// <summary>
    /// Список всех объектов на сцене.
    /// </summary>
    IReadOnlyList<ISceneObject> SceneObjects { get; }

    /// <summary>
    /// Создать пустое изображение, которое отображается на сцене.
    /// </summary>
    /// <param name="layer">Слой, на котором изображение будет отображаться.</param>
    IImageSceneObject AddImageSceneObject(int layer);

    /// <summary>
    /// Создать текст, который будет отображаться на сцене.
    /// </summary>
    /// <param name="text">Текст.</param>
    /// <param name="fontSize">Размер шрифта.</param>
    /// <param name="layer">Слой, на котором будет отображаться текст.</param>
    /// <param name="isBold">Использовать жирный шрифт.</param>
    ITextSceneObject AddTextSceneObject(string? text, double fontSize, int layer, bool isBold = false);

    /// <summary>
    /// Создать текст, который будет отображаться на сцене.
    /// </summary>
    /// <param name="text">Текст.</param>
    /// <param name="fontSize">Размер шрифта.</param>
    /// <param name="layer">Слой, на котором будет отображаться текст.</param>
    /// <param name="width">Ширина пространства, занимаемого текстом.</param>
    /// <param name="textAlignment">Выравнивание текста по ширине.</param>
    /// <param name="isBold">Использовать жирный шрифт.</param>
    /// <param name="foregroundColor">Цвет шрифта.</param>
    ITextSceneObject AddTextSceneObject(string? text,
        double fontSize,
        int layer,
        double width,
        TextAlignment textAlignment = TextAlignment.Center,
        bool isBold = false,
        Color? foregroundColor = null);

    /// <summary>
    /// Удалить объект со сцены.
    /// </summary>
    void RemoveSceneObject(ISceneObject sceneObject);

    /// <summary>
    /// Обновить список всех объектов на сцене.
    /// </summary>
    /// <remarks>
    /// Используется, чтобы создавать/удалять объекты за один проход, а не каждый раз при вызове метода.
    /// </remarks>
    void UpdateContainer();
}