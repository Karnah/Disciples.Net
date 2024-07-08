using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Disciples.Common.Models;
using Disciples.Engine;

namespace Disciples.Avalonia.Converters;

/// <summary>
/// Конвертер для максимальной ширины.
/// </summary>
internal class MaxWidthConverter : IValueConverter
{
    /// <inheritdoc />
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var bounds = value as RectangleD?;
        if (bounds == null)
            return double.NaN;

        return GameInfo.OriginalWidth - bounds.Value.X;
    }

    /// <inheritdoc />
    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
