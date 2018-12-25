using System;
using System.Globalization;

using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;

using Disciples.Engine;

namespace Disciples.Avalonia.Converters
{
    public class BitmapHeightScaleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var bitmap = value as Bitmap;
            if (bitmap == null)
                return 0;

            return bitmap.PixelSize.Height * GameInfo.Scale;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}