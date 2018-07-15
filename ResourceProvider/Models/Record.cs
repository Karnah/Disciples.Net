namespace ResourceProvider.Models
{
    internal class Record
    {
        public Record(int id, int size, long offset)
        {
            Id = id;
            Size = size;
            Offset = offset;
        }


        public int Id { get; }

        public int Size { get; }

        public long Offset { get; }
    }
}
