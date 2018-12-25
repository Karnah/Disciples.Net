using System;
using System.Globalization;

using Avalonia;
using Avalonia.Data.Converters;

using Disciples.Engine;

namespace Disciples.Avalonia.Converters
{
    public class MarginScaleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var thicknessString = parameter as string;
            if (thicknessString == null)
                return new Thickness();

            var thickness = Thickness.Parse(thicknessString);
            return new Thickness(
                thickness.Left * GameInfo.Scale,
                thickness.Top * GameInfo.Scale,
                thickness.Right * GameInfo.Scale,
                thickness.Bottom * GameInfo.Scale);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}