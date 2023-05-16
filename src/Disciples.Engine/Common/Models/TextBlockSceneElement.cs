using Disciples.Engine.Common.Enums;

namespace Disciples.Engine.Common.Models;

/// <summary>
/// Текстовый блок.
/// </summary>
public class TextBlockSceneElement : SceneElement
{
    /// <inheritdoc />
    public override SceneElementType Type => SceneElementType.TextBlock;

    /// <summary>
    /// Информация о шрифте.
    /// </summary>
    public string? Font { get; init; }

    /// <summary>
    /// Текст.
    /// </summary>
    public string? Text { get; init; }

    /// <summary>
    /// Текстовая подсказка при наведении на элемент.
    /// </summary>
    public string? ToolTip { get; init; }
}