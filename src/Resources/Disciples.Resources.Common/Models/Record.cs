namespace Disciples.Resources.Common.Models;

/// <summary>
/// Запись в файле в ресурса.
/// </summary>
public class Record
{
    public Record(int id, int size, long offset)
    {
        Id = id;
        Size = size;
        Offset = offset;
    }

    /// <summary>
    /// Идентификатор записи
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Размер записи
    /// </summary>
    public int Size { get; }

    /// <summary>
    /// Смещение записи относительно начала файла ресурсов
    /// </summary>
    public long Offset { get; }
}