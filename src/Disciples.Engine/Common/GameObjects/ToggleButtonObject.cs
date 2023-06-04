using Disciples.Engine.Base;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Models;

namespace Disciples.Engine.Common.GameObjects;

/// <summary>
/// Кнопка-переключатель из двух состояний.
/// </summary>
public class ToggleButtonObject : BaseButtonObject
{
    private readonly ToggleButtonSceneElement _toggleButton;

    /// <summary>
    /// Создать объект типа <see cref="ToggleButtonObject" />.
    /// </summary>
    public ToggleButtonObject(
        ISceneObjectContainer sceneObjectContainer,
        ToggleButtonSceneElement toggleButton,
        int layer)
        : base(sceneObjectContainer, toggleButton, toggleButton.HotKeys, layer)
    {
        _toggleButton = toggleButton;
    }

    /// <summary>
    /// Признак, что кнопка находится во включенном состоянии.
    /// </summary>
    public bool IsChecked { get; private set; }

    /// <inheritdoc />
    protected override ButtonStates? ButtonStates => IsChecked
        ? _toggleButton.CheckedButtonStates
        : _toggleButton.ButtonStates;

    /// <inheritdoc />
    public override void SetDisabled()
    {
        IsChecked = false;

        base.SetDisabled();
    }

    /// <inheritdoc />
    protected override SceneButtonState ProcessClickInternal()
    {
        IsChecked = !IsChecked;

        return base.ProcessClickInternal();
    }

    /// <summary>
    /// Установить новое состояние кнопки.
    /// </summary>
    public void SetState(bool isChecked)
    {
        if (ButtonState == SceneButtonState.Disabled || IsChecked == isChecked)
            return;

        IsChecked = isChecked;
        UpdateButtonVisualObject();
    }
}