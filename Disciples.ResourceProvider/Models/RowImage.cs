namespace Disciples.ResourceProvider.Models
{
    public class RowImage
    {
        public RowImage(int minRow, int maxRow, int minColumn, int maxColumn, int width, int height, byte[] data)
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
        /// Все пиксели выше этой строки - прозрачные
        /// </summary>
        public int MinRow { get; }

        /// <summary>
        /// Все пиксели на этой строке и ниже - прозрачные
        /// </summary>
        public int MaxRow { get; set; }

        /// <summary>
        /// Все пиксели меньше этой колонки - прозрачные
        /// </summary>
        public int MinColumn { get; }

        /// <summary>
        /// Все пиксели на этой колонке и дальше - прозрачные
        /// </summary>
        public int MaxColumn { get; }


        /// <summary>
        /// Ширина всего изображения
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Высота всего изображения
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Массив байт, содержащий пиксели в RGBA. То есть, имеет размер Width * Height * 4
        /// </summary>
        public byte[] Data { get; }
    }
}
