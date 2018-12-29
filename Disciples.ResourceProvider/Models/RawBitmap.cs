namespace Disciples.ResourceProvider.Models
{
    /// <summary>
    /// Изображение, представленное массивом пикселей.
    /// </summary>
    public class RawBitmap
    {
        /// <inheritdoc />
        public RawBitmap(int minRow, int maxRow, int minColumn, int maxColumn, int width, int height, byte[] data)
        {
            MinRow = minRow;
            MaxRow = maxRow;
            MinColumn = minColumn;
            MaxColumn = maxColumn;
            Width = width;
            Height = height;
            Data = data;
        }


        /// <summary>
        /// Все пиксели выше этой строки - прозрачные.
        /// </summary>
        public int MinRow { get; }

        /// <summary>
        /// Все пиксели на этой строке и ниже - прозрачные.
        /// </summary>
        public int MaxRow { get; }

        /// <summary>
        /// Все пиксели меньше этой колонки - прозрачные.
        /// </summary>
        public int MinColumn { get; }

        /// <summary>
        /// Все пиксели на этой колонке и дальше - прозрачные.
        /// </summary>
        public int MaxColumn { get; }


        /// <summary>
        /// Ширина всего изображения.
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Высота всего изображения.
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Массив байт, содержащий пиксели в RGBA.
        /// </summary>
        /// <remarks>Имеет размеры Width * Height * 4.</remarks>
        public byte[] Data { get; }
    }
}