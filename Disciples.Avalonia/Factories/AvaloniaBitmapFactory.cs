using System;
using System.IO;
using System.Runtime.InteropServices;

using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

using Disciples.Avalonia.Models;
using Disciples.Engine;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Platform.Factories;
using Disciples.Engine.Platform.Models;
using Disciples.ResourceProvider.Models;

using IBitmap = Disciples.Engine.IBitmap;

namespace Disciples.Avalonia.Factories;

/// <inheritdoc />
public class AvaloniaBitmapFactory : IBitmapFactory
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
        if (bitmapData == null)
            return null;

        using (var memoryStream = new MemoryStream(bitmapData)) {
            var bitmap = new Bitmap(memoryStream);
            return new AvaloniaBitmap(bitmap);
        }
    }

    /// <inheritdoc />
    public IBitmap FromFile(string filePath)
    {
        var bitmap = new Bitmap(filePath);
        return new AvaloniaBitmap(bitmap);
    }

    /// <inheritdoc />
    public Frame FromRawBitmap(RawBitmap rawBitmap, Bounds bounds = null)
    {
        // Если границы не указаны явно, то берём наименьшее возможное изображение.
        if (bounds == null) {
            bounds = new Bounds(rawBitmap.MinRow, rawBitmap.MaxRow, rawBitmap.MinColumn, rawBitmap.MaxColumn);
        }

        var width = bounds.MaxColumn - bounds.MinColumn;
        var height = bounds.MaxRow - bounds.MinRow;
        var dpi = new Vector(96, 96); 

        var bitmap = new WriteableBitmap(new PixelSize(width, height), dpi, PixelFormat.Bgra8888, AlphaFormat.Unpremul);
        using (var l = bitmap.Lock()) {
            // Размер строки = ширина изображения * 4 (количество байт, которым кодируется один пиксель).
            var stride = width << 2;

            for (int row = bounds.MinRow; row < bounds.MaxRow; ++row) {
                var begin = (row * rawBitmap.Width + bounds.MinColumn) << 2;

                Marshal.Copy(rawBitmap.Data, begin,
                    new IntPtr(l.Address.ToInt64() + (row - bounds.MinRow) * stride), stride);
            }
        }

        var offsetX = bounds.MinColumn;
        var offsetY = bounds.MinRow;

        // Если изображение занимает весь экран, то это, вероятно, анимации юнитов.
        // Чтобы юниты отображались на своих местах, координаты конечного изображения приходится смещать далеко в минус.
        // Чтобы иметь нормальные координаты, здесь производим перерасчёт.
        if (rawBitmap.Width == GameInfo.OriginalWidth && rawBitmap.Height == GameInfo.OriginalHeight) {
            offsetX -= BIG_FRAME_OFFSET_X;
            offsetY -= BIG_FRAME_OFFSET_Y;
        }

        return new Frame(width, height, offsetX, offsetY, new AvaloniaBitmap(bitmap));
    }


    /// <inheritdoc />
    public void SaveToFile(IBitmap bitmap, string filePath)
    {
        var avaloniaBitmapData = bitmap.BitmapData as Bitmap;
        avaloniaBitmapData?.Save(filePath);
    }
}