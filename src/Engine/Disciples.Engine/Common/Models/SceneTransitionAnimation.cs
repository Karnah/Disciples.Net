namespace Disciples.Engine.Common.Models;

/// <summary>
/// Анимация перехода между сценами.
/// </summary>
/// <remarks>
/// На самом деле для анимации перехода используется видео в формате bik.
/// </remarks>
public class SceneTransitionAnimation
{
    /// <summary>
    /// Создать объект типа <see cref="SceneTransitionAnimation" />.
    /// </summary>
    public SceneTransitionAnimation(byte[] data)
    {
        Data = data;
    }

    /// <summary>
    /// Видеоролик перехода.
    /// </summary>
    public byte[] Data { get; }
}