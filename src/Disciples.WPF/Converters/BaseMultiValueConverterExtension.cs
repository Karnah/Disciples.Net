using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Disciples.WPF.Converters;

/// <summary>
/// Базовый класс для создания конвертеров в xaml.
/// </summary>
[MarkupExtensionReturnType(typeof(IValueConverter))]
public abstract class BaseMultiValueConverterExtension : MarkupExtension, IMultiValueConverter
{
    /// <summary>
    /// Метод, позволяющий объявлять конвертер как MarkupExtension.
    /// </summary>
    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        return this;
    }

    /// <inheritdoc />
    public virtual object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc />
    public virtual object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}