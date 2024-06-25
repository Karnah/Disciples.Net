using System;

namespace Disciples.Engine.Models;

/// <summary>
/// Обёртка над проигрываемой музыкой/звуком.
/// </summary>
public interface IPlayingSound : IDisposable
{
    /// <summary>
    /// Признак, что музыка завершила проигрывание.
    /// </summary>
    bool IsCompleted { get; }

    /// <summary>
    /// Позиция проигрывания звука.
    /// </summary>
    TimeSpan PlayingPosition { get; }

    /// <summary>
    /// Длительность звука.
    /// </summary>
    TimeSpan Duration { get; }

    /// <summary>
    /// Остановить проигрывание музыки.
    /// </summary>
    void Stop();
}