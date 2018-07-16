using System;
using System.Runtime.InteropServices;

using Avalonia.Media.Imaging;
using Avalonia.Platform;

using ResourceProvider.Models;

namespace AvaloniaDisciplesII.Helpers
{
    public static class ImageExtensions
    {
        public static Bitmap ToBitmap(this Image image)
        {
            if (image == null)
                return null;

            var bitmap = new WritableBitmap(image.Width, image.Height, PixelFormat.Rgba8888);
            using (var l = bitmap.Lock()) {
                for (int row = 0; row < image.Height; ++row) {
                    var begin = (row * image.Width) << 2;
                    var length = image.Width << 2;

                    Marshal.Copy(image.Data, begin, new IntPtr(l.Address.ToInt64() + row * length), length);
                }
            }

            return bitmap;
        }
    }
}
