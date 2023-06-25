using System.Windows;
using Disciples.WPF.SceneObjects;
using LibVLCSharp.WPF;

namespace Disciples.WPF.Controls;

/// <summary>
/// Контрол для автоматического воспроизведения видео.
/// </summary>
public class AutoPlayVideoView : VideoView
{
    /// <summary>
    /// Создать объект типа <see cref="AutoPlayVideoView" />.
    /// </summary>
    public AutoPlayVideoView()
    {
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
    }

    /// <summary>
    /// Видео на сцене.
    /// </summary>
    public static readonly DependencyProperty VideoSceneObjectProperty = DependencyProperty.Register(
        nameof(VideoSceneObject), typeof(VideoSceneObject), typeof(AutoPlayVideoView), new PropertyMetadata(default(VideoSceneObject)));

    /// <summary>
    /// Видео на сцене.
    /// </summary>
    public VideoSceneObject? VideoSceneObject
    {
        get => (VideoSceneObject?)GetValue(VideoSceneObjectProperty);
        set => SetValue(VideoSceneObjectProperty, value);
    }

    /// <summary>
    /// Обработать событие загрузки контрола.
    /// </summary>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        VideoSceneObject?.Play();
    }

    /// <summary>
    /// Обработать событие удаление контрола.
    /// </summary>
    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        VideoSceneObject?.Stop();
    }
}