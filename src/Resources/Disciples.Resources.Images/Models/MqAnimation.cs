namespace Disciples.Resources.Images.Models;

/// <summary>
/// Анимация.
/// </summary>
internal class MqAnimation
{
    /// <summary>
    /// Создать объект типа <see cref="MqAnimation" />.
    /// </summary>
    public MqAnimation(int index, string name, IReadOnlyCollection<MqImage> frames)
    {
        Index = index;
        Name = name;
        Frames = frames;
    }

    /// <summary>
    /// Идентификатор анимации.
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// Название анимации.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Кадры анимации.
    /// </summary>
    public IReadOnlyCollection<MqImage> Frames { get; }
}