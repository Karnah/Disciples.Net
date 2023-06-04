using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Platform.Factories;
using Disciples.Resources.Images.Models;

namespace Disciples.Engine.Implementation.Extensions;

/// <summary>
/// Набор методов расширения для работы с изображениями.
/// </summary>
public static class BitmapExtensions
{
    /// <summary>
    /// Конвертировать изображение из сырых данных в битмап.
    /// </summary>
    public static IBitmap FromRawToBitmap(this IBitmapFactory bitmapFactory, RawBitmap image, Rectangle? bounds = null)
    {
        return bitmapFactory.FromRawBitmap(image, bounds);
    }

    /// <summary>
    /// Получить конечные кадры анимации из сырых данных.
    /// </summary>
    public static AnimationFrames ConvertToFrames(this IBitmapFactory bitmapFactory, IReadOnlyCollection<RawBitmap> images)
    {
        if (images.Count == 0)
            return new AnimationFrames();

        return new AnimationFrames(images
            .Select(i => bitmapFactory.FromRawBitmap(i))
            .ToArray());
    }
}