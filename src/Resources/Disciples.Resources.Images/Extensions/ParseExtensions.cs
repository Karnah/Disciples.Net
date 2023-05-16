using Disciples.Common.Models;

namespace Disciples.Resources.Images.Extensions;

/// <summary>
/// Методы-расширения для пасинга данных.
/// </summary>
internal static class ParseExtensions
{
    /// <summary>
    /// Распарсить границы элемента.
    /// </summary>
    public static Bounds ParseBounds(this IReadOnlyList<string> data, int startIndex)
    {
        return new Bounds(
            int.Parse(data[startIndex + 3]),
            int.Parse(data[startIndex + 1]),
            int.Parse(data[startIndex]),
            int.Parse(data[startIndex + 2]));
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
    /// Получить булево значение.
    /// </summary>
    public static bool ParseBoolean(this string data)
    {
        return int.Parse(data) == 1;
    }
}