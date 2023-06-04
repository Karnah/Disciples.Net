using Disciples.Common.Models;
using Disciples.Engine;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.SceneObjects;
using ReactiveUI.Fody.Helpers;

namespace Disciples.Avalonia.SceneObjects;

/// <inheritdoc cref="IImageSceneObject" />
public class ImageSceneObject : BaseSceneObject, IImageSceneObject
{
    /// <summary>
    /// Создать объект типа <see cref="ImageSceneObject" />.
    /// </summary>
    public ImageSceneObject(IBitmap? bitmap, RectangleD bounds, int layer) : base(bounds, layer)
    {
        Bitmap = bitmap;
    }

    /// <inheritdoc />
    [Reactive]
    public IBitmap? Bitmap { get; set; }

    /// <inheritdoc />
    [Reactive]
    public bool IsReflected { get; set; }

    /// <inheritdoc />
    public HorizontalAlignment HorizontalAlignment { get; set; }

    /// <inheritdoc />
    public VerticalAlignment VerticalAlignment { get; set; }
}