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
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var gameColor = value as GameColor?;
        switch (gameColor) {
            case GameColor.Black:
                return new SolidColorBrush(Colors.Black);
            case GameColor.White:
                return new SolidColorBrush(Colors.White);
            case GameColor.Red:
                return new SolidColorBrush(Colors.Red, 128);
            case GameColor.Yellow:
                return new SolidColorBrush(Colors.Yellow, 128);
            case GameColor.Blue:
                return new SolidColorBrush(Colors.Blue, 128);
            case GameColor.Gray:
                return new SolidColorBrush(Colors.Gray, 128);
            case GameColor.Green:
                return new SolidColorBrush(Colors.Green, 128);
            case null:
                return new SolidColorBrush(Colors.Black);
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