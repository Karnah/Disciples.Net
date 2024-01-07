using Disciples.Engine.Common.Enums;
using Disciples.Engine.Models;

namespace Disciples.Engine.Common.Models;

/// <summary>
/// Текстовый блок.
/// </summary>
public class TextBlockSceneElement : SceneElement
{
    /// <inheritdoc />
    public override SceneElementType Type => SceneElementType.TextBlock;

    /// <summary>
    ///Стиль текста.
    /// </summary>
    public TextStyle? TextStyle { get; init; }

    /// <summary>
    /// Текст.
    /// </summary>
    public TextContainer? Text { get; init; }

    /// <summary>
    /// Текстовая подсказка при наведении на элемент.
    /// </summary>
    public TextContainer? ToolTip { get; init; }
}