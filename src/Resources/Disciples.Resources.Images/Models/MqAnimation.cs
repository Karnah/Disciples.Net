namespace Disciples.Resources.Images.Models;

internal class MqAnimation
{
    public MqAnimation(int index, string name, IReadOnlyCollection<MqImage> frames)
    {
        Index = index;
        Name = name;
        Frames = frames;
    }

    /// <summary>
    /// Идентификатор анимации
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// Название анимации
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Фреймы из которых состоит анимация
    /// </summary>
    public IReadOnlyCollection<MqImage> Frames { get; }
}