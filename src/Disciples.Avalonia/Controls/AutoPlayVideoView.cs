using Avalonia;
using Avalonia.Data;
using Avalonia.Interactivity;
using Disciples.Avalonia.SceneObjects;

namespace Disciples.Avalonia.Controls;

/// <summary>
/// Контрол для автоматического воспроизведения видео.
/// </summary>
public class AutoPlayVideoView : VideoView
{
    private VideoSceneObject? _videoSceneObject;

    /// <summary>
    /// Видео на сцене.
    /// </summary>
    public static readonly DirectProperty<AutoPlayVideoView, VideoSceneObject?> VideoSceneObjectProperty =
        AvaloniaProperty.RegisterDirect<AutoPlayVideoView, VideoSceneObject?>(
            nameof(VideoSceneObject),
            o => o.VideoSceneObject,
            (o, v) => o.VideoSceneObject = v,
            defaultBindingMode: BindingMode.TwoWay);

    /// <summary>
    /// Видео на сцене.
    /// </summary>
    public VideoSceneObject? VideoSceneObject
    {
        get => _videoSceneObject;
        set
        {
            if (ReferenceEquals(_videoSceneObject, value))
            {
                return;
            }

            _videoSceneObject = value;
        }
    }

    /// <inheritdoc />
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        VideoSceneObject?.Play();
    }
}