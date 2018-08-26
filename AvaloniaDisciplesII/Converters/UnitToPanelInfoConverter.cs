using System;
using System.Collections.Generic;
using System.Globalization;

using Avalonia.Data.Converters;

namespace AvaloniaDisciplesII.Converters
{
    public class UnitToPanelInfoConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Count < 3)
                return string.Empty;

            var unitName = values[0] as string;
            var unitHitPoints = values[1] as int?;
            var unitMaxHitPoints = values[2] as int?;

            if (unitName == null || unitHitPoints == null || unitMaxHitPoints == null)
                return string.Empty;

            return $"{unitName}{Environment.NewLine}" +
                   $"ОЗ : {unitHitPoints}/{unitMaxHitPoints}";
        }
    }
}
