﻿using System;
using System.IO;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Disciples.Avalonia.Models;
using Disciples.Engine;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Platform.Factories;
using Disciples.Resources.Images.Models;
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
        using (var memoryStream = new MemoryStream(bitmapData))
        {
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
    public Frame FromRawBitmap(RawBitmap rawBitmap, Bounds? bounds = null)
    {
        var resultBounds = bounds ?? rawBitmap.Bounds;
        var width = resultBounds.Width;
        var height = resultBounds.Height;
        var dpi = new Vector(96, 96);

        var bitmap = new WriteableBitmap(new PixelSize(width, height), dpi, PixelFormat.Bgra8888, AlphaFormat.Unpremul);
        using (var l = bitmap.Lock())
        {
            if (bounds == null || bounds.Value == rawBitmap.Bounds)
            {
                Marshal.Copy(rawBitmap.Data, 0, new IntPtr(l.Address.ToInt64()), rawBitmap.Data.Length);
            }
            else
            {
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

                // Сколько в каждой строке в итоговом массиве нужно пропускать байт.
                var targetOffsetColumnBytes = (unionBounds.MinColumn - resultBounds.MinColumn) * 4;

                // Сколько байт в каждой строке нужно копировать в итоговый массив.
                var copyRowLength = unionBounds.Width * 4;

                for (int row = unionBounds.MinRow; row < unionBounds.MaxRow; ++row)
                {
                    var begin = ((row - rawBitmap.Bounds.MinRow) * rawBitmap.Bounds.Width + sourceOffsetColumnPixels) * 4;

                    Marshal.Copy(rawBitmap.Data, begin,
                        new IntPtr(l.Address.ToInt64() + (row - resultBounds.MinRow) * destinationRowLength + targetOffsetColumnBytes), copyRowLength);
                }
            }
        }

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

        return new Frame(width, height, offsetX, offsetY, new AvaloniaBitmap(bitmap));
    }

    /// <inheritdoc />
    public void SaveToFile(IBitmap bitmap, string filePath)
    {
        var avaloniaBitmapData = bitmap.BitmapData as Bitmap;
        avaloniaBitmapData?.Save(filePath);
    }
}