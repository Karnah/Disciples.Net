using System;
using System.Drawing;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Disciples.Common.Models;
using Disciples.Engine;
using Disciples.Engine.Platform.Factories;
using Disciples.Resources.Images.Models;
using Disciples.WPF.Models;

namespace Disciples.WPF.Factories;

/// <inheritdoc />
public class WpfBitmapFactory : IBitmapFactory
{
    private const string IMAGES_DIRECTORY = "TempImages";

    /// <inheritdoc />
    public IBitmap FromByteArray(byte[] bitmapData)
    {
        using var memoryStream = new MemoryStream(bitmapData);
        var bitmap = BitmapFrame.Create(memoryStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
        return new WpfBitmap(bitmap);
    }

    /// <inheritdoc />
    public IBitmap FromFile(string filePath)
    {
        var bitmap = new BitmapImage(new Uri(filePath, UriKind.Relative));
        bitmap.Freeze();
        return new WpfBitmap(bitmap);
    }

    /// <inheritdoc />
    public IBitmap FromRawBitmap(RawBitmap rawBitmap, Rectangle? bounds = null)
    {
        var resultBounds = bounds == null
            ? rawBitmap.Bounds
            : Rectangle.Intersect(bounds.Value, rawBitmap.Bounds);
        var width = Math.Max(resultBounds.Width, 1);
        var height = Math.Max(resultBounds.Height, 1);
        var pixelFormat = PixelFormats.Bgra32;
        var dpi = 96;

        // Размер строки = ширина изображения * 4 (количество байт, которым кодируется один пиксель).
        var stride = width << 2;

        var bitmapSource = BitmapSource.Create(width, height, dpi, dpi, pixelFormat, null, GetBitmapByteArray(rawBitmap, resultBounds), stride);
        bitmapSource.Freeze();

        var originalSize = new SizeD(bounds?.Width ?? rawBitmap.OriginalWidth, bounds?.Height ?? rawBitmap.OriginalHeight);
        var offset = new PointD(Math.Max(0, rawBitmap.Bounds.X - (bounds?.X ?? 0)), Math.Max(0, rawBitmap.Bounds.Y - (bounds?.Y ?? 0)));
        return new WpfBitmap(bitmapSource, originalSize, offset);
    }

    /// <inheritdoc />
    public void SaveToFile(IBitmap bitmap, string fileName)
    {
        if (!Directory.Exists(IMAGES_DIRECTORY))
            Directory.CreateDirectory(IMAGES_DIRECTORY);

        var bitmapFrame = bitmap.BitmapData as BitmapFrame;
        if (bitmapFrame == null)
        {
            var bitmapSource = bitmap.BitmapData as BitmapSource;
            if (bitmapSource == null)
                return;

            bitmapFrame = BitmapFrame.Create(bitmapSource);
        }

        using (var fileStream = new FileStream(Path.Combine(IMAGES_DIRECTORY, fileName), FileMode.Create))
        {
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(bitmapFrame);
            encoder.Save(fileStream);
        }
    }

    /// <summary>
    /// Получить результирующий массив данных.
    /// </summary>
    private static byte[] GetBitmapByteArray(RawBitmap rawBitmap, Rectangle resultBounds)
    {
        // Возвращаем один пустой пиксель.
        if (resultBounds == Rectangle.Empty)
            return new byte[4];

        if (rawBitmap.Bounds == resultBounds)
            return rawBitmap.Data;

        var width = resultBounds.Width;
        var height = resultBounds.Height;
        var stride = width << 2;

        var data = new byte[stride * height];

        // Каждый пиксель кодируется 4 байтами.
        var sourceRowLength = rawBitmap.Bounds.Width * 4;
        var sourceOffsetColumnPixels = (resultBounds.X - rawBitmap.Bounds.X) * 4;
        var sourcePosition = resultBounds.Y * sourceRowLength + sourceOffsetColumnPixels;

        var targetRowLength = width * 4;

        for (int row = 0; row < height; ++row, sourcePosition += sourceRowLength)
        {
            Buffer.BlockCopy(rawBitmap.Data, sourcePosition,
                data, row * targetRowLength, targetRowLength);
        }

        return data;
    }
}