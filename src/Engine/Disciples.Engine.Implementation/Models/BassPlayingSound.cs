using System;
using System.Diagnostics;
using Disciples.Engine.Models;
using ManagedBass;

namespace Disciples.Engine.Implementation.Models;

/// <summary>
/// Проигрываемый звук на основе <see cref="ManagedBass" />.
/// </summary>
internal class BassPlayingSound : IPlayingSound
{
    private readonly int _soundHandle;
    private readonly Stopwatch _playingStopwatch;

    private bool _isDisposed;

    /// <summary>
    /// Создать объект типа <see cref="BassPlayingSound" />.
    /// </summary>
    public BassPlayingSound(int soundHandle)
    {
        _soundHandle = soundHandle;
        _playingStopwatch = Stopwatch.StartNew();

        var byteLength = Bass.ChannelGetLength(_soundHandle);
        var durationSeconds = Bass.ChannelBytes2Seconds(_soundHandle, byteLength);
        Duration = TimeSpan.FromSeconds(durationSeconds);
    }

    ~BassPlayingSound()
    {
        ReleaseUnmanagedResources();
    }

    /// <inheritdoc />
    public TimeSpan PlayingPosition => _playingStopwatch.Elapsed;

    /// <inheritdoc />
    public TimeSpan Duration { get; }

    /// <inheritdoc />
    public bool IsCompleted => _isDisposed || PlayingPosition >= Duration;

    /// <inheritdoc />
    public void Stop()
    {
        Dispose();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _playingStopwatch.Stop();
        ReleaseUnmanagedResources();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Освободить неуправляемые ресурсы.
    /// </summary>
    private void ReleaseUnmanagedResources()
    {
        if (_isDisposed)
            return;

        Bass.ChannelStop(_soundHandle);
        Bass.MusicFree(_soundHandle);

        _isDisposed = true;
    }
}