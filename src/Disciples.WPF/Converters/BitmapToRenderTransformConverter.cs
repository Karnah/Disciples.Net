using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Disciples.Engine;
using Disciples.Engine.Common.Constants;
using Disciples.Engine.Common.Enums;
using Disciples.WPF.SceneObjects;

namespace Disciples.WPF.Converters;

/// <summary>
/// Использовать отраженную трансформацию, если передано <see langword="true" />.
/// </summary>
public class BitmapToRenderTransformConverter : BaseMultiValueConverterExtension
{
    /// <inheritdoc />
    public override object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length < 2)
            return Binding.DoNothing;

        var image = values[0] as ImageSceneObject;
        var isReflected = values[1] as bool?;
        var bitmap = values[2] as IBitmap;
        if (image == null || isReflected == null || bitmap == null)
            return Binding.DoNothing;

        var transforms = new TransformCollection();

        if (isReflected == true)
            transforms.Add(new ScaleTransform(-1, 1));

        var offsetX = isReflected == false
            ? bitmap.Offset.X
            : bitmap.OriginalSize.Width - bitmap.ActualSize.Width - bitmap.Offset.X;
        switch (image.HorizontalAlignment)
        {
            case HorizontalAlignment.Left:
                break;
            case HorizontalAlignment.Center:
                offsetX += (image.Bounds.Width - bitmap.OriginalSize.Width) / 2;
                break;
            case HorizontalAlignment.Right:
                offsetX += image.Bounds.Width;
                break;
        }

        var offsetY = isReflected == false
            ? bitmap.Offset.Y
            : bitmap.OriginalSize.Height - bitmap.ActualSize.Height - bitmap.Offset.Y;
        switch (image.VerticalAlignment)
        {
            case VerticalAlignment.Top:
                break;
            case VerticalAlignment.Center:
                offsetY += (image.Bounds.Height - bitmap.OriginalSize.Height) / 2;
                break;
            case VerticalAlignment.Bottom:
                offsetY += image.Bounds.Height;
                break;
        }

        if (Math.Abs(offsetX - 1) > EngineConstants.DOUBLE_TOLERANCE || Math.Abs(offsetY - 1) > EngineConstants.DOUBLE_TOLERANCE)
            transforms.Add(new TranslateTransform(offsetX, offsetY));

        if (transforms.Count == 0)
            return Binding.DoNothing;

        if (transforms.Count == 1)
            return transforms[0];

        return new TransformGroup { Children = transforms };
    }
}