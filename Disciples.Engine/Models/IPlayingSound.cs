namespace Disciples.Engine.Models;

/// <summary>
/// Обёртка над проигрываемой музыкой/звуком.
/// </summary>
public interface IPlayingSound
{
    /// <summary>
    /// Признак, что музыка завершила проигрывание.
    /// </summary>
    bool IsCompleted { get; }

    /// <summary>
    /// Остановить проигрывание музыки.
    /// </summary>
    void Stop();
}