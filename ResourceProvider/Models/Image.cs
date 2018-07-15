namespace ResourceProvider.Models
{
    public class Image
    {
        public Image(int minRow, int maxRow, int minColumn, int maxColumn, int width, int height, byte[] data)
        {
            MinRow = minRow;
            MaxRow = maxRow;
            MinColumn = minColumn;
            MaxColumn = maxColumn;
            Width = width;
            Height = height;
            Data = data;
        }


        public int MinRow { get; }

        public int MaxRow { get; set; }

        public int MinColumn { get; }

        public int MaxColumn { get; }


        public int Width { get; }

        public int Height { get; }

        public byte[] Data { get; }
    }
}
