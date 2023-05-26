using System;
using System.Linq;
using Avalonia.Layout;
using Disciples.Engine.Enums;
using Disciples.Engine.Models;

namespace Disciples.Avalonia.Converters;

/// <summary>
/// Сконвертировать вертикальное выравнивание текста.
/// </summary>
public class TextVerticalAlignmentConverter : BaseTextConverter<VerticalAlignment>
{
    /// <inheritdoc />
    protected override VerticalAlignment Convert(TextContainer textContainer, TextStyle textStyle)
    {
        return GetTextVerticalAlignment(
            textContainer.TextPieces.FirstOrDefault()?.Style.VerticalAlignment ??
            textStyle.VerticalAlignment
            ?? TextVerticalAlignment.Top)!.Value;
    }

    /// <summary>
    /// Сконвертировать выравнивание по высоте.
    /// </summary>
    private static VerticalAlignment? GetTextVerticalAlignment(TextVerticalAlignment? textVerticalAlignment)
    {
        return textVerticalAlignment switch
        {
            TextVerticalAlignment.Top => VerticalAlignment.Top,
            TextVerticalAlignment.Center => VerticalAlignment.Center,
            TextVerticalAlignment.Bottom => VerticalAlignment.Bottom,
            null => null,
            _ => throw new ArgumentOutOfRangeException(nameof(textVerticalAlignment), textVerticalAlignment, null)
        };
    }
}