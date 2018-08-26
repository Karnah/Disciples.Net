using System;
using System.Collections.Generic;
using System.Globalization;

using Avalonia.Data.Converters;

namespace AvaloniaDisciplesII.Converters
{
    public class UnitHitpointsToStringConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Count < 2)
                return string.Empty;

            var hitPoints = values[0] as int?;
            var maxHitPoints = values[1] as int?;
            if (hitPoints == null || maxHitPoints == null)
                return string.Empty;

            return $"{hitPoints}/{maxHitPoints}";
        }
    }
}
