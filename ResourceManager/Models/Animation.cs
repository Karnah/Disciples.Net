namespace DII.ResourceExtractor.Models
{
    internal class Animation
    {
        public Animation(string name, int fileId)
        {
            Name = name;
            FileId = fileId;
        }


        public string Name { get; }

        public int FileId { get; }
    }
}
