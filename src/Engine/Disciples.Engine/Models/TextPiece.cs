namespace Disciples.Engine.Models;

/// <summary>
/// Кусочек текста со своим стилем.
/// </summary>
public record TextPiece
{
    /// <summary>
    /// Создать объект типа <see cref="TextPiece" />.
    /// </summary>
    public TextPiece(TextStyle style, string text)
    {
        Style = style;
        Text = text;
    }

    /// <summary>
    /// Создать объект типа <see cref="TextPiece" />.
    /// </summary>
    public TextPiece(string text)
    {
        Text = text;
    }

    /// <summary>
    /// Стиль текста.
    /// </summary>
    public TextStyle Style { get; }

    /// <summary>
    /// Текст.
    /// </summary>
    public string Text { get; }

    /// <inheritdoc />
    public override string ToString()
    {
        return Text;
    }
}