using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using Disciples.Engine.Common.Constants;
using Disciples.Engine.Common.SceneObjects;
using Disciples.Engine.Enums;
using Disciples.Engine.Models;
using FontStyle = System.Windows.FontStyle;
using FontWeight = System.Windows.FontWeight;

namespace Disciples.WPF.Controls;

/// <summary>
/// Контрол для работы с <see cref="ITextSceneObject" />
/// </summary>
public class SceneTextBlock : Decorator
{
    /// <summary>
    /// Стиль текста.
    /// </summary>
    public static readonly DependencyProperty TextStyleProperty = DependencyProperty.Register(
        nameof(TextStyle), typeof(TextStyle), typeof(SceneTextBlock), new PropertyMetadata(default(TextStyle), TextStyleChangedCallback));

    /// <summary>
    /// Текст.
    /// </summary>
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text), typeof(TextContainer), typeof(SceneTextBlock), new PropertyMetadata(default(TextContainer), TextChangedCallback));

    /// <summary>
    /// Стиль текста.
    /// </summary>
    public TextStyle TextStyle
    {
        get => (TextStyle)GetValue(TextStyleProperty);
        set => SetValue(TextStyleProperty, value);
    }

    /// <summary>
    /// Текст.
    /// </summary>
    public TextContainer? Text
    {
        get => (TextContainer?)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>
    /// Обработать событие изменения стиля текста.
    /// </summary>
    private static void TextStyleChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((SceneTextBlock)d).TextChanged();
    }

    /// <summary>
    /// Обработать событие изменения текста.
    /// </summary>
    private static void TextChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        ((SceneTextBlock)d).TextChanged();
    }

    /// <summary>
    /// Обработать событие изменения текста или его стиля.
    /// </summary>
    private void TextChanged()
    {
        var text = Text;
        if (text == null)
        {
            Child = null;
            return;
        }

        var textStyle = TextStyle;
        var firstPieceStyle = text.TextPieces.FirstOrDefault()?.Style;
        var textBlock = new TextBlock
        {
            TextAlignment = GetTextHorizontalAlignment(
                firstPieceStyle?.HorizontalAlignment ??
                textStyle.HorizontalAlignment
                ?? TextHorizontalAlignment.Left)!.Value,
            VerticalAlignment = GetTextVerticalAlignment(
                firstPieceStyle?.VerticalAlignment ??
                textStyle.VerticalAlignment
                ?? TextVerticalAlignment.Top)!.Value,
            Background = GetBrush(
                firstPieceStyle?.BackgroundColor ??
                textStyle.BackgroundColor),
            TextWrapping = TextWrapping.Wrap
        };
        FillInlineCollection(textBlock.Inlines, textStyle, text);

        Child = textBlock;
    }

    /// <summary>
    /// Заполнить контейнер текстовыми элементами.
    /// </summary>
    private static void FillInlineCollection(InlineCollection inlineCollection, TextStyle textStyle, TextContainer textContainer)
    {
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
    }

    /// <summary>
    /// Сконвертировать жирность шрифта.
    /// </summary>
    private static FontWeight? GetFontWeight(Engine.Enums.FontWeight? fontWeight)
    {
        return fontWeight switch
        {
            Engine.Enums.FontWeight.Normal => FontWeights.Normal,
            Engine.Enums.FontWeight.Medium => FontWeights.Medium,
            Engine.Enums.FontWeight.Bold => FontWeights.Bold,
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
            Engine.Enums.FontStyle.Normal => FontStyles.Normal,
            Engine.Enums.FontStyle.Italic => FontStyles.Italic,
            null => null,
            _ => throw new ArgumentOutOfRangeException(nameof(fontStyle), fontStyle, null)
        };
    }

    /// <summary>
    /// Сконвертировать цвет.
    /// </summary>
    private static Brush? GetBrush(System.Drawing.Color? color)
    {
        if (color == null)
            return null;

        return new SolidColorBrush(Color.FromArgb(color.Value.A, color.Value.R, color.Value.G, color.Value.B));
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