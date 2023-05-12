using System;
using Disciples.Engine.Common.GameObjects;

namespace Disciples.Engine.Common.Components;

/// <summary>
/// Компонент для объектов, которые могут быть нажаты ПКМ.
/// </summary>
public class MouseRightButtonClickComponent : BaseComponent
{
    private readonly Action? _onPressedAction;
    private readonly Action? _onReleasedAction;

    /// <summary>
    /// Создать объект типа <see cref="MouseRightButtonClickComponent" />.
    /// </summary>
    public MouseRightButtonClickComponent(GameObject gameObject, Action? onPressedAction = null, Action? onReleasedAction = null) : base(gameObject)
    {
        _onPressedAction = onPressedAction;
        _onReleasedAction = onReleasedAction;
    }

    /// <summary>
    /// Признак, что над объектом зажат указатель.
    /// </summary>
    public bool IsPressed { get; private set; }

    /// <summary>
    /// Обработать зажатие указателя над объектом.
    /// </summary>
    public void Pressed()
    {
        IsPressed = true;
        _onPressedAction?.Invoke();
    }

    /// <summary>
    /// Обработать освобождения указателя над объектом.
    /// </summary>
    public void Released()
    {
        // Если с объекта снимали указатель, то событие не выполняем.
        if (!IsPressed)
            return;

        IsPressed = false;
        _onReleasedAction?.Invoke();
    }
}