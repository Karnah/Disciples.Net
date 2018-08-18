using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Avalonia.Media.Imaging;
using Avalonia.Platform;

using Engine.Models;
using ResourceProvider.Models;

namespace Engine.Implementation.Helpers
{
    public static class ImageExtensions
    {
        /// <summary>
        /// Сконвертировать сырое изображение в битмап
        /// </summary>
        public static Bitmap ToBitmap(this RowImage image)
        {
            if (image == null)
                return null;

            return ConvertImageToFrame(image).Bitmap;
        }

        /// <summary>
        /// Получить конечные кадры анимации из сырых
        /// </summary>
        public static IReadOnlyList<Frame> ConvertToFrames(this IReadOnlyCollection<RowImage> images)
        {
            var result = new List<Frame>(images.Count);

            int minRow = int.MaxValue, maxRow = int.MinValue;
            int minColumn = int.MaxValue, maxColumn = int.MinValue;
            foreach (var image in images) {
                minRow = Math.Min(minRow, image.MinRow);
                maxRow = Math.Max(maxRow, image.MaxRow);

                minColumn = Math.Min(minColumn, image.MinColumn);
                maxColumn = Math.Max(maxColumn, image.MaxColumn);
            }

            // todo Здесь можно огрести, если фреймы будут иметь различные размеры
            var bounds = new OpacityBounds(minRow, maxRow, minColumn, maxColumn);
            foreach (var image in images) {
                result.Add(ConvertImageToFrame(image, bounds));
            }

            return result;
        }

        /// <summary>
        /// Получить кадр из сырого изображения
        /// </summary>
        public static Frame ConvertToFrame(this RowImage image)
        {
            return ConvertImageToFrame(image);
        }


        /// <summary>
        /// Получить кадр из сырого изображения
        /// </summary>
        /// <param name="image">Сырое изображение</param>
        /// <param name="opacityBounds">Границы кадра. Если null, то используются границы сырого изображения</param>
        private static Frame ConvertImageToFrame(RowImage image, OpacityBounds opacityBounds = null)
        {
            if (opacityBounds == null) {
                opacityBounds = new OpacityBounds(image.MinRow, image.MaxRow, image.MinColumn, image.MaxColumn);
            }

            var width = opacityBounds.MaxColumn - opacityBounds.MinColumn + 1;
            var height = opacityBounds.MaxRow - opacityBounds.MinRow + 1;

            var bitmap = new WritableBitmap(width, height, PixelFormat.Rgba8888);
            using (var l = bitmap.Lock()) {
                for (int row = opacityBounds.MinRow; row <= opacityBounds.MaxRow; ++row) {
                    var begin = (row * image.Width + opacityBounds.MinColumn) << 2;
                    var length = width << 2;

                    Marshal.Copy(image.Data, begin, new IntPtr(l.Address.ToInt64() + (row - opacityBounds.MinRow) * length), length);
                }
            }

            var scaledWidth = width * GameInfo.Scale;
            var scaledHeight = height * GameInfo.Scale;

            var offsetX = opacityBounds.MinColumn * GameInfo.Scale;
            var offsetY = opacityBounds.MinRow * GameInfo.Scale;

            return new Frame(scaledWidth, scaledHeight, offsetX, offsetY, new Bitmap(bitmap.PlatformImpl));
        }


        private class OpacityBounds
        {
            public OpacityBounds(int minRow, int maxRow, int minColumn, int maxColumn)
            {
                MinRow = minRow;
                MaxRow = maxRow;
                MinColumn = minColumn;
                MaxColumn = maxColumn;
            }


            public int MinRow { get; }

            public int MaxRow { get; }

            public int MinColumn { get; }

            public int MaxColumn { get; }
        }
    }
}
