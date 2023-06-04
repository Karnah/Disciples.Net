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
    [Reactive]
    public bool IsHidden { get; set; }


    /// <inheritdoc />
    public virtual void Destroy()
    { }
}