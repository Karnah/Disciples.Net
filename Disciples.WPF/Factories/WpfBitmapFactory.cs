using System;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Disciples.Engine;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Platform.Factories;
using Disciples.Resources.Images.Models;
using Disciples.WPF.Models;

namespace Disciples.WPF.Factories;

/// <inheritdoc />
public class WpfBitmapFactory : IBitmapFactory
{
    /// <summary>
    /// Уменьшение смещение для больших изображений по оси X.
    /// </summary>
    private const int BIG_FRAME_OFFSET_X = 380;
    /// <summary>
    /// Уменьшение смещение для больших изображений по оси Y.
    /// </summary>
    private const int BIG_FRAME_OFFSET_Y = 410;


    /// <inheritdoc />
    public IBitmap FromByteArray(byte[] bitmapData)
    {
        using (var memoryStream = new MemoryStream(bitmapData))
        {
            var bitmap = BitmapFrame.Create(memoryStream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
            return new WpfBitmap(bitmap);
        }
    }

    /// <inheritdoc />
    public IBitmap FromFile(string filePath)
    {
        var bitmap = new BitmapImage(new Uri(filePath, UriKind.Relative));
        bitmap.Freeze();
        return new WpfBitmap(bitmap);
    }

    /// <inheritdoc />
    public Frame FromRawBitmap(RawBitmap rawBitmap, Bounds? bounds = null)
    {
        var resultBounds = bounds ?? rawBitmap.Bounds;
        var width = resultBounds.Width;
        var height = resultBounds.Height;
        var pixelFormat = PixelFormats.Bgra32;
        var dpi = 96;

        // Размер строки = ширина изображения * 4 (количество байт, которым кодируется один пиксель).
        var stride = width << 2;

        var bitmapSource = BitmapSource.Create(width, height, dpi, dpi, pixelFormat, null, GetBitmapByteArray(rawBitmap, resultBounds), stride);
        bitmapSource.Freeze();

        var offsetX = resultBounds.MinColumn;
        var offsetY = resultBounds.MinRow;

        // Если изображение занимает весь экран, то это, вероятно, анимации юнитов.
        // Чтобы юниты отображались на своих местах, координаты конечного изображения приходится смещать далеко в минус.
        // Чтобы иметь нормальные координаты, здесь производим перерасчёт.
        if (Math.Abs(rawBitmap.OriginalWidth - GameInfo.OriginalWidth) < float.Epsilon
            && Math.Abs(rawBitmap.OriginalHeight - GameInfo.OriginalHeight) < float.Epsilon)
        {
            offsetX -= BIG_FRAME_OFFSET_X;
            offsetY -= BIG_FRAME_OFFSET_Y;
        }

        return new Frame(width, height, offsetX, offsetY, new WpfBitmap(bitmapSource));
    }

    /// <inheritdoc />
    public void SaveToFile(IBitmap bitmap, string filePath)
    {
        var bitmapFrame = bitmap.BitmapData as BitmapFrame;
        if (bitmapFrame == null)
        {
            var bitmapSource = bitmap.BitmapData as BitmapSource;
            if (bitmapSource == null)
                return;

            bitmapFrame = BitmapFrame.Create(bitmapSource);
        }

        using (var fileStream = new FileStream(filePath, FileMode.Create))
        {
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(bitmapFrame);
            encoder.Save(fileStream);
        }
    }

    /// <summary>
    /// Получить результирующий массив данных.
    /// </summary>
    private static byte[] GetBitmapByteArray(RawBitmap rawBitmap, Bounds resultBounds)
    {
        if (rawBitmap.Bounds == resultBounds)
            return rawBitmap.Data;

        var width = resultBounds.Width;
        var height = resultBounds.Height;
        var stride = width << 2;

        var data = new byte[stride * height];
        var unionBounds = new Bounds
        {
            MinRow = Math.Max(resultBounds.MinRow, rawBitmap.Bounds.MinRow),
            MaxRow = Math.Min(resultBounds.MaxRow, rawBitmap.Bounds.MaxRow),
            MinColumn = Math.Max(resultBounds.MinColumn, rawBitmap.Bounds.MinColumn),
            MaxColumn = Math.Min(resultBounds.MaxColumn, rawBitmap.Bounds.MaxColumn)
        };

        // Размер итоговой строки = ширина изображения * 4 (количество байт, которым кодируется один пиксель).
        var destinationRowLength = width * 4;

        // Сколько в каждой строке в исходном массиве нужно пропускать пикселей.
        var sourceOffsetColumnPixels = unionBounds.MinColumn - rawBitmap.Bounds.MinColumn;

        // Сколько байт в каждой строке нужно копировать в итоговый массив.
        var copyRowLength = unionBounds.Width * 4;

        for (int row = unionBounds.MinRow; row < unionBounds.MaxRow; ++row)
        {
            var begin = (row * rawBitmap.Bounds.Width + sourceOffsetColumnPixels) * 4;

            Buffer.BlockCopy(rawBitmap.Data, begin, data,
                (row - resultBounds.MinRow) * destinationRowLength, copyRowLength);
        }

        return data;
    }
}