using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Disciples.Avalonia.Models;
using Disciples.Common.Models;
using Disciples.Engine.Platform.Factories;
using Disciples.Resources.Images.Models;
using Bitmap = Avalonia.Media.Imaging.Bitmap;
using IBitmap = Disciples.Engine.IBitmap;

namespace Disciples.Avalonia.Factories;

/// <inheritdoc />
public class AvaloniaBitmapFactory : IBitmapFactory
{
    /// <inheritdoc />
    public IBitmap FromByteArray(byte[] bitmapData)
    {
        using var memoryStream = new MemoryStream(bitmapData);
        var bitmap = new Bitmap(memoryStream);
        return new AvaloniaBitmap(bitmap);
    }

    /// <inheritdoc />
    public IBitmap FromFile(string filePath)
    {
        var bitmap = new Bitmap(filePath);
        return new AvaloniaBitmap(bitmap);
    }

    /// <inheritdoc />
    public IBitmap FromRawBitmap(RawBitmap rawBitmap, Rectangle? bounds = null)
    {
        var resultBounds = bounds == null
            ? rawBitmap.Bounds
            : Rectangle.Intersect(bounds.Value, rawBitmap.Bounds);
        var width = resultBounds.Width;
        var height = resultBounds.Height;
        var dpi = new Vector(96, 96);

        var bitmap = new WriteableBitmap(new PixelSize(width, height), dpi, PixelFormat.Bgra8888, AlphaFormat.Unpremul);
        using (var l = bitmap.Lock())
        {
            if (resultBounds == rawBitmap.Bounds)
            {
                Marshal.Copy(rawBitmap.Data, 0, new IntPtr(l.Address.ToInt64()), rawBitmap.Data.Length);
            }
            else
            {
                // Каждый пиксель кодируется 4 байтами.
                var sourceRowLength = rawBitmap.Bounds.Width * 4;
                var sourceOffsetColumnPixels = (resultBounds.X - rawBitmap.Bounds.X) * 4;
                var sourcePosition = resultBounds.Y * sourceRowLength + sourceOffsetColumnPixels;

                var targetRowLength = width * 4;

                for (int row = 0; row < height; ++row, sourcePosition += sourceRowLength)
                {
                    Marshal.Copy(rawBitmap.Data, sourcePosition,
                        new IntPtr(l.Address.ToInt64() + row * targetRowLength), targetRowLength);
                }
            }
        }

        var originalSize = new SizeD(bounds?.Width ?? rawBitmap.OriginalWidth, bounds?.Height ?? rawBitmap.OriginalHeight);
        var offset = new PointD(Math.Max(0, rawBitmap.Bounds.X - (bounds?.X ?? 0)), Math.Max(0, rawBitmap.Bounds.Y - (bounds?.Y ?? 0)));
        return new AvaloniaBitmap(bitmap, originalSize, offset);
    }

    /// <inheritdoc />
    public void SaveToFile(IBitmap bitmap, string filePath)
    {
        var avaloniaBitmapData = bitmap.BitmapData as Bitmap;
        avaloniaBitmapData?.Save(filePath);
    }
}