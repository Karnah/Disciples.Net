using System;
using System.Globalization;

using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Disciples.Avalonia.Converters;

/// <summary>
/// Использовать жирный шрифт, если передано <see langword="true" />.
/// </summary>
public class BoldFontConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isBold) {
            if (isBold)
                return FontWeight.Bold;
        }

        return FontWeight.Normal;
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}