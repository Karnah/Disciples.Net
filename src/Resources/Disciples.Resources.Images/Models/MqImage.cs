namespace Disciples.Resources.Images.Models;

/// <summary>
/// Класс для изображения или кадра анимации.
/// </summary>
internal class MqImage
{
    /// <summary>
    /// Создать объект типа <see cref="MqImage" />.
    /// </summary>
    public MqImage(string name, int fileId, int width, int height, MqImageMetadata metadata, IReadOnlyList<MqImagePart> imageParts)
    {
        Name = name;
        FileId = fileId;
        Width = width;
        Height = height;
        Metadata = metadata;
        ImageParts = imageParts;
    }

    /// <summary>
    /// Название изображения.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Идентификатор файла, который содержит базовое изображение.
    /// </summary>
    public int FileId { get; }

    /// <summary>
    /// Ширина изображения.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Высота изображения.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Дополнительная информация про изображение.
    /// </summary>
    public MqImageMetadata Metadata { get; }

    /// <summary>
    /// Информация о частях, которая позволит собрать изображение из базового.
    /// </summary>
    public IReadOnlyList<MqImagePart> ImageParts { get; }
}