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
        public static Bitmap ToBitmap(this Image image)
        {
            if (image == null)
                return null;

            var bitmap = new WritableBitmap(image.Width, image.Height, PixelFormat.Rgba8888);
            using (var l = bitmap.Lock()) {
                for (int row = 0; row < image.Height; ++row) {
                    var begin = (row * image.Width) << 2;
                    var length = image.Width << 2;

                    Marshal.Copy(image.Data, begin, new IntPtr(l.Address.ToInt64() + row * length), length);
                }
            }

            return bitmap;
        }

        public static IReadOnlyList<Frame> ConvertToFrames(this IReadOnlyCollection<Image> images)
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


        private static Frame ConvertImageToFrame(Image image, OpacityBounds opacityBounds)
        {
            var frame = new Frame();

            frame.OffsetX = opacityBounds.MinColumn * GameInfo.Scale;
            frame.OffsetY = opacityBounds.MinRow * GameInfo.Scale;

            var width = opacityBounds.MaxColumn - opacityBounds.MinColumn + 1;
            var height = opacityBounds.MaxRow - opacityBounds.MinRow + 1;

            var bitmap = new WritableBitmap(width, height, PixelFormat.Rgba8888);
            using (var l = bitmap.Lock()) {
                for (int row = opacityBounds.MinRow; row <= opacityBounds.MaxRow; ++row) {
                    var begin = (row * image.Width + opacityBounds.MinColumn) << 2;
                    var length = width << 2;

                    Marshal.Copy(image.Data, begin,
                        new IntPtr(l.Address.ToInt64() + (row - opacityBounds.MinRow) * length), length);
                }
            }

            frame.Width = width * GameInfo.Scale;
            frame.Height = height * GameInfo.Scale;

            frame.Bitmap = bitmap;

            return frame;
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
