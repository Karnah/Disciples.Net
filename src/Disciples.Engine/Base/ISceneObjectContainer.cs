using System.Drawing;
using Disciples.Common.Models;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.SceneObjects;
using Disciples.Engine.Models;

namespace Disciples.Engine.Base;

/// <summary>
/// Контейнер для добавления и удаления объектов со сцены.
/// </summary>
public interface ISceneObjectContainer
{
    /// <summary>
    /// Платформозависимый контейнер объект на сцене.
    /// </summary>
    IPlatformSceneObjectContainer PlatformSceneObjectContainer { get; }

    /// <summary>
    /// Добавить статичное изображение на сцену.
    /// </summary>
    /// <param name="bitmap">Изображение.</param>
    /// <param name="x">Положение изображения, координата X.</param>
    /// <param name="y">Положение изображения, координата Y.</param>
    /// <param name="layer">Слой на котором будет отображаться изображение.</param>
    IImageSceneObject AddImage(IBitmap bitmap, double x, double y, int layer);

    /// <summary>
    /// Добавить статичное изображение на сцену.
    /// </summary>
    /// <param name="imageSceneElement">Изображение.</param>
    /// <param name="layer">Слой на котором будет отображаться изображение.</param>
    IImageSceneObject AddImage(ImageSceneElement imageSceneElement, int layer);

    /// <summary>
    /// Добавить статичное изображение указанных размеров на сцену.
    /// </summary>
    /// <param name="bitmap">Изображение.</param>
    /// <param name="bounds">Границы изображения.</param>
    /// <param name="layer">Слой на котором будет отображаться изображение.</param>
    IImageSceneObject AddImage(IBitmap? bitmap, RectangleD bounds, int layer);

    /// <summary>
    /// Добавить прямоугольник указанного цвета и размеров на сцену.
    /// </summary>
    /// <param name="color">Цвет изображения.</param>
    /// <param name="bounds">Границы изображения.</param>
    /// <param name="layer">Слой на котором будет отображаться изображение.</param>
    IImageSceneObject AddColorImage(Color color, RectangleD bounds, int layer);

    /// <summary>
    /// Добавить текст на сцену.
    /// </summary>
    /// <param name="text">Текст.</param>
    /// <param name="bounds">Границы текста.</param>
    /// <param name="layer">Слой на котором будет отображаться текст.</param>
    ITextSceneObject AddText(TextContainer? text, RectangleD bounds, int layer);

    /// <summary>
    /// Добавить текст на сцену.
    /// </summary>
    /// <param name="text">Текст.</param>
    /// <param name="textStyle">Стиль текста.</param>
    /// <param name="bounds">Границы текста.</param>
    /// <param name="layer">Слой на котором будет отображаться текст.</param>
    ITextSceneObject AddText(TextContainer? text, TextStyle? textStyle, RectangleD bounds, int layer);

    /// <summary>
    /// Удалить объект со сцены.
    /// </summary>
    /// <param name="sceneObject">Удаляемый объект.</param>
    public void RemoveSceneObject(ISceneObject? sceneObject);
}