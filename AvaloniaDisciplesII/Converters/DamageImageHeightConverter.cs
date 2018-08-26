using System;
using System.Collections.Generic;
using System.Globalization;

using Avalonia.Data.Converters;

namespace AvaloniaDisciplesII.Converters
{
    public class DamageImageHeightConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Count < 3)
                return 0;

            var imageHeight = values[0] as double?;
            var hitPoints = values[1] as int?;
            var maxHitPoints = values[2] as int?;
            if (imageHeight == null || hitPoints == null || maxHitPoints == null)
                return 0;

            return imageHeight * (maxHitPoints - hitPoints) / maxHitPoints;
        }
    }
}
