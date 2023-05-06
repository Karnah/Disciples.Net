using System;
using System.Globalization;
using System.Windows.Media;
using Disciples.Engine.Common.Constants;

namespace Disciples.WPF.Converters;

/// <summary>
/// Сконвертировать из цвета игры в цвет кисти.
/// </summary>
public class GameColorToBrushConverter : BaseValueConverterExtension
{
    /// <inheritdoc />
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var gameColor = value as System.Drawing.Color? ?? GameColors.Black;
        return new SolidColorBrush(Color.FromArgb(gameColor.A, gameColor.R, gameColor.G, gameColor.B));
    }
}