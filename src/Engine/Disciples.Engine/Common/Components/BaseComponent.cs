using Disciples.Engine.Common.GameObjects;

namespace Disciples.Engine.Common.Components;

/// <summary>
/// Базовый класс для компонентов игровых объектов.
/// </summary>
public abstract class BaseComponent : IComponent
{
    /// <summary>
    /// Создать объект типа <see cref="BaseComponent" />.
    /// </summary>
    protected BaseComponent(GameObject gameObject)
    {
        GameObject = gameObject;
    }

    /// <summary>
    /// Игровой объект, который содержит данный компонент.
    /// </summary>
    public GameObject GameObject { get; }

    /// <inheritdoc />
    public virtual void Initialize()
    {
    }

    /// <inheritdoc />
    public virtual void Update(long tickCount)
    {
    }

    /// <inheritdoc />
    public virtual void Destroy()
    {
    }
}