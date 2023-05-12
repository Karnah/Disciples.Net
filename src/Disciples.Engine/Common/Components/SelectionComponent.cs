using System;
using Disciples.Engine.Common.GameObjects;

namespace Disciples.Engine.Common.Components;

/// <summary>
/// Компонент для игровых объектов, который могут быть выбраны в качестве цели.
/// </summary>
public class SelectionComponent : BaseComponent
{
    private readonly Action? _onSelectedAction;
    private readonly Action? _onUnselectedAction;

    /// <summary>
    /// Создать объект типа <see cref="SelectionComponent" />.
    /// </summary>
    public SelectionComponent(GameObject gameObject, Action? onSelectedAction = null, Action? onUnselectedAction = null) : base(gameObject)
    {
        _onSelectedAction = onSelectedAction;
        _onUnselectedAction = onUnselectedAction;
    }

    /// <summary>
    /// Признак, что указатель находится над объектом.
    /// </summary>
    public bool IsSelected { get; private set; }

    /// <summary>
    /// Обработать наведение указателя на объект.
    /// </summary>
    public void Selected()
    {
        IsSelected = true;
        _onSelectedAction?.Invoke();
    }

    /// <summary>
    /// Обработать снятие указателя с объекта.
    /// </summary>
    public void Unselected()
    {
        IsSelected = false;
        _onUnselectedAction?.Invoke();

        // Если убрали выделение с объекта, сбрасываем нажатие.
        var clickComponent = GameObject.TryGetComponent<MouseLeftButtonClickComponent>();
        clickComponent?.Unpressed();
    }
}