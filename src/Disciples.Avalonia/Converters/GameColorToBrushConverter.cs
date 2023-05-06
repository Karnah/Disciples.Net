using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Disciples.Engine.Common.Enums;

namespace Disciples.Avalonia.Converters;

/// <summary>
/// Сконвертировать из цвета игры в цвет кисти.
/// </summary>
public class GameColorToBrushConverter : IValueConverter
{
    /// <inheritdoc />
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var gameColor = value as GameColor?;
        return gameColor switch
        {
            GameColor.Black => new SolidColorBrush(Colors.Black),
            GameColor.White => new SolidColorBrush(Colors.White),
            GameColor.Red => new SolidColorBrush(Colors.Red, 128),
            GameColor.Yellow => new SolidColorBrush(Colors.Yellow, 128),
            GameColor.Blue => new SolidColorBrush(Colors.Blue, 128),
            GameColor.Gray => new SolidColorBrush(Colors.Gray, 128),
            GameColor.Green => new SolidColorBrush(Colors.Green, 128),
            GameColor.Orange => new SolidColorBrush(Colors.Orange, 128),
            GameColor.Paralyze => new SolidColorBrush(Colors.White, 64),
            null => new SolidColorBrush(Colors.Black),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    /// <inheritdoc />
    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}