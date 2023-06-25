using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Disciples.Avalonia.Models;
using Disciples.Engine;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Implementation.Common.Controllers;
using IBitmap = Disciples.Engine.IBitmap;
using IAvaloniaBitmap = Avalonia.Media.Imaging.Bitmap;

namespace Disciples.Avalonia.Managers;

/// <inheritdoc />
internal class AvaloniaCursorController : BaseCursorController
{
    private readonly AvaloniaGameInfo _gameInfo;

    private Cursor? _defaultCursor;
    private Cursor? _noneCursor;

    /// <summary>
    /// Создать объект типа <see cref="AvaloniaCursorController" />.
    /// </summary>
    public AvaloniaCursorController(IInterfaceProvider interfaceProvider, AvaloniaGameInfo gameInfo) : base(interfaceProvider)
    {
        _gameInfo = gameInfo;
    }

    /// <inheritdoc />
    protected override async void SetDefaultCursorState()
    {
        await ExecuteMainThreadAsync(window =>
        {
            if (_defaultCursor == null)
                _defaultCursor = new Cursor(GetScaledBitmap(GetDefaultCursorBitmap()), PixelPoint.Origin);

            window.Cursor = _defaultCursor;
        });
    }

    /// <inheritdoc />
    protected override async void SetNoneCursorState()
    {
        await ExecuteMainThreadAsync(window =>
        {
            if (_noneCursor == null)
                _noneCursor = new Cursor(StandardCursorType.None);

            window.Cursor = _noneCursor;
        });
    }

    /// <summary>
    /// Получить увеличенное изображение.
    /// </summary>
    private static IAvaloniaBitmap GetScaledBitmap(IBitmap originalBitmap)
    {
        var originalAvaloniaBitmap = (IAvaloniaBitmap)((AvaloniaBitmap)originalBitmap).BitmapData;
        if (Math.Abs(GameInfo.Scale - 1) < 0.001)
            return originalAvaloniaBitmap;

        var pixelSize = new PixelSize(
            (int) (originalAvaloniaBitmap.PixelSize.Width * GameInfo.Scale),
            (int) (originalAvaloniaBitmap.PixelSize.Height * GameInfo.Scale));
        var size = new Size(
            originalAvaloniaBitmap.Size.Width * GameInfo.Scale,
            originalAvaloniaBitmap.Size.Height * GameInfo.Scale);
        var target = new Image
        {
            Source = originalAvaloniaBitmap
        };

        var bitmap = new RenderTargetBitmap(pixelSize, new Vector(96, 96));
        target.Measure(size);
        target.Arrange(new Rect(size));
        bitmap.Render(target);
        return bitmap;
    }

    /// <summary>
    /// Выполнить асинхронное действие с окном в главном потоке.
    /// </summary>
    private async Task ExecuteMainThreadAsync(Action<Window> action)
    {
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            action(_gameInfo.OverlapWindow);
        });
    }
}