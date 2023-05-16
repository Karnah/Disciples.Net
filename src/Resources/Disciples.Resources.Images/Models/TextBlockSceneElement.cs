using Disciples.Resources.Images.Enums;

namespace Disciples.Resources.Images.Models;

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
    /// Идентификатор текста.
    /// </summary>
    public string? TextId { get; init; }

    /// <summary>
    /// Текстовая подсказка при наведении на элемент.
    /// </summary>
    /// <remarks>
    /// Не уверен, что поле служит именно для этой цели.
    /// В ресурсах игры оно нигде не задано.
    /// </remarks>
    public string? ToolTipTextId { get; init; }
}