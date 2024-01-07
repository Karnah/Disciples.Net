using System.IO;
using Avalonia.Controls;
using Disciples.Common.Models;
using Disciples.Engine;
using Disciples.Engine.Common.SceneObjects;
using LibVLCSharp.Shared;

namespace Disciples.Avalonia.SceneObjects;

/// <inheritdoc cref="IVideoSceneObject" />
public class VideoSceneObject : BaseSceneObject, IVideoSceneObject
{
    private static readonly LibVLC LibVlc = new(false);
    private readonly Media _media;

    /// <summary>
    /// Создать объект типа <see cref="VideoSceneObject" />.
    /// </summary>
    public VideoSceneObject(Stream stream, RectangleD bounds, int layer) : base(ScaleBounds(bounds), layer)
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

    /// <inheritdoc />
    public override void Destroy()
    {
        base.Destroy();

        MediaPlayer.Stop();
        MediaPlayer.Dispose();
    }

    /// <summary>
    /// Отмасштабировать границы.
    /// </summary>
    /// <remarks>
    /// Видео отображается в <see cref="NativeControlHost" />, который не поддерживает RenderTransform.
    /// Поэтому масштабируем здесь.
    /// </remarks>
    private static RectangleD ScaleBounds(RectangleD originalBounds)
    {
        return new RectangleD(
            originalBounds.X * GameInfo.Scale,
            originalBounds.Y * GameInfo.Scale,
            originalBounds.Width * GameInfo.Scale,
            originalBounds.Height * GameInfo.Scale);
    }
}