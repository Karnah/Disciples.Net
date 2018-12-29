using System;
using System.Globalization;

using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Disciples.Avalonia.Converters
{
    /// <summary>
    /// Использовать отраженную трансформацию, если передано <see langword="true" />.
    /// </summary>
    public class ReflectedToRenderTransformConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var isReflected = value as bool? ?? false;
            if (isReflected)
                return new ScaleTransform(-1, 1);

            return null;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}