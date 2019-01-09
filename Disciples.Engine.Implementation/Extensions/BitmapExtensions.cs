using System;
using System.Collections.Generic;

using Disciples.Engine.Common.Models;
using Disciples.Engine.Platform.Factories;
using Disciples.Engine.Platform.Models;
using Disciples.ResourceProvider.Models;

namespace Disciples.Engine.Implementation.Extensions
{
    /// <summary>
    /// Набор методов расширения для работы с изображениями.
    /// </summary>
    public static class BitmapExtensions
    {
        /// <summary>
        /// Конвертировать изображение из сырых данных в битмап.
        /// </summary>
        public static IBitmap FromRawToBitmap(this IBitmapFactory bitmapFactory, RawBitmap image, Bounds bounds = null)
        {
            if (image == null)
                return null;

            return bitmapFactory.FromRawBitmap(image, bounds).Bitmap;
        }

        /// <summary>
        /// Конвертировать изображение из сырых данных в битмап, сохраняя исходные размеры.
        /// </summary>
        public static IBitmap FromRawToOriginalBitmap(this IBitmapFactory bitmapFactory, RawBitmap image)
        {
            if (image == null)
                return null;

            return bitmapFactory.FromRawBitmap(image, new Bounds(0, image.Height, 0, image.Width)).Bitmap;
        }

        /// <summary>
        /// Получить конечные кадры анимации из сырых данных.
        /// </summary>
        public static IReadOnlyList<Frame> ConvertToFrames(this IBitmapFactory bitmapFactory, IReadOnlyCollection<RawBitmap> images)
        {
            if (images == null)
                return null;

            var result = new List<Frame>(images.Count);

            int minRow = int.MaxValue, maxRow = int.MinValue;
            int minColumn = int.MaxValue, maxColumn = int.MinValue;
            foreach (var image in images) {
                minRow = Math.Min(minRow, image.MinRow);
                maxRow = Math.Max(maxRow, image.MaxRow);

                minColumn = Math.Min(minColumn, image.MinColumn);
                maxColumn = Math.Max(maxColumn, image.MaxColumn);
            }

            // todo Здесь предполагается, что все кадры анимации имеют одинаковые размеры.
            // Вроде, исключений не наблюдается, но это вполне возможно.
            var bounds = new Bounds(minRow, maxRow, minColumn, maxColumn);
            foreach (var image in images) {
                result.Add(bitmapFactory.FromRawBitmap(image, bounds));
            }

            return result;
        }
    }
}