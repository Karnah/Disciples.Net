using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

using Engine.Common.Models;
using ResourceProvider.Models;

namespace Engine.Implementation.Helpers
{
    public static class ImageExtensions
    {
        /// <summary>
        /// Уменьшение смещение для больших изображений по оси X.
        /// </summary>
        private const int BIG_FRAME_OFFSET_X = 380;
        /// <summary>
        /// Уменьшение смещение для больших изображений по оси Y.
        /// </summary>
        private const int BIG_FRAME_OFFSET_Y = 410;


        /// <summary>
        /// Сконвертировать сырое изображение в битмап.
        /// </summary>
        public static Bitmap ToBitmap(this byte[] content, Bounds bounds = null)
        {
            if (content == null)
                return null;

            using (var memoryStream = new MemoryStream(content)) {
                return new Bitmap(memoryStream);
            }
        }

        /// <summary>
        /// Сконвертировать сырое изображение в битмап.
        /// </summary>
        public static Bitmap ToBitmap(this RowImage image, Bounds bounds = null)
        {
            if (image == null)
                return null;

            return ConvertImageToFrame(image, bounds).Bitmap;
        }

        /// <summary>
        /// Сконвертировать сырое изображение в битмап, сохраняя исходные размеры.
        /// </summary>
        public static Bitmap ToOriginalBitmap(this RowImage image)
        {
            if (image == null)
                return null;

            return ConvertImageToFrame(image, new Bounds(0, image.Height, 0, image.Width)).Bitmap;
        }

        /// <summary>
        /// Получить конечные кадры анимации из сырых.
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
            var bounds = new Bounds(minRow, maxRow, minColumn, maxColumn);
            foreach (var image in images) {
                result.Add(ConvertImageToFrame(image, bounds));
            }

            return result;
        }

        /// <summary>
        /// Получить кадр из сырого изображения.
        /// </summary>
        public static Frame ConvertToFrame(this RowImage image)
        {
            return ConvertImageToFrame(image);
        }


        /// <summary>
        /// Получить кадр из сырого изображения.
        /// </summary>
        /// <param name="image">Сырое изображение.</param>
        /// <param name="bounds">Границы кадра. Если null, то используются границы сырого изображения.</param>
        private static Frame ConvertImageToFrame(RowImage image, Bounds bounds = null)
        {
            if (bounds == null) {
                bounds = new Bounds(image.MinRow, image.MaxRow, image.MinColumn, image.MaxColumn);
            }

            var width = bounds.MaxColumn - bounds.MinColumn;
            var height = bounds.MaxRow - bounds.MinRow;

            var bitmap = new WriteableBitmap(new PixelSize(width, height), new Vector(), PixelFormat.Rgba8888);
            using (var l = bitmap.Lock()) {
                for (int row = bounds.MinRow; row < bounds.MaxRow; ++row) {
                    var begin = (row * image.Width + bounds.MinColumn) << 2;
                    var length = width << 2;

                    Marshal.Copy(image.Data, begin, new IntPtr(l.Address.ToInt64() + (row - bounds.MinRow) * length), length);
                }
            }

            var scaledWidth = width;
            var scaledHeight = height;

            var offsetX = bounds.MinColumn;
            var offsetY = bounds.MinRow;

            // Если изображение занимает весь экран, то это, вероятно, анимации юнитов.
            // Чтобы юниты отображались на своих местах, координаты конечного изображения приходится смещать далеко в минус.
            // Чтобы иметь нормальные координаты, здесь производим перерасчёт.
            if (image.Width == GameInfo.OriginalWidth && image.Height == GameInfo.OriginalHeight) {
                offsetX -= BIG_FRAME_OFFSET_X;
                offsetY -= BIG_FRAME_OFFSET_Y;
            }

            return new Frame(scaledWidth, scaledHeight, offsetX, offsetY, bitmap);
        }


        public class Bounds
        {
            public Bounds(int minRow, int maxRow, int minColumn, int maxColumn)
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
