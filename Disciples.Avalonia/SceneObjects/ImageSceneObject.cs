using Disciples.Engine;
using Disciples.Engine.Common.SceneObjects;
using ReactiveUI.Fody.Helpers;

namespace Disciples.Avalonia.SceneObjects;

/// <inheritdoc cref="IImageSceneObject" />
public class ImageSceneObject : BaseSceneObject, IImageSceneObject
{
    /// <inheritdoc />
    public ImageSceneObject(int layer) : base(layer)
    { }


    /// <inheritdoc />
    [Reactive]
    public IBitmap? Bitmap { get; set; }

    /// <inheritdoc />
    [Reactive]
    public bool IsReflected { get; set; }
}