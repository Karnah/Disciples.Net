using System;
using Disciples.Engine.Common.GameObjects;

namespace Disciples.Engine.Common.Components;

/// <summary>
/// Компонент для игровых объектов, который могут быть выбраны в качестве цели.
/// </summary>
public class SelectionComponent : BaseComponent
{
    private readonly Action? _onHoveredAction;
    private readonly Action? _onUnhoveredAction;

    /// <summary>
    /// Создать объект типа <see cref="SelectionComponent" />.
    /// </summary>
    public SelectionComponent(GameObject gameObject, Action? onHoveredAction = null, Action? onUnhoveredAction = null) : base(gameObject)
    {
        _onHoveredAction = onHoveredAction;
        _onUnhoveredAction = onUnhoveredAction;
    }

    /// <summary>
    /// Признак, что указатель находится над объектом.
    /// </summary>
    public bool IsHover { get; private set; }

    /// <summary>
    /// Признак, что объект доступен для выделения.
    /// </summary>
    public bool IsSelectionEnabled { get; set; } = true;

    /// <summary>
    /// Обработать наведение указателя на объект.
    /// </summary>
    public void Hovered()
    {
        IsHover = true;
        _onHoveredAction?.Invoke();
    }

    /// <summary>
    /// Обработать снятие указателя с объекта.
    /// </summary>
    public void Unhovered()
    {
        IsHover = false;

        // Если убрали выделение с объекта, сбрасываем нажатие.
        var clickComponent = GameObject.TryGetComponent<MouseLeftButtonClickComponent>();
        clickComponent?.Unpressed();

        _onUnhoveredAction?.Invoke();
    }
}