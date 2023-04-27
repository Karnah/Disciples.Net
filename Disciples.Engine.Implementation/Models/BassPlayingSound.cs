using Disciples.Engine.Models;
using ManagedBass;

namespace Disciples.Engine.Implementation.Models;

/// <summary>
/// Проигрываемый звук на основе <see cref="ManagedBass" />.
/// </summary>
internal class BassPlayingSound : IPlayingSound
{
    private readonly int _soundHandle;

    /// <summary>
    /// Создать объект типа <see cref="BassPlayingSound" />.
    /// </summary>
    public BassPlayingSound(int soundHandle)
    {
        _soundHandle = soundHandle;
    }

    /// <inheritdoc />
    public bool IsCompleted => Bass.ChannelIsActive(_soundHandle) != PlaybackState.Playing;

    /// <inheritdoc />
    public void Stop()
    {
        if (IsCompleted)
            return;

        // TODO Реализовать через IDisposable?
        Bass.ChannelStop(_soundHandle);
        Bass.MusicFree(_soundHandle);
    }
}