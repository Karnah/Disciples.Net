using System;
using System.Windows;
using System.Windows.Media;
using Disciples.Engine;

namespace Disciples.WPF;

/// <summary>
/// Окно игры.
/// </summary>
public partial class GameWindow : Window
{
    public GameWindow(GameWindowViewModel viewModel)
    {
        this.DataContext = viewModel;

        Loaded += OnLoaded;

        InitializeComponent();
    }

    /// <summary>
    /// Обработать событие отображения окна.
    /// </summary>
    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Рассчитываем реальные размеры экрана и пропорционально растягиваем изображение.
        var scale = Math.Min(ActualWidth / GameInfo.OriginalWidth, ActualHeight / GameInfo.OriginalHeight);

        Field.LayoutTransform = new ScaleTransform(scale, scale);

        GameInfo.Width = scale * GameInfo.OriginalWidth;
        GameInfo.Height = scale * GameInfo.OriginalHeight;
        GameInfo.Scale = scale;
    }
}