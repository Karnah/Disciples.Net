namespace Disciples.ResourceProvider.Models
{
    internal class File
    {
        public File(int id, string name, int size, long offset)
        {
            Id = id;
            Name = name;
            Size = size;
            Offset = offset;
        }


        /// <summary>
        /// Идентификатор файла
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Имя файла
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Размер файла
        /// </summary>
        public int Size { get; }

        /// <summary>
        /// Позиция начала файла в файле ресуров
        /// </summary>
        public long Offset { get; }
    }
}
