using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Disciples.Engine.Common.Constants;

namespace Disciples.Avalonia.Converters;

/// <summary>
/// Сконвертировать цвет в цвет кисти.
/// </summary>
public class GameColorToBrushConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var gameColor = value as System.Drawing.Color? ?? GameColors.Black;
        return new SolidColorBrush(new Color(gameColor.A, gameColor.R, gameColor.G, gameColor.B));
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}