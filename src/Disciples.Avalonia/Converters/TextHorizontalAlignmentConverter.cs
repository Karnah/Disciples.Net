using System;
using System.Linq;
using Avalonia.Media;
using Disciples.Engine.Enums;
using Disciples.Engine.Models;

namespace Disciples.Avalonia.Converters;

/// <summary>
/// Сконвертировать горизонтальное выравнивание текста.
/// </summary>
public class TextHorizontalAlignmentConverter : BaseTextConverter<TextAlignment>
{
    /// <inheritdoc />
    protected override TextAlignment Convert(TextContainer textContainer, TextStyle textStyle)
    {
        return GetTextHorizontalAlignment(
            textContainer.TextPieces.FirstOrDefault()?.Style.HorizontalAlignment ??
            textStyle.HorizontalAlignment
            ?? TextHorizontalAlignment.Left)!.Value;
    }

    /// <summary>
    /// Сконвертировать выравнивание по ширине.
    /// </summary>
    private static TextAlignment? GetTextHorizontalAlignment(TextHorizontalAlignment? textHorizontalAlignment)
    {
        return textHorizontalAlignment switch
        {
            TextHorizontalAlignment.Left => TextAlignment.Left,
            TextHorizontalAlignment.Center => TextAlignment.Center,
            TextHorizontalAlignment.Right => TextAlignment.Right,
            null => null,
            _ => throw new ArgumentOutOfRangeException(nameof(textHorizontalAlignment), textHorizontalAlignment, null)
        };
    }
}