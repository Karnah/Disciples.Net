using Disciples.Engine.Models;

namespace Disciples.Engine.Implementation.Models;

/// <summary>
/// Не проигрываемый звук.
/// </summary>
internal class NullPlayingSound : IPlayingSound
{
    /// <inheritdoc />
    public bool IsCompleted { get; private set; }

    /// <inheritdoc />
    public void Stop()
    {
        IsCompleted = true;
    }
}