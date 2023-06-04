using System;
using System.Globalization;
using System.Windows;

namespace Disciples.WPF.Converters;

/// <summary>
/// Конвертер возвращает <see cref="Visibility.Hidden" />, если параметр равен <see langword="true" />.
/// </summary>
internal class InverseBooleanToVisibilityConverter : BaseValueConverterExtension
{
    /// <inheritdoc />
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var isHidden = value as bool?;
        return isHidden == true
            ? Visibility.Hidden
            : Visibility.Visible;
    }
}