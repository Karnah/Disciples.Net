using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Disciples.Engine;
using DryIoc;

namespace Disciples.Avalonia;

public partial class GameWindow : Window
{
    public GameWindow()
    {
        DataContext = ((App)Application.Current!).Container.Resolve<GameWindowViewModel>();

        Activated += OnActivated;

        AvaloniaXamlLoader.Load(this);

#if DEBUG
        this.AttachDevTools();
#endif
    }

    /// <summary>
    /// Обработать событие отображения окна.
    /// </summary>
    private void OnActivated(object? sender, EventArgs e)
    {
        var screen = this.Screens.ScreenFromVisual(this);

        // Если окно открыть и тут же закрыть, screen будет null.
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (screen == null)
            return;

        // Рассчитываем реальные размер экрана и пропорционально растягиваем изображение.
        var scale = Math.Min(screen.WorkingArea.Width / GameInfo.OriginalWidth, screen.WorkingArea.Height / GameInfo.OriginalHeight);

        var field = this.Find<Grid>("Field");
        field.RenderTransform = new ScaleTransform(scale, scale);

        GameInfo.Width = scale * GameInfo.OriginalWidth;
        GameInfo.Height = scale * GameInfo.OriginalHeight;
        GameInfo.Scale = scale;
    }
}