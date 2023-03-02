namespace Disciples.ResourceProvider.Models;

internal class MqIndex
{
    public MqIndex(int id, string name, int unknownValue1, int unknownValue2)
    {
        Id = id;
        Name = name;
        UnknownValue1 = unknownValue1;
        UnknownValue2 = unknownValue2;
    }


    /// <summary>
    /// Идентификатор файла
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Наименование изображения, которое содержится в файле
    /// </summary>
    public string Name { get; }

    public int UnknownValue1 { get; }

    public int UnknownValue2 { get; }
}