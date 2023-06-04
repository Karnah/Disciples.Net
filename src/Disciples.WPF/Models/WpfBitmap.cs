using System.Windows.Media;
using Disciples.Common.Models;
using Disciples.Engine;

namespace Disciples.WPF.Models;

/// <summary>
/// Изображение WPF.
/// </summary>
public class WpfBitmap : IBitmap
{
    private readonly ImageSource _imageSource;

    /// <summary>
    /// Создать объект типа <see cref="WpfBitmap" />.
    /// </summary>
    public WpfBitmap(ImageSource imageSource)
    {
        _imageSource = imageSource;

        var size = new SizeD(_imageSource.Width, _imageSource.Height);
        OriginalSize = size;
        ActualSize = size;
        Offset = new PointD(0, 0);
    }

    /// <summary>
    /// Создать объект типа <see cref="WpfBitmap" />.
    /// </summary>
    public WpfBitmap(ImageSource imageSource, SizeD originalSize, PointD offset)
    {
        _imageSource = imageSource;

        OriginalSize = originalSize;
        ActualSize = new SizeD(_imageSource.Width, _imageSource.Height);
        Offset = offset;
    }

    /// <inheritdoc />
    public SizeD OriginalSize { get; }

    /// <inheritdoc />
    public SizeD ActualSize { get; }

    /// <inheritdoc />
    public PointD Offset { get; }

    /// <inheritdoc />
    public object BitmapData => _imageSource;
}