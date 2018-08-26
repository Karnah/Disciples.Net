using System;
using System.Globalization;

using Avalonia;
using Avalonia.Data.Converters;

using Engine;
using Engine.Battle.Enums;
using Engine.Battle.GameObjects;

namespace AvaloniaDisciplesII.Converters
{
    public class UnitPositionToMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var battleUnit = value as BattleUnit;
            if (battleUnit == null)
                return new Thickness();

            // Защищающиеся на правой панели распологаются ли справа налево,
            // А атакающие слева направо
            var lineOffset = battleUnit.Direction == BattleDirection.Defender
                ? battleUnit.Unit.SquadLinePosition
                : (battleUnit.Unit.SquadLinePosition + 1) % 2;

            return new Thickness(
                0,
                106 * (2 - battleUnit.Unit.SquadFlankPosition) * GameInfo.Scale,
                battleUnit.Unit.UnitType.SizeSmall
                    ? 80 * lineOffset * GameInfo.Scale
                    : 0,
                0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
