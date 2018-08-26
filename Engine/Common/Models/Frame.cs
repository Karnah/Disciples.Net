using Avalonia.Media.Imaging;

namespace Engine.Common.Models
{
    public class Frame
    {
        public Frame(double width, double height, double offsetX, double offsetY, Bitmap bitmap)
        {
            Width = width;
            Height = height;
            OffsetX = offsetX;
            OffsetY = offsetY;
            Bitmap = bitmap;
        }


        /// <summary>
        /// Ширина изображения
        /// </summary>
        public double Width { get; }

        /// <summary>
        /// Высота изображения
        /// </summary>
        public double Height { get; }

        /// <summary>
        /// Дополнительное смещение по оси X (вправо)
        /// </summary>
        public double OffsetX { get; }

        /// <summary>
        /// Дополнительно смещение по оси Y (вниз)
        /// </summary>
        public double OffsetY { get; }

        /// <summary>
        /// Изображение
        /// </summary>
        public Bitmap Bitmap { get; }
    }
}
