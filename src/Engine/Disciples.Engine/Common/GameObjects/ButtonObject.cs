using Disciples.Engine.Base;
using Disciples.Engine.Common.Models;

namespace Disciples.Engine.Common.GameObjects;

/// <summary>
/// Класс для кнопки.
/// </summary>
public class ButtonObject : BaseButtonObject
{
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
    }

    /// <inheritdoc />
    protected override ButtonStates? ButtonStates { get; }
}