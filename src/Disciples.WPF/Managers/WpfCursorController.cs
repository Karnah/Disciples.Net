using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Disciples.Engine;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Implementation.Common.Controllers;
using Disciples.WPF.Models;

namespace Disciples.WPF.Managers;

/// <inheritdoc />
internal class WpfCursorController : BaseCursorController
{
    private Cursor? _defaultCursor;

    /// <summary>
    /// Создать объект типа <see cref="WpfCursorController" />.
    /// </summary>
    public WpfCursorController(IInterfaceProvider interfaceProvider) : base(interfaceProvider)
    {
    }

    /// <inheritdoc />
    protected override async void SetDefaultCursorState()
    {
        await SetCursorAsync(() =>
        {
            if (_defaultCursor == null)
            {
                _defaultCursor = CreateCursor(GetScaledBitmap(GetDefaultCursorBitmap()));
            }

            return _defaultCursor;
        }).ConfigureAwait(false);
    }

    /// <inheritdoc />
    protected override async void SetNoneCursorState()
    {
        await SetCursorAsync(() => Cursors.None).ConfigureAwait(false);
    }

    /// <summary>
    /// Получить увеличенное изображение.
    /// </summary>
    private static BitmapSource GetScaledBitmap(IBitmap originalBitmap)
    {
        var originalWpfBitmap = (BitmapSource)((WpfBitmap)originalBitmap).BitmapData;
        if (Math.Abs(GameInfo.Scale - 1) < 0.001)
            return originalWpfBitmap;

        var width = originalWpfBitmap.Width * GameInfo.Scale;
        var height = originalWpfBitmap.Height * GameInfo.Scale;
        var rect = new Rect(0, 0, width, height);
        var group = new DrawingGroup();
        RenderOptions.SetBitmapScalingMode(group, BitmapScalingMode.HighQuality);
        group.Children.Add(new ImageDrawing(originalWpfBitmap, rect));

        var drawingVisual = new DrawingVisual();
        using (var drawingContext = drawingVisual.RenderOpen())
            drawingContext.DrawDrawing(group);

        var resizedImage = new RenderTargetBitmap(
            (int)width, (int)height,
            96, 96,
            PixelFormats.Default);
        resizedImage.Render(drawingVisual);

        return BitmapFrame.Create(resizedImage);
    }

    /// <summary>
    /// Получить курсор с указанным изображением.
    /// </summary>
    private static Cursor CreateCursor(BitmapSource bitmapSource)
    {
        using var ms1 = new MemoryStream();
        var pngEncoder = new PngBitmapEncoder();
        pngEncoder.Frames.Add(BitmapFrame.Create(bitmapSource));
        pngEncoder.Save(ms1);

        var pngBytes = ms1.ToArray();
        var size = pngBytes.GetLength(0);

        using (var ms = new MemoryStream())
        {
            // Reserved must be zero; 2 bytes.
            ms.Write(BitConverter.GetBytes((short)0), 0, 2);

            // Image type 1 = ico 2 = cur; 2 bytes.
            ms.Write(BitConverter.GetBytes((short)2), 0, 2);

            // Number of images; 2 bytes.
            ms.Write(BitConverter.GetBytes((short)1), 0, 2);

            // Image width in pixels.
            ms.WriteByte(32);

            // Image height in pixels.
            ms.WriteByte(32);

            // Number of Colors in the color palette.
            // Should be 0 if the image doesn't use a color palette.
            ms.WriteByte(0);

            // Reserved must be 0.
            ms.WriteByte(0);

            // 2 bytes. In CUR format: Specifies the horizontal coordinates of the hotspot in number of pixels from the left.
            ms.Write(BitConverter.GetBytes(0), 0, 2);
            // 2 bytes. In CUR format: Specifies the vertical coordinates of the hotspot in number of pixels from the top.
            ms.Write(BitConverter.GetBytes(0), 0, 2);

            // Specifies the size of the image's data in bytes.
            ms.Write(BitConverter.GetBytes(size), 0, 4);

            // Specifies the offset of BMP or PNG data from the beginning of the ICO/CUR file.
            ms.Write(BitConverter.GetBytes(22), 0, 4);

            ms.Write(pngBytes, 0, size);
            ms.Seek(0, SeekOrigin.Begin);
            return new Cursor(ms);
        }
    }

    /// <summary>
    /// Установить курсор.
    /// </summary>
    private static async Task SetCursorAsync(Func<Cursor> action)
    {
        var applicationDispatcher = Application.Current.Dispatcher;
        if (applicationDispatcher.CheckAccess())
        {
            Mouse.OverrideCursor = action.Invoke();
            return;
        }

        await applicationDispatcher.InvokeAsync(() =>
        {
            Mouse.OverrideCursor = action.Invoke();
        }).Task.ConfigureAwait(false);
    }
}