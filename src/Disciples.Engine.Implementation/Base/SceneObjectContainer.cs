using System;
using System.Drawing;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Common.SceneObjects;

namespace Disciples.Engine.Implementation.Base;

/// <inheritdoc cref="ISceneObjectContainer" />
/// <remarks>
/// Класс запечатан, чтобы не было проблем с DI из-за наследования.
/// </remarks>
public sealed class SceneObjectContainer : ISceneObjectContainer
{
    private readonly IInterfaceProvider _interfaceProvider;

    /// <summary>
    /// Создать объект типа <see cref="SceneObjectContainer" />.
    /// </summary>
    public SceneObjectContainer(IPlatformSceneObjectContainer platformSceneObjectContainer, IInterfaceProvider interfaceProvider)
    {
        PlatformSceneObjectContainer = platformSceneObjectContainer;
        _interfaceProvider = interfaceProvider;
    }

    /// <inheritdoc />
    public IPlatformSceneObjectContainer PlatformSceneObjectContainer { get; }

    /// <inheritdoc />
    public IImageSceneObject AddImage(int layer)
    {
        return PlatformSceneObjectContainer.AddImageSceneObject(layer);
    }

    /// <inheritdoc />
    public IImageSceneObject AddImage(IBitmap bitmap, double x, double y, int layer)
    {
        if (bitmap == null)
            throw new ArgumentNullException(nameof(bitmap));

        return AddImage(bitmap, bitmap.Width, bitmap.Height, x, y, layer);
    }

    /// <inheritdoc />
    public IImageSceneObject AddImage(ImageSceneElement imageSceneElement, int layer)
    {
        var bounds = imageSceneElement.Position;
        return AddImage(imageSceneElement.ImageBitmap, bounds.Width, bounds.Height, bounds.Left, bounds.Top, layer);
    }

    /// <inheritdoc />
    public IImageSceneObject AddImage(IBitmap? bitmap, double width, double height, double x, double y, int layer)
    {
        var imageVisual = PlatformSceneObjectContainer.AddImageSceneObject(layer);
        imageVisual.X = x;
        imageVisual.Y = y;
        imageVisual.Width = width;
        imageVisual.Height = height;
        imageVisual.Bitmap = bitmap;

        return imageVisual;
    }

    /// <inheritdoc />
    public IImageSceneObject AddColorImage(Color color, double width, double height, double x, double y, int layer)
    {
        return AddImage(_interfaceProvider.GetColorBitmap(color), width, height, x, y, layer);
    }

    /// <inheritdoc />
    public ITextSceneObject AddText(string? text, double fontSize, double x, double y, int layer, bool isBold = false)
    {
        return AddText(text, fontSize, x, y, layer, double.NaN, TextAlignment.Left, isBold);
    }

    /// <inheritdoc />
    public ITextSceneObject AddText(string? text, double fontSize, double x, double y, int layer, double width,
        TextAlignment textAlignment = TextAlignment.Center, bool isBold = false, Color? foregroundColor = null)
    {
        var textVisual = PlatformSceneObjectContainer.AddTextSceneObject(text, fontSize, layer, width, textAlignment, isBold, foregroundColor);
        textVisual.X = x;
        textVisual.Y = y;

        return textVisual;
    }

    /// <inheritdoc />
    public void RemoveSceneObject(ISceneObject? sceneObject)
    {
        if (sceneObject == null)
            return;

        PlatformSceneObjectContainer.RemoveSceneObject(sceneObject);
    }
}