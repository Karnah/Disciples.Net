namespace Disciples.Resources.Sounds.Models;

/// <summary>
/// Данные звукового файла.
/// </summary>
public class RawSound
{
    /// <summary>
    /// Создать объект типа <see cref="RawSound" />.
    /// </summary>
    public RawSound(byte[] data)
    {
        Data = data;
    }

    /// <summary>
    /// Данные.
    /// </summary>
    public byte[] Data { get; }
}