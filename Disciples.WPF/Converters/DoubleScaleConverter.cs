using System;
using System.Globalization;

using Disciples.Engine;

namespace Disciples.WPF.Converters
{
    public class DoubleScaleConverter : BaseValueConverterExtension
    {
        /// <inheritdoc />
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d) {
                return d * GameInfo.Scale;
            }

            if (parameter is string sizeString) {
                if (double.TryParse(sizeString, NumberStyles.Float, CultureInfo.InvariantCulture, out var size) ==
                    false)
                    return 0;

                return size * GameInfo.Scale;
            }

            return 0;
        }

        /// <inheritdoc />
        public override object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value ?? parameter;
        }
    }
}