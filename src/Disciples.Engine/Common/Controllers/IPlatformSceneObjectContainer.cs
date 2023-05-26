using System.Collections.Generic;
using Disciples.Engine.Common.SceneObjects;
using Disciples.Engine.Models;

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
    /// <param name="textStyle">Стиль текста.</param>
    /// <param name="width">Ширина пространства, занимаемого текстом.</param>
    /// <param name="height">Высота пространства, занимаемого текстом.</param>
    /// <param name="x">X-координата текста.</param>
    /// <param name="y">Y-координата текста.</param>
    /// <param name="layer">Слой, на котором будет отображаться текст.</param>
    ITextSceneObject AddTextSceneObject(TextContainer? text,
        TextStyle textStyle,
        double width,
        double height,
        double x,
        double y,
        int layer);

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