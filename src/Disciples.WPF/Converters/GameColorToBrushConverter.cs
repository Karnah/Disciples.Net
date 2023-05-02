using System;
using System.Globalization;
using System.Windows.Media;

using Disciples.Engine.Common.Enums;

namespace Disciples.WPF.Converters;

/// <summary>
/// Сконвертировать из цвета игры в цвет кисти.
/// </summary>
public class GameColorToBrushConverter : BaseValueConverterExtension
{
    /// <inheritdoc />
    public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var gameColor = value as GameColor?;
        return gameColor switch
        {
            GameColor.Black => BuildBrush(Colors.Black),
            GameColor.White => BuildBrush(Colors.White),
            GameColor.Red => BuildBrush(Colors.Red, 128),
            GameColor.Yellow => BuildBrush(Colors.Yellow, 128),
            GameColor.Blue => BuildBrush(Colors.Blue, 128),
            GameColor.Gray => BuildBrush(Colors.Gray, 128),
            GameColor.Green => BuildBrush(Colors.Green, 128),
            GameColor.Orange => BuildBrush(Colors.Orange, 128),
            null => BuildBrush(Colors.Black),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    /// <summary>
    /// Создать кисть указанного цвета и прозрачности.
    /// </summary>
    /// <param name="color">Цвет кисти.</param>
    /// <param name="opacity">Прозрачность кисти.</param>
    private static Brush BuildBrush(Color color, double opacity = 255)
    {
        var brush = new SolidColorBrush(color) {
            Opacity = opacity
        };

        return brush;
    }
}