using System.Collections.Generic;
using System.Linq;

namespace Disciples.Engine.Models;

/// <summary>
/// Контейнер, содержащий текст.
/// </summary>
public class TextContainer
{
    /// <summary>
    /// Создать объект типа <see cref="TextContainer" />.
    /// </summary>
    public TextContainer(string text) : this(SplitText(text))
    {
    }

    /// <summary>
    /// Создать объект типа <see cref="TextContainer" />.
    /// </summary>
    public TextContainer(IReadOnlyList<TextPiece> textPieces)
    {
        TextPieces = textPieces;
    }

    /// <summary>
    /// Кусочки текста.
    /// </summary>
    public IReadOnlyList<TextPiece> TextPieces { get; }

    /// <inheritdoc />
    public override string ToString()
    {
        return string.Concat(TextPieces);
    }

    /// <summary>
    /// Заменить текст в плейсхолдерах.
    /// </summary>
    /// <remarks>
    /// В replaces предполагается, что TextContainer содержит только одну строку без стиля.
    /// В теории, можно поддержать несколько кусочков с разными стилями, но вроде это не требуется.
    /// </remarks>
    public TextContainer ReplacePlaceholders(IReadOnlyList<(string placeholder, TextContainer text)> replaces)
    {
        var textPieces = TextPieces
            .Select(tp =>
            {
                var resultText = tp.Text;
                foreach (var (placeholder, text) in replaces)
                {
                    resultText = resultText.Replace(placeholder, text.ToString());
                }

                return new TextPiece(tp.Style, resultText);
            })
            .ToArray();

        return new TextContainer(textPieces!);
    }

    /// <summary>
    /// Разбить текст на отдельные кусочки.
    /// </summary>
    private static IReadOnlyList<TextPiece> SplitText(string text)
    {
        var formatTokenIndex = text.IndexOf(TextStyle.START_TEXT_STYLE_TOKEN);
        if (formatTokenIndex == -1)
            return new[] { new TextPiece(text)};

        var textPieces = new List<TextPiece>();
        if (formatTokenIndex > 0)
            textPieces.Add(new TextPiece(text[..formatTokenIndex]));

        var offset = formatTokenIndex;
        while (true)
        {
            var textStyle = new TextStyle(text, offset, out var tokensLength);
            offset += tokensLength;

            formatTokenIndex = text.IndexOf(TextStyle.START_TEXT_STYLE_TOKEN, offset);
            if (formatTokenIndex == -1)
            {
                textPieces.Add(new TextPiece(textStyle, text[offset..]));
                return textPieces;
            }

            textPieces.Add(new TextPiece(textStyle, text[offset..formatTokenIndex]));
            offset = formatTokenIndex;
        }
    }
}