using Avalonia.Media.Imaging;
using IBitmap = Disciples.Engine.IBitmap;

namespace Disciples.Avalonia.Platform.Models
{
    /// <summary>
    /// Изображение Avalonia.
    /// </summary>
    public class AvaloniaBitmap : IBitmap
    {
        private readonly Bitmap _bitmap;

        /// <inheritdoc />
        public AvaloniaBitmap(Bitmap bitmap)
        {
            _bitmap = bitmap;
        }


        /// <inheritdoc />
        public double Width => _bitmap.PixelSize.Width;

        /// <inheritdoc />
        public double Height => _bitmap.PixelSize.Height;

        /// <inheritdoc />
        public object BitmapData => _bitmap;
    }
}