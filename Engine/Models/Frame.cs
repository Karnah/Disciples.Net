using Avalonia.Media.Imaging;

namespace Engine.Models
{
    public class Frame
    {
        public double Width { get; set; }

        public double Height { get; set; }

        public double OffsetX { get; set; }

        public double OffsetY { get; set; }

        public Bitmap Bitmap { get; set; }
    }
}
