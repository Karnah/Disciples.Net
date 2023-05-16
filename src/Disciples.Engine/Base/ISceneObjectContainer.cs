using System.Drawing;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.SceneObjects;

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
    /// <param name="fontSize">Размер шрифта.</param>
    /// <param name="x">Положение тексте, координата X.</param>
    /// <param name="y">Положение текста, координата Y.</param>
    /// <param name="layer">Слой на котором будет отображаться текст.</param>
    /// <param name="isBold">Использовать жирный шрифт.</param>
    ITextSceneObject AddText(string? text, double fontSize, double x, double y, int layer, bool isBold = false);

    /// <summary>
    /// Добавить текст на сцену.
    /// </summary>
    /// <param name="text">Текст.</param>
    /// <param name="fontSize">Размер шрифта.</param>
    /// <param name="x">Положение тексте, координата X.</param>
    /// <param name="y">Положение текста, координата Y.</param>
    /// <param name="layer">Слой на котором будет отображаться текст.</param>
    /// <param name="width">Ширина текста.</param>
    /// <param name="textAlignment">Выравнивание текста.</param>
    /// <param name="isBold">Использовать жирный шрифт.</param>
    /// <param name="foregroundColor">Цвет текста.</param>
    ITextSceneObject AddText(string? text, double fontSize, double x, double y, int layer, double width,
        TextAlignment textAlignment = TextAlignment.Center, bool isBold = false, Color? foregroundColor = null);

    /// <summary>
    /// Удалить объект со сцены.
    /// </summary>
    /// <param name="sceneObject">Удаляемый объект.</param>
    public void RemoveSceneObject(ISceneObject? sceneObject);
}