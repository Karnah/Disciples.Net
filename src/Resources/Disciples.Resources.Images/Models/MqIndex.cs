namespace Disciples.Resources.Images.Models;

internal class MqIndex
{
    /// <summary>
    /// Создать объект типа <see cref="MqIndex" />.
    /// </summary>
    public MqIndex(int id, string name, int relatedOffset, int size)
    {
        Id = id;
        Name = name;
        RelatedOffset = relatedOffset;
        Size = size;
    }

    /// <summary>
    /// Идентификатор файла.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Наименование изображения, которое содержится в файле.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Относительное расположение файла в файле ресурсов.
    /// </summary>
    public int RelatedOffset { get; }

    /// <summary>
    /// Размер файла.
    /// </summary>
    public int Size { get; }
}