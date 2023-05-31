using System;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;
using Disciples.Engine.Common.Constants;
using Disciples.Engine.Models;
using FontStyle = Avalonia.Media.FontStyle;
using FontWeight = Avalonia.Media.FontWeight;
using Color = Avalonia.Media.Color;

namespace Disciples.Avalonia.Converters;

/// <summary>
/// Конвертер для <see cref="TextBlock.Inlines" />
/// </summary>
public class TextContainerConverter : BaseTextConverter<InlineCollection>
{
    /// <inheritdoc />
    protected override InlineCollection Convert(TextContainer textContainer, TextStyle textStyle)
    {
        var inlineCollection = new InlineCollection();

        var fontSize = textStyle.FontSize ?? 12;
        var fontWeight = GetFontWeight(textStyle.FontWeight ?? Engine.Enums.FontWeight.Normal)!.Value;
        var fontStyle = GetFontStyle(textStyle.FontStyle ?? Engine.Enums.FontStyle.Normal)!.Value;
        var foreground = GetBrush(textStyle.ForegroundColor ?? GameColors.Black)!;
        var tabOffset = textStyle.TabOffset ?? null;

        for (int textPieceIndex = 0; textPieceIndex < textContainer.TextPieces.Count; textPieceIndex++)
        {
            var textPiece = textContainer.TextPieces[textPieceIndex];
            var textPieceStyle = textPiece.Style;
            fontSize = textPieceStyle.FontSize ?? fontSize;
            fontWeight = GetFontWeight(textPieceStyle.FontWeight) ?? fontWeight;
            fontStyle = GetFontStyle(textPieceStyle.FontStyle) ?? fontStyle;
            foreground = GetBrush(textPieceStyle.ForegroundColor) ?? foreground;
            tabOffset = textPieceStyle.TabOffset ?? tabOffset;

            for (var i = 0; i < textPieceStyle.NewLinesCount; i++)
                inlineCollection.Add(new LineBreak());

            var nextTextPiece = textPieceIndex + 1 < textContainer.TextPieces.Count
                ? textContainer.TextPieces[textPieceIndex + 1]
                : null;
            if (tabOffset != null && nextTextPiece?.Style.HasTabulate == true)
            {
                inlineCollection.Add(new TextBlock
                {
                    Text = textPiece.Text,
                    FontSize = fontSize,
                    FontWeight = fontWeight,
                    FontStyle = fontStyle,
                    Foreground = foreground,
                    Width = tabOffset.Value,
                    TextWrapping = TextWrapping.Wrap
                });
            }
            else
            {
                inlineCollection.Add(new Run(textPiece.Text)
                {
                    FontSize = fontSize,
                    FontWeight = fontWeight,
                    FontStyle = fontStyle,
                    Foreground = foreground,
                });
            }
        }

        return inlineCollection;
    }

    /// <summary>
    /// Сконвертировать жирность шрифта.
    /// </summary>
    private static FontWeight? GetFontWeight(Engine.Enums.FontWeight? fontWeight)
    {
        return fontWeight switch
        {
            Engine.Enums.FontWeight.Normal => FontWeight.Normal,
            Engine.Enums.FontWeight.Medium => FontWeight.Medium,
            Engine.Enums.FontWeight.Bold => FontWeight.Bold,
            null => null,
            _ => throw new ArgumentOutOfRangeException(nameof(fontWeight), fontWeight, null)
        };
    }

    /// <summary>
    /// Сконвертировать стиль шрифта.
    /// </summary>
    private static FontStyle? GetFontStyle(Engine.Enums.FontStyle? fontStyle)
    {
        return fontStyle switch
        {
            Engine.Enums.FontStyle.Normal => FontStyle.Normal,
            Engine.Enums.FontStyle.Italic => FontStyle.Italic,
            null => null,
            _ => throw new ArgumentOutOfRangeException(nameof(fontStyle), fontStyle, null)
        };
    }
}