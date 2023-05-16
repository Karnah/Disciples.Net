using System;
using System.Collections.Generic;
using System.Linq;
using Disciples.Common.Models;
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
    public static IBitmap FromRawToBitmap(this IBitmapFactory bitmapFactory, RawBitmap image, Bounds? bounds = null)
    {
        return bitmapFactory.FromRawBitmap(image, bounds).Bitmap;
    }

    /// <summary>
    /// Конвертировать изображение из сырых данных в битмап, сохраняя исходные размеры.
    /// </summary>
    public static IBitmap FromRawToOriginalBitmap(this IBitmapFactory bitmapFactory, RawBitmap image)
    {
        var bounds = new Bounds(image.OriginalWidth, image.OriginalHeight);
        return bitmapFactory.FromRawBitmap(image, bounds).Bitmap;
    }

    /// <summary>
    /// Получить конечные кадры анимации из сырых данных.
    /// </summary>
    public static IReadOnlyList<Frame> ConvertToFrames(this IBitmapFactory bitmapFactory, IReadOnlyCollection<RawBitmap> images)
    {
        if (images.Count == 0)
            return Array.Empty<Frame>();

        return images
            .Select(i => bitmapFactory.FromRawBitmap(i))
            .ToArray();
    }
}