using System.Diagnostics;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Models;

namespace Disciples.Engine.Common.GameObjects;

/// <summary>
/// Класс для кнопки.
/// </summary>
public class ButtonObject : BaseButtonObject
{
    /// <summary>
    /// Время между нажатием, и когда сработает автоматический клик.
    /// Используется для механизма нажатия с повторениями.
    /// </summary>
    private const int REPEAT_CLICK_TIME_MS = 300;

    private readonly bool _isRepeat;
    private readonly Stopwatch _pressedStopwatch = Stopwatch.StartNew();

    private bool _isRepeating;

    /// <summary>
    /// Создать объект типа <see cref="ButtonObject" />.
    /// </summary>
    public ButtonObject(
        ISceneObjectContainer sceneObjectContainer,
        ButtonSceneElement button,
        int layer
    ) : base(sceneObjectContainer, button, button.HotKeys, layer)
    {
        ButtonStates = button.ButtonStates;
        _isRepeat = button.IsRepeat;
    }

    /// <inheritdoc />
    protected override ButtonStates? ButtonStates { get; }

    /// <inheritdoc />
    public override void Update(long ticksCount)
    {
        base.Update(ticksCount);

        // Если кнопку достаточно долго держат и выставлен признак "повторение",
        // То отрабатываем автоматический клик.
        if (MouseLeftButtonClickComponent!.IsPressed &&
            _isRepeat &&
            _pressedStopwatch.ElapsedMilliseconds > REPEAT_CLICK_TIME_MS)
        {
            _isRepeating = false;
            ProcessClickInternal();
            _isRepeating = true;
            _pressedStopwatch.Restart();
        }
    }

    /// <inheritdoc />
    protected override void OnPressed()
    {
        base.OnPressed();

        _pressedStopwatch.Restart();
    }

    /// <inheritdoc />
    protected override void ProcessClickInternal()
    {
        // Если выполнялось повторение нажатия на кнопку, то отпускание мыши не должно вызывать никакой эффект.
        // Пример: игрок зажимает кнопку "вниз" в списке, дожидается когда фокус окажется в нужном месте и отпускает.
        // Если после этого обработать клик, то фокус перескочит на следующий элемент.
        if (_isRepeating)
            return;

        base.ProcessClickInternal();
    }
}