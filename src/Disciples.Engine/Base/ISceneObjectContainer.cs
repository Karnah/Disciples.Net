using System.Drawing;
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
    /// Добавить пустое изображение на сцену.
    /// </summary>
    /// <param name="layer">Слой на котором будет отображаться изображение.</param>
    IImageSceneObject AddImage(int layer);

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
    /// <param name="width">Ширина изображения.</param>
    /// <param name="height">Высота изображения.</param>
    /// <param name="x">Положение изображения, координата X.</param>
    /// <param name="y">Положение изображения, координата Y.</param>
    /// <param name="layer">Слой на котором будет отображаться изображение.</param>
    IImageSceneObject AddImage(IBitmap? bitmap, double width, double height, double x, double y, int layer);

    /// <summary>
    /// Добавить прямоугольник указанного цвета и размеров на сцену.
    /// </summary>
    /// <param name="color">Цвет изображения.</param>
    /// <param name="width">Ширина изображения.</param>
    /// <param name="height">Высота изображения.</param>
    /// <param name="x">Положение изображения, координата X.</param>
    /// <param name="y">Положение изображения, координата Y.</param>
    /// <param name="layer">Слой на котором будет отображаться изображение.</param>
    IImageSceneObject AddColorImage(Color color, double width, double height, double x, double y, int layer);

    /// <summary>
    /// Добавить текст на сцену.
    /// </summary>
    /// <param name="text">Текст.</param>
    /// <param name="width">Ширина текста.</param>
    /// <param name="height">Высота текста.</param>
    /// <param name="x">Положение тексте, координата X.</param>
    /// <param name="y">Положение текста, координата Y.</param>
    /// <param name="layer">Слой на котором будет отображаться текст.</param>
    ITextSceneObject AddText(TextContainer? text, double width, double height, double x, double y, int layer);

    /// <summary>
    /// Добавить текст на сцену.
    /// </summary>
    /// <param name="text">Текст.</param>
    /// <param name="textStyle">Стиль текста.</param>
    /// <param name="width">Ширина текста.</param>
    /// <param name="height">Высота текста.</param>
    /// <param name="x">Положение тексте, координата X.</param>
    /// <param name="y">Положение текста, координата Y.</param>
    /// <param name="layer">Слой на котором будет отображаться текст.</param>
    ITextSceneObject AddText(TextContainer? text, TextStyle? textStyle, double width, double height, double x, double y, int layer);

    /// <summary>
    /// Удалить объект со сцены.
    /// </summary>
    /// <param name="sceneObject">Удаляемый объект.</param>
    public void RemoveSceneObject(ISceneObject? sceneObject);
}