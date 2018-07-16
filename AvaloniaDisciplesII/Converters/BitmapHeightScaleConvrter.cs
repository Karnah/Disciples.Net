using System;
using System.Globalization;

using Avalonia.Markup;
using Avalonia.Media.Imaging;
using Engine;

namespace AvaloniaDisciplesII.Converters
{
    public class BitmapHeightScaleConvrter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var bitmap = value as Bitmap;
            if (bitmap == null)
                return 0;

            return bitmap.PixelHeight * GameInfo.Scale;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
