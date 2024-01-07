using Disciples.Engine.Models;

namespace Disciples.Engine.Common.Models;

/// <summary>
/// Текстовый элемент списка.
/// </summary>
public class TextListBoxItem
{
    /// <summary>
    /// Отображаемый текст.
    /// </summary>
    public TextContainer Text { get; init; } = null!;
}