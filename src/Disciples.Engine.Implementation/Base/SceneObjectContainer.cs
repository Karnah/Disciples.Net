using System;
using System.Drawing;
using Disciples.Common.Models;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Common.SceneObjects;
using Disciples.Engine.Models;

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
    public IImageSceneObject AddImage(IBitmap bitmap, double x, double y, int layer)
    {
        if (bitmap == null)
            throw new ArgumentNullException(nameof(bitmap));

        return AddImage(bitmap, new RectangleD(x, y, bitmap.OriginalSize.Width, bitmap.OriginalSize.Height), layer);
    }

    /// <inheritdoc />
    public IImageSceneObject AddImage(ImageSceneElement imageSceneElement, int layer)
    {
        return AddImage(imageSceneElement.ImageBitmap, imageSceneElement.Position, layer);
    }

    /// <inheritdoc />
    public IImageSceneObject AddImage(IBitmap? bitmap, RectangleD bounds, int layer)
    {
        return PlatformSceneObjectContainer.AddImageSceneObject(bitmap, bounds, layer);
    }

    /// <inheritdoc />
    public IImageSceneObject AddColorImage(Color color, RectangleD bounds, int layer)
    {
        return AddImage(_interfaceProvider.GetColorBitmap(color, new SizeD(bounds.Width, bounds.Height)), bounds, layer);
    }

    /// <inheritdoc />
    public ITextSceneObject AddText(TextContainer? text, RectangleD bounds, int layer)
    {
        return AddText(text, null, bounds, layer);
    }

    /// <inheritdoc />
    public ITextSceneObject AddText(TextContainer? text, TextStyle? textStyle, RectangleD bounds, int layer)
    {
        return PlatformSceneObjectContainer.AddTextSceneObject(text, textStyle ?? new TextStyle(), bounds, layer);
    }

    /// <inheritdoc />
    public void RemoveSceneObject(ISceneObject? sceneObject)
    {
        if (sceneObject == null)
            return;

        PlatformSceneObjectContainer.RemoveSceneObject(sceneObject);
    }
}