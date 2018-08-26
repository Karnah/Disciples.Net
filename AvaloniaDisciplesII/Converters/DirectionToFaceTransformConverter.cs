using System;
using System.Globalization;

using Avalonia.Data.Converters;
using Avalonia.Media;

using Engine.Battle.Enums;

namespace AvaloniaDisciplesII.Converters
{
    public class DirectionToFaceTransformConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var direction = value as BattleDirection?;
            if (direction == null)
                return null;

            // Защищающиеся на правой панели смотрят налево, а атакующие направо
            if (direction == BattleDirection.Defender)
                return new ScaleTransform(-1, 1);

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
