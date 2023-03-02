using ReactiveUI;
using Disciples.Engine.Common.SceneObjects;
using ReactiveUI.Fody.Helpers;

namespace Disciples.Avalonia.SceneObjects;

/// <inheritdoc cref="ISceneObject" />
public abstract class BaseSceneObject : ReactiveObject, ISceneObject
{
    /// <inheritdoc />
    protected BaseSceneObject(int layer)
    {
        Layer = layer;
    }


    /// <inheritdoc />
    [Reactive]
    public double X { get; set; }

    /// <inheritdoc />
    [Reactive]
    public double Y { get; set; }

    /// <inheritdoc />
    [Reactive]
    public double Width { get; set; }

    /// <inheritdoc />
    [Reactive]
    public double Height { get; set; }

    /// <inheritdoc />
    public int Layer { get; }


    /// <inheritdoc />
    public virtual void Destroy()
    { }
}