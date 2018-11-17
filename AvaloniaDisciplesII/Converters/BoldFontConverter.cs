using System;
using System.Globalization;

using Avalonia.Data.Converters;
using Avalonia.Media;

namespace AvaloniaDisciplesII.Converters
{
    public class BoldFontConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isBold) {
                if (isBold)
                    return FontWeight.Bold;
            }

            return FontWeight.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
