using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Avalonia.Media;
using Disciples.Engine.Models;

namespace Disciples.Avalonia.Converters;

/// <summary>
/// Базовый конвертер для работы с текстом.
/// </summary>
public abstract class BaseTextConverter<TResult> : IMultiValueConverter
{
    /// <inheritdoc />
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count < 2)
            return BindingOperations.DoNothing;

        var textContainer = values[0] as TextContainer;
        var textStyle = values[1] as TextStyle?;
        if (textContainer == null || textStyle == null)
            return BindingOperations.DoNothing;

        return Convert(textContainer, textStyle.Value);
    }

    /// <summary>
    /// Получить результат.
    /// </summary>
    protected abstract TResult Convert(TextContainer textContainer, TextStyle textStyle);

    /// <summary>
    /// Сконвертировать цвет.
    /// </summary>
    protected static IBrush? GetBrush(System.Drawing.Color? color)
    {
        if (color == null)
            return null;

        return new SolidColorBrush(new Color(color.Value.A, color.Value.R, color.Value.G, color.Value.B));
    }
}