using System;
using System.Globalization;

using Avalonia.Data.Converters;

using Disciples.Engine.Common.Enums;

using AvaloniaTextAlignment = Avalonia.Media.TextAlignment;

namespace Disciples.Avalonia.Converters;

/// <summary>
/// Конвертировать из выравнивания текста Disciples в выравнивание текста Avalonia.
/// </summary>
public class TextAlignmentConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var textAlignment = value as TextAlignment?;
        switch (textAlignment) {
            case TextAlignment.Left:
                return AvaloniaTextAlignment.Left;
            case TextAlignment.Right:
                return AvaloniaTextAlignment.Right;
            case TextAlignment.Center:
                return AvaloniaTextAlignment.Center;
            case null:
                return AvaloniaTextAlignment.Left;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <inheritdoc />
    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}