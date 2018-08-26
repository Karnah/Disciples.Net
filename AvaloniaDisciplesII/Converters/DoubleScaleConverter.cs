using System;
using System.Globalization;

using Avalonia.Data.Converters;

using Engine;

namespace AvaloniaDisciplesII.Converters
{
    public class DoubleScaleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double d) {
                return d * GameInfo.Scale;
            }

            if (parameter is string sizeString) {
                if (double.TryParse(sizeString, NumberStyles.Float, CultureInfo.InvariantCulture, out var size) == false)
                    return 0;

                return size * GameInfo.Scale;
            }


            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value ?? parameter;
        }
    }
}
