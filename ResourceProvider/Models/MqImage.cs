using System.Collections.Generic;

namespace ResourceProvider.Models
{
    internal class MqImage
    {
        public MqImage(string name, int fileId, int width, int height, IReadOnlyList<MqImagePiece> imagePieces)
        {
            Name = name;
            FileId = fileId;
            Width = width;
            Height = height;
            ImagePieces = imagePieces;
        }


        /// <summary>
        /// Название изображения
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Идентификатор файла, который содержит базовое изображение
        /// </summary>
        public int FileId { get; }

        /// <summary>
        /// Ширина изображения
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// Высота изображения
        /// </summary>
        public int Height { get; }

        /// <summary>
        /// Информация о частях, которая позволит собрать изображение из базового
        /// </summary>
        public IReadOnlyList<MqImagePiece> ImagePieces { get; }
    }
}
