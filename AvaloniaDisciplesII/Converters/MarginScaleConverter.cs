using System;
using System.Globalization;

using Avalonia;
using Avalonia.Data.Converters;

using Engine;

namespace AvaloniaDisciplesII.Converters
{
    public class MarginScaleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var thiknessString = parameter as string;
            if (thiknessString == null)
                return new Thickness();

            var thikness = Thickness.Parse(thiknessString);
            return new Thickness(
                thikness.Left * GameInfo.Scale,
                thikness.Top * GameInfo.Scale,
                thikness.Right * GameInfo.Scale,
                thikness.Bottom * GameInfo.Scale);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
