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
    public IReadOnlyDictionary<SceneButtonState, IBitmap> ButtonStates { get; init; } = null!;

    /// <summary>
    /// Текстовая подсказка при наведении на элемент.
    /// </summary>
    public string? ToolTip { get; init; }

    /// <summary>
    /// Признак, что при зажатой кнопке мыши срабатывают периодические клики.
    /// </summary>
    public bool IsRepeat { get; init; }

    /// <summary>
    /// Список "горячих клавиш" для кнопки.
    /// </summary>
    public IReadOnlyList<KeyboardButton> HotKeys { get; init; } = Array.Empty<KeyboardButton>();
}