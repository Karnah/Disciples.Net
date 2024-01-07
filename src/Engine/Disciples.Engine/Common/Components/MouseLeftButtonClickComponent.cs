using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.GameObjects;

namespace Disciples.Engine.Common.Components;

/// <summary>
/// Компонент для объектов, которые могут быть нажаты ЛКМ.
/// </summary>
public class MouseLeftButtonClickComponent : BaseComponent
{
    /// <summary>
    /// Время между двумя кликами, чтобы это отработало как двойной клик.
    /// </summary>
    private const int DOUBLE_CLICK_TIME_MS = 300;

    private readonly Stopwatch _lastClickStopwatch = Stopwatch.StartNew();

    private readonly Action? _onPressedAction;
    private readonly Action? _onClickedAction;
    private readonly Action? _onDoubleClickedAction;

    /// <summary>
    /// Создать объект типа <see cref="MouseLeftButtonClickComponent" />
    /// </summary>
    public MouseLeftButtonClickComponent(GameObject gameObject,
        IReadOnlyList<KeyboardButton> hotKeys,
        Action? onPressedAction = null,
        Action? onClickedAction = null,
        Action? onDoubleClickedAction = null
        ) : base(gameObject)
    {
        _onPressedAction = onPressedAction;
        _onClickedAction = onClickedAction;
        _onDoubleClickedAction = onDoubleClickedAction;

        HotKeys = hotKeys;
    }

    /// <summary>
    /// Признак, что над объектом зажат указатель.
    /// </summary>
    public bool IsPressed { get; private set; }

    /// <summary>
    /// Список горячих клавиш.
    /// </summary>
    public IReadOnlyList<KeyboardButton> HotKeys { get; }

    /// <summary>
    /// Обработать зажатие указателя над объектом.
    /// </summary>
    public void Pressed()
    {
        IsPressed = true;
        _onPressedAction?.Invoke();
    }

    /// <summary>
    /// Обработать клик указателя над объектом.
    /// </summary>
    public void Clicked()
    {
        // Если с объекта снимали указатель, то клик не выполняем.
        if (!IsPressed)
            return;

        IsPressed = false;

        if (_lastClickStopwatch.ElapsedMilliseconds < DOUBLE_CLICK_TIME_MS && _onDoubleClickedAction != null)
        {
            _onDoubleClickedAction.Invoke();
        }
        else
        {
            _onClickedAction?.Invoke();
        }

        _lastClickStopwatch.Restart();
    }

    /// <summary>
    /// Обработать сброс зажатия указателя без клика на объекте.
    /// </summary>
    /// <remarks>
    /// Возникает, если нажать на объект и сместить зажатый указатель в сторону.
    /// </remarks>
    public void Unpressed()
    {
        IsPressed = false;
    }

    /// <summary>
    /// Обработать нажатие на клавишу.
    /// </summary>
    public void PressedKeyboardButton(KeyboardButton keyboardButton)
    {
        if (!HotKeys.Contains(keyboardButton))
            return;

        Pressed();
        Clicked();
    }
}