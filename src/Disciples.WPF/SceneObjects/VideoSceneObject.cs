using System.IO;
using Disciples.Common.Models;
using Disciples.Engine;
using Disciples.Engine.Common.SceneObjects;
using LibVLCSharp.Shared;

namespace Disciples.WPF.SceneObjects;

/// <inheritdoc cref="IVideoSceneObject" />
public class VideoSceneObject : BaseSceneObject, IVideoSceneObject
{
    private static readonly LibVLC LibVlc = new(false);
    private readonly Media _media;

    /// <summary>
    /// Создать объект типа <see cref="VideoSceneObject" />.
    /// </summary>
    public VideoSceneObject(Stream stream, RectangleD bounds, int layer) : base(bounds, layer)
    {
        _media = new Media(LibVlc, new StreamMediaInput(stream));

        MediaPlayer = new MediaPlayer(LibVlc);
    }

    /// <summary>
    /// Плеер видео.
    /// </summary>
    public MediaPlayer MediaPlayer { get; }

    /// <inheritdoc />
    public bool IsCompleted => _media.State is VLCState.Ended or VLCState.Stopped or VLCState.Error;

    /// <summary>
    /// Начать проигрывание видеоролика.
    /// </summary>
    public void Play()
    {
        MediaPlayer.Play(_media);
    }

    /// <summary>
    /// Остановить проигрывание видеоролика.
    /// </summary>
    public void Stop()
    {
        MediaPlayer.Stop();
        MediaPlayer.Dispose();
    }
}