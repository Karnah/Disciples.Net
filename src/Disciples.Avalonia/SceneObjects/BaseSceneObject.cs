using Disciples.Common.Models;
using ReactiveUI;
using Disciples.Engine.Common.SceneObjects;
using ReactiveUI.Fody.Helpers;

namespace Disciples.Avalonia.SceneObjects;

/// <inheritdoc cref="ISceneObject" />
public abstract class BaseSceneObject : ReactiveObject, ISceneObject
{
    /// <summary>
    /// Создать объект типа <see cref="BaseSceneObject" />.
    /// </summary>
    protected BaseSceneObject(RectangleD bounds, int layer)
    {
        Bounds = bounds;
        Layer = layer;
    }

    /// <inheritdoc />
    [Reactive]
    public RectangleD Bounds { get; set; }

    /// <inheritdoc />
    public int Layer { get; }

    /// <inheritdoc />
    [Reactive]
    public bool IsHidden { get; set; }

    /// <inheritdoc />
    public virtual void Destroy()
    {
    }
}