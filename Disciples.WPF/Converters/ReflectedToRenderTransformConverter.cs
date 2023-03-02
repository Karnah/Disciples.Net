using System;
using System.Globalization;
using System.Windows.Media;

namespace Disciples.WPF.Converters;

/// <summary>
/// Использовать отраженную трансформацию, если передано <see langword="true" />.
/// </summary>
public class ReflectedToRenderTransformConverter : BaseValueConverterExtension
{
    /// <inheritdoc />
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var isReflected = value as bool? ?? false;
        if (isReflected)
            return new ScaleTransform(-1, 1);

        return null;
    }
}