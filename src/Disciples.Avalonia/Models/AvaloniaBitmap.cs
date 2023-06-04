using Avalonia.Media.Imaging;
using Disciples.Common.Models;
using IBitmap = Disciples.Engine.IBitmap;

namespace Disciples.Avalonia.Models;

/// <summary>
/// Изображение Avalonia.
/// </summary>
public class AvaloniaBitmap : IBitmap
{
    private readonly Bitmap _bitmap;

    /// <summary>
    /// Создать объект типа <see cref="AvaloniaBitmap" />.
    /// </summary>
    public AvaloniaBitmap(Bitmap bitmap)
    {
        _bitmap = bitmap;

        var size = new SizeD(_bitmap.PixelSize.Width, _bitmap.PixelSize.Height);
        OriginalSize = size;
        ActualSize = size;
        Offset = new PointD(0, 0);
    }

    /// <summary>
    /// Создать объект типа <see cref="AvaloniaBitmap" />.
    /// </summary>
    public AvaloniaBitmap(Bitmap bitmap, SizeD originalSize, PointD offset)
    {
        _bitmap = bitmap;

        OriginalSize = originalSize;
        ActualSize = new SizeD(_bitmap.PixelSize.Width, _bitmap.PixelSize.Height);
        Offset = offset;
    }

    /// <inheritdoc />
    public SizeD OriginalSize { get; }

    /// <inheritdoc />
    public SizeD ActualSize { get; }

    /// <inheritdoc />
    public PointD Offset { get; }

    /// <inheritdoc />
    public object BitmapData => _bitmap;
}