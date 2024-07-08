using System;
using System.Collections.Generic;
using Disciples.Engine.Common.Enums;

namespace Disciples.Engine.Common.Models;

/// <summary>
/// Кнопка.
/// </summary>
public class ButtonSceneElement : SceneElement
{
    /// <inheritdoc />
    public override SceneElementType Type => SceneElementType.Button;

    /// <summary>
    /// Вид кнопки для каждого состояния.
    /// </summary>
    /// <remarks>
    /// Если <see langword="null" />, значит кнопка не отображается на экране,
    /// И нужна только для обработки горячих клавиш.
    /// </remarks>
    public ButtonStates? ButtonStates { get; init; }

    /// <summary>
    /// Признак, что при зажатой кнопке мыши срабатывают периодические клики.
    /// </summary>
    public bool IsRepeat { get; init; }

    /// <summary>
    /// Список "горячих клавиш" для кнопки.
    /// </summary>
    public IReadOnlyList<KeyboardButton> HotKeys { get; init; } = Array.Empty<KeyboardButton>();
}