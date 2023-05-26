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
    /// Информация о стиле отображения текста.
    /// </summary>
    public string? TextStyle { get; init; }

    /// <summary>
    /// Идентификатор текста.
    /// </summary>
    public string? TextId { get; init; }

    /// <summary>
    /// Идентификатор текстовой подсказки при наведении на элемент.
    /// </summary>
    public string? ToolTipTextId { get; init; }
}