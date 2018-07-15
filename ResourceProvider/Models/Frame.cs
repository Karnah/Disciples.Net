namespace ResourceProvider.Models
{
    internal class Frame
    {
        public Frame(int id, int animNumber, int seqNumber, string name)
        {
            Id = id;
            AnimNumber = animNumber;
            SeqNumber = seqNumber;
            Name = name;
        }


        public int Id { get; }

        public int AnimNumber { get; }

        public int SeqNumber { get; }

        public string Name { get; }

        public long Offset { get; set; }
    }
}
