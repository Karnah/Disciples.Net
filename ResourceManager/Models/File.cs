using System.Collections.Generic;

namespace DII.ResourceExtractor.Models
{
    internal class File
    {
        public File(int id, int size, long offset, string name)
        {
            Id = id;
            Size = size;
            Offset = offset;
            Name = name;
            Content = null;
            Frames = new List<Frame>();
        }


        public int Id { get; }

        public int Size { get; }

        public long Offset { get; }

        public string Name { get; }

        public byte[] Content { get; set; }

        public List<Frame> Frames { get; set; }
    }
}
