using System;
using System.Drawing;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using Disciples.Engine;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Platform.Factories;
using Disciples.Engine.Platform.Models;
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
        // Если границы не указаны явно, то берём наименьшее возможное изображение.
        if (bounds == null)
            bounds = new Bounds(rawBitmap.MinRow, rawBitmap.MaxRow, rawBitmap.MinColumn, rawBitmap.MaxColumn);

        var width = bounds.MaxColumn - bounds.MinColumn;
        var height = bounds.MaxRow - bounds.MinRow;

        var pixelFormat = PixelFormats.Bgra32;
        // Размер строки = ширина изображения * 4 (количество байт, которым кодируется один пиксель).
        int stride = width << 2;

        var data = new byte[stride * height];
        for (int row = bounds.MinRow; row < bounds.MaxRow; ++row)
        {
            var begin = (row * rawBitmap.Width + bounds.MinColumn) << 2;
            Buffer.BlockCopy(rawBitmap.Data, begin, data, (row - bounds.MinRow) * stride, stride);
        }

        var bitmapSource = BitmapSource.Create(width, height, 96, 96, pixelFormat, null, data, stride);
        bitmapSource.Freeze();

        var offsetX = bounds.MinColumn;
        var offsetY = bounds.MinRow;

        // Если изображение занимает весь экран, то это, вероятно, анимации юнитов.
        // Чтобы юниты отображались на своих местах, координаты конечного изображения приходится смещать далеко в минус.
        // Чтобы иметь нормальные координаты, здесь производим перерасчёт.
        if (Math.Abs(rawBitmap.Width - GameInfo.OriginalWidth) < float.Epsilon
            && Math.Abs(rawBitmap.Height - GameInfo.OriginalHeight) < float.Epsilon)
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
}