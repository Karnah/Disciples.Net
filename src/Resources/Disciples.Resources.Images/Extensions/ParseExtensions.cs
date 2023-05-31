using System.Drawing;

namespace Disciples.Resources.Images.Extensions;

/// <summary>
/// Методы-расширения для пасинга данных.
/// </summary>
internal static class ParseExtensions
{
    /// <summary>
    /// Распарсить границы элемента.
    /// </summary>
    public static Rectangle ParseBounds(this IReadOnlyList<string> data, int startIndex, int offsetX = 0, int offsetY = 0)
    {
        return Rectangle.FromLTRB(
            int.Parse(data[startIndex]) + offsetX,
            int.Parse(data[startIndex + 1]) + offsetY,
            int.Parse(data[startIndex + 2]) + offsetX,
            int.Parse(data[startIndex + 3]) + offsetY);
    }

    /// <summary>
    /// Распарсить точку.
    /// </summary>
    public static Point ParsePoint(this IReadOnlyList<string> data, int startIndex)
    {
        return new Point(
            int.Parse(data[startIndex]),
            int.Parse(data[startIndex + 1]));
    }

    /// <summary>
    /// Получить имя изображения.
    /// </summary>
    public static string? ParseImageName(this string data)
    {
        return string.IsNullOrEmpty(data)
            ? null
            : data;
    }

    /// <summary>
    /// Получить имя другого элемента на сцене.
    /// </summary>
    public static string? ParseObjectName(this string data)
    {
        return string.IsNullOrEmpty(data)
            ? null
            : data;
    }

    /// <summary>
    /// Получить идентификатор текстового ресурса.
    /// </summary>
    public static string? ParseTextId(this string data)
    {
        var trimmed = data.Trim('\"');
        return string.IsNullOrEmpty(trimmed)
            ? null
            : trimmed;
    }

    /// <summary>
    /// Получить строку стиля.
    /// </summary>
    public static string? ParseTextStyle(this string data)
    {
        // Некоторые строки стиля обрамлены кавычками, некоторые нет.
        var trimmed = data.Trim('\"');
        return string.IsNullOrEmpty(trimmed)
            ? null
            : trimmed;
    }

    /// <summary>
    /// Получить целое значение.
    /// </summary>
    public static int ParseInt(this string data)
    {
        return int.Parse(data);
    }

    /// <summary>
    /// Получить булево значение.
    /// </summary>
    public static bool ParseBoolean(this string data)
    {
        return int.Parse(data) == 1;
    }
}