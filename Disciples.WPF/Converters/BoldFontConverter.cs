using System;
using System.Globalization;
using System.Windows;

namespace Disciples.WPF.Converters;

/// <summary>
/// Использовать жирный шрифт, если передано <see langword="true" />.
/// </summary>
public class BoldFontConverter : BaseValueConverterExtension
{
    /// <inheritdoc />
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isBold)
        {
            if (isBold)
                return FontWeights.Bold;
        }

        return FontWeights.Normal;
    }
}