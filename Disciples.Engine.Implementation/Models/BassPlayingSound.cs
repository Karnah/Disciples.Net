using Disciples.Engine.Models;
using ManagedBass;

namespace Disciples.Engine.Implementation.Models;

/// <summary>
/// Проигрываемый звук на основе <see cref="ManagedBass" />.
/// </summary>
internal class BassPlayingSound : IPlayingSound
{
    private readonly int _soundHandle;

    private bool _isDisposed;

    /// <summary>
    /// Создать объект типа <see cref="BassPlayingSound" />.
    /// </summary>
    public BassPlayingSound(int soundHandle)
    {
        _soundHandle = soundHandle;
    }

    /// <inheritdoc />
    public bool IsCompleted => _isDisposed || Bass.ChannelIsActive(_soundHandle) != PlaybackState.Playing;

    /// <inheritdoc />
    public void Stop()
    {
        if (_isDisposed)
            return;

        // TODO Реализовать через IDisposable?
        Bass.MusicFree(_soundHandle);

        _isDisposed = true;
    }
}