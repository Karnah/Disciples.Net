using System;
using System.Globalization;
using System.Windows.Media;

using Disciples.Engine.Common.Enums;

namespace Disciples.WPF.Converters
{
    /// <summary>
    /// Сконвертировать из цвета игры в цвет кисти.
    /// </summary>
    public class GameColorToBrushConverter : BaseValueConverterExtension
    {
        /// <inheritdoc />
        public override object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var gameColor = value as GameColor?;
            switch (gameColor)
            {
                case GameColor.Black:
                    return BuildBrush(Colors.Black);
                case GameColor.White:
                    return BuildBrush(Colors.White);
                case GameColor.Red:
                    return BuildBrush(Colors.Red, 128);
                case GameColor.Yellow:
                    return BuildBrush(Colors.Yellow, 128);
                case GameColor.Blue:
                    return BuildBrush(Colors.Blue, 128);
                case GameColor.Gray:
                    return BuildBrush(Colors.Gray, 128);
                case GameColor.Green:
                    return BuildBrush(Colors.Green, 128);
                case null:
                    return BuildBrush(Colors.Black);
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
}