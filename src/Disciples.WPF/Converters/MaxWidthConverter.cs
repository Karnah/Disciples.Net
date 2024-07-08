using Disciples.Common.Models;
using Disciples.Engine;
using System;
using System.Globalization;

namespace Disciples.WPF.Converters;

/// <summary>
/// Конвертер для максимальной ширины.
/// </summary>
internal class MaxWidthConverter : BaseValueConverterExtension
{
    /// <inheritdoc />
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var bounds = value as RectangleD?;
        if (bounds == null)
            return double.NaN;

        return GameInfo.OriginalWidth - bounds.Value.X;
    }
}
