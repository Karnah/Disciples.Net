using System;
using System.Collections.Generic;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Models;

namespace Disciples.Engine.Common.Models;

/// <summary>
/// Кнопка-переключатель из двух состояний.
/// </summary>
public class ToggleButtonSceneElement : SceneElement
{
    /// <inheritdoc />
    public override SceneElementType Type => SceneElementType.ToggleButton;

    /// <summary>
    /// Вид кнопки для каждого состояния.
    /// </summary>
    /// <remarks>
    /// Если <see langword="null" />, значит кнопка не отображается на экране,
    /// И нужна только для обработки горячих клавиш.
    /// </remarks>
    public ButtonStates? ButtonStates { get; init; }

    /// <summary>
    /// Вид кнопки для каждого состояния, когда кнопка активирована.
    /// </summary>
    public ButtonStates? CheckedButtonStates { get; init; }

    /// <summary>
    /// Текстовая подсказка при наведении на элемент.
    /// </summary>
    public TextContainer? ToolTip { get; init; }

    /// <summary>
    /// Список "горячих клавиш" для кнопки.
    /// </summary>
    public IReadOnlyList<KeyboardButton> HotKeys { get; init; } = Array.Empty<KeyboardButton>();
}