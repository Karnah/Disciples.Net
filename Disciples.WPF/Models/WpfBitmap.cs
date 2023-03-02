using System.Windows.Media;
using Disciples.Engine;

namespace Disciples.WPF.Models;

/// <summary>
/// Изображение WPF.
/// </summary>
public class WpfBitmap : IBitmap
{
    private readonly ImageSource _imageSource;

    /// <inheritdoc />
    public WpfBitmap(ImageSource imageSource)
    {
        _imageSource = imageSource;
    }


    /// <inheritdoc />
    public double Width => _imageSource.Width;

    /// <inheritdoc />
    public double Height => _imageSource.Height;

    /// <inheritdoc />
    public object BitmapData => _imageSource;
}