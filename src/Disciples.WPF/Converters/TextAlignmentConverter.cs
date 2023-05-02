using System;
using System.Globalization;

using Disciples.Engine.Common.Enums;

using WpfTextAlignment = System.Windows.TextAlignment;

namespace Disciples.WPF.Converters;

/// <summary>
/// Конвертировать из выравнивания текста Disciples в выравнивание текста WPF.
/// </summary>
public class TextAlignmentConverter : BaseValueConverterExtension
{
    /// <inheritdoc />
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var textAlignment = value as TextAlignment?;
        switch (textAlignment)
        {
            case TextAlignment.Left:
                return WpfTextAlignment.Left;
            case TextAlignment.Right:
                return WpfTextAlignment.Right;
            case TextAlignment.Center:
                return WpfTextAlignment.Center;
            case null:
                return WpfTextAlignment.Left;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}