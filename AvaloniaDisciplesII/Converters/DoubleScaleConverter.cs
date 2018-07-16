using System;
using System.Globalization;

using Avalonia.Markup;

using Engine;

namespace AvaloniaDisciplesII.Converters
{
    public class DoubleScaleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var sizeString = parameter as string;
            if (sizeString == null)
                return 0;

            if (double.TryParse(sizeString, out var size) == false)
                return 0;

            return size * GameInfo.Scale;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter;
        }
    }
}
