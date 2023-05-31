using System;
using System.Drawing;
using Disciples.Engine.Enums;

namespace Disciples.Engine.Models;

/// <summary>
/// Стиль текста.
/// </summary>
public readonly record struct TextStyle
{
    /// <summary>
    /// Символа начала токена стиля текста.
    /// </summary>
    public const char START_TEXT_STYLE_TOKEN = '\\';
    /// <summary>
    /// Символа конца токена стиля текста.
    /// </summary>
    public const char END_TEXT_STYLE_TOKEN = ';';

    /// <summary>
    /// Создать объект типа <see cref="TextStyle" />
    /// </summary>
    public TextStyle(string text) : this(text, 0, out _)
    {

    }

    /// <summary>
    /// Создать объект типа <see cref="TextStyle" />
    /// </summary>
    public TextStyle(string text, int offset, out int tokensLength) : this()
    {
        if (text[offset] != START_TEXT_STYLE_TOKEN)
            throw new ArgumentException($"Токен стиля текста должен начинаться с символа {START_TEXT_STYLE_TOKEN}");

        tokensLength = 0;
        while (true)
        {
            var token = GetToken(text, offset).ToUpperInvariant();
            switch (token[1])
            {
                case 'N':
                    NewLinesCount++;
                    break;

                case 'T':
                    HasTabulate = true;
                    break;

                case 'C':
                    ForegroundColor = ParseColor(token);
                    break;

                case 'O':
                    OutlineColor = ParseColor(token);
                    break;

                case 'B':
                    BackgroundColor = ParseColor(token);
                    break;

                case 'F':
                    (FontSize, FontWeight, FontStyle) = ParseFont(token);
                    break;

                case 'V':
                    VerticalAlignment = ParseVerticalAlignment(token);
                    break;

                case 'H':
                    HorizontalAlignment = ParseHorizontalAlignment(token);
                    break;

                // TODO Margin left/right?
                case 'M':
                    NewLinesCount++;
                    SpacePadding = ParseSpacePadding(token);
                    break;

                case 'S':
                    TabOffset = ParseOffset(token);
                    break;

                case 'P':
                    WrapOffset = ParseOffset(token);
                    break;

                default:
                    throw new ArgumentException($"Неизвестный тип токена {token[1]} в строке {text[offset]}");
            }

            offset += token.Length;
            tokensLength += token.Length;
            if (text.Length <= offset || text[offset] != START_TEXT_STYLE_TOKEN)
                return;
        }
    }

    /// <summary>
    /// Количество переносов строк перед текстом.
    /// </summary>
    public int NewLinesCount { get; init; }

    /// <summary>
    /// Требует ли текст символ табуляции.
    /// </summary>
    public bool HasTabulate { get; init; }

    /// <summary>
    /// Цвет шрифта.
    /// </summary>
    public Color? ForegroundColor { get; init; }

    /// <summary>
    /// Цвет обводки текста.
    /// </summary>
    public Color? OutlineColor { get; init; }

    /// <summary>
    /// Цвет фона текста.
    /// </summary>
    public Color? BackgroundColor { get; init; }

    /// <summary>
    /// Размер шрифта.
    /// </summary>
    public int? FontSize { get; init; }

    /// <summary>
    /// Толщина шрифта.
    /// </summary>
    public FontWeight? FontWeight { get; init; }

    /// <summary>
    /// Стиль шрифта.
    /// </summary>
    public FontStyle? FontStyle { get; init; }

    /// <summary>
    /// Выравнивание текста по высоте.
    /// </summary>
    public TextVerticalAlignment? VerticalAlignment { get; init; }

    /// <summary>
    /// Выравнивание текста по ширине.
    /// </summary>
    public TextHorizontalAlignment? HorizontalAlignment { get; init; }

    /// <summary>
    /// Отступ пробелами от левого края.
    /// Если задано значение, то обязательно выполняется перенос строки.
    /// </summary>
    public int? SpacePadding { get; init; }

    /// <summary>
    /// Отступ от левого края в случае наличия символа \t.
    /// </summary>
    public int? TabOffset { get; init; }

    /// <summary>
    /// Отступ от левого края для текста, который переносится на следующую строку.
    /// </summary>
    public int? WrapOffset { get; init; }

    /// <summary>
    /// Получить токен.
    /// </summary>
    private static string GetToken(string input, int offset)
    {
        if (input[offset] != START_TEXT_STYLE_TOKEN)
            throw new ArgumentException($"Токен стиля текста должен начинаться с символа {START_TEXT_STYLE_TOKEN}");

        var textStyleTokenType = char.ToUpperInvariant(input[offset + 1]);

        // Символы \n и \t.
        if (textStyleTokenType is 'N' or 'T')
            return input[offset..(offset + 2)];

        // Если это цвет, то там 14 символов.
        if (textStyleTokenType is 'C' or 'O' or 'B')
            return input[offset..(offset+14)];

        // Во всех остальных случаях будет брать до первого символа ;.
        var endTokenIndex = input.IndexOf(END_TEXT_STYLE_TOKEN, offset);
        if (endTokenIndex == -1)
            throw new ArgumentException($"Неизвестный тип токена: {input}");

        return input[offset..(endTokenIndex + 1)];
    }

    /// <summary>
    /// Получить цвет из токена.
    /// </summary>
    /// <remarks>
    /// Токен имеет вид \cRRR;GGG;BBB; или \oRRR;GGG;BBB;
    /// </remarks>
    private static Color ParseColor(string colorToken)
    {
        return Color.FromArgb(
            int.Parse(colorToken[2..5]),
            int.Parse(colorToken[6..9]),
            int.Parse(colorToken[10..13]));
    }

    /// <summary>
    /// Получить стиль текста из шрифта.
    /// </summary>
    /// <remarks>
    /// Типы стилей:
    /// \fSmall; -- мелкий
    /// \fNormal; -- нормальный
    /// \fMenu; -- обычный жирный шрифт
    /// \fMedium; -- чуть больше нормального
    /// \fMedBold; -- жирный
    /// \fLarge; -- крупный
    /// \fVLarge; -- большой полужирный курсив
    /// </remarks>
    private static (int fontSize, FontWeight fontWeight, FontStyle fontStyle) ParseFont(string fontToken)
    {
        return fontToken switch
        {
            "\\FSMALL;" => new (12, Enums.FontWeight.Normal, Enums.FontStyle.Normal),
            "\\FNORMAL;" => new (13, Enums.FontWeight.Normal, Enums.FontStyle.Normal),
            "\\FMENU;" => new (14, Enums.FontWeight.Bold, Enums.FontStyle.Normal),
            "\\FMEDIUM;" => new (14, Enums.FontWeight.Medium, Enums.FontStyle.Normal),
            "\\FMEDBOLD;" => new (13, Enums.FontWeight.Bold, Enums.FontStyle.Normal),
            "\\FLARGE;" => new (14, Enums.FontWeight.Bold, Enums.FontStyle.Normal),
            "\\FVLARGE;" => new (20, Enums.FontWeight.Medium, Enums.FontStyle.Italic),
                _ => throw new ArgumentException($"Неизвестный тип шрифта {fontToken}", nameof(fontToken))
        };
    }

    /// <summary>
    /// Получить тип выравнивания по высоте.
    /// </summary>
    /// <remarks>
    /// \vT; -- выравнивание по верху
    /// \vC; -- выравнивание по центру
    /// \vB; -- выравнивание по низу
    /// </remarks>
    private static TextVerticalAlignment ParseVerticalAlignment(string verticalAlignmentToken)
    {
        return verticalAlignmentToken[2] switch
        {
            'T' => TextVerticalAlignment.Top,
            'C' => TextVerticalAlignment.Center,
            'B' => TextVerticalAlignment.Bottom,
            _ => throw new ArgumentException($"Неизвестный тип выравнивания по высоте {verticalAlignmentToken}", nameof(verticalAlignmentToken))
        };
    }

    /// <summary>
    /// Получить тип выравнивания по ширине.
    /// </summary>
    /// <remarks>
    /// \hL; -- выравнивание по левому краю
    /// \hC; -- выравнивание по центру
    /// \hR; -- выравнивание по правому краю
    /// </remarks>
    private static TextHorizontalAlignment ParseHorizontalAlignment(string verticalAlignmentToken)
    {
        return verticalAlignmentToken[2] switch
        {
            'L' => TextHorizontalAlignment.Left,
            'C' => TextHorizontalAlignment.Center,
            'R' => TextHorizontalAlignment.Right,
            _ => throw new ArgumentException($"Неизвестный тип выравнивания по высоте {verticalAlignmentToken}", nameof(verticalAlignmentToken))
        };
    }

    /// <summary>
    /// Получить отступ пробелами от левого края.
    /// </summary>
    /// <remarks>
    /// \mL0;
    /// \mL3;
    /// </remarks>
    private static int ParseSpacePadding(string spacePaddingToken)
    {
        return int.Parse(spacePaddingToken[3..^1]);
    }

    /// <summary>
    /// Получить отступ для текста.
    /// </summary>
    /// <remarks>
    /// \p110;
    /// \s110;
    /// </remarks>
    private static int ParseOffset(string offsetToken)
    {
        return int.Parse(offsetToken[2..^1]);
    }
}