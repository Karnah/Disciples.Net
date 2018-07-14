namespace ResourceManager.Models
{
    public class Image
    {
        public Image(int width, int height, byte[] data)
        {
            Width = width;
            Height = height;
            Data = data;
        }

        public int Width { get; }

        public int Height { get; }

        public byte[] Data { get; }
    }
}
