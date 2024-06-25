using System;
using Disciples.Engine.Models;

namespace Disciples.Engine.Implementation.Models;

/// <summary>
/// Не проигрываемый звук.
/// </summary>
internal class NullPlayingSound : IPlayingSound
{
    /// <inheritdoc />
    public bool IsCompleted { get; } = true;

    /// <inheritdoc />
    public TimeSpan PlayingPosition { get; } = TimeSpan.Zero;

    /// <inheritdoc />
    public TimeSpan Duration { get; } = TimeSpan.Zero;

    /// <inheritdoc />
    public void Stop()
    {
    }

    /// <inheritdoc />
    public void Dispose()
    {
    }
}