using System;
using System.Globalization;

using Avalonia.Data.Converters;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace AvaloniaDisciplesII.Converters
{
    /// <summary>
    /// Конвертер из картинки в кисть.
    /// </summary>
    public class BitmapToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var bitmap = value as Bitmap;
            if (bitmap == null)
                return null;

            return new ImageBrush(bitmap);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}