using System;
using System.Windows;
using System.Windows.Media;
using Disciples.Engine;
using Disciples.WPF.Models;

namespace Disciples.WPF;

/// <summary>
/// Окно игры.
/// </summary>
public partial class GameWindow : Window
{
    private readonly WpfGameInfo _gameInfo;

    private bool _isClosing;

    /// <summary>
    /// Создать объект типа <see cref="GameWindow" />.
    /// </summary>
    public GameWindow(GameWindowViewModel viewModel, WpfGameInfo gameInfo)
    {
        _gameInfo = gameInfo;

        DataContext = viewModel;

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

        Field.RenderTransformOrigin = new Point(0.5, 0.5);
        Field.RenderTransform = new ScaleTransform(scale, scale);
        _gameInfo.GameFiled = Field;

        GameInfo.Width = scale * GameInfo.OriginalWidth;
        GameInfo.Height = scale * GameInfo.OriginalHeight;
        GameInfo.Scale = scale;

        // Инициализируем в первый раз.
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (_gameInfo.OverlapWindow == null)
        {
            var overlapWindow = new OverlapWindow();
            overlapWindow.Closed += (_, _) =>
            {
                if (_isClosing)
                    return;

                _isClosing = true;
                Close();
            };
            overlapWindow.Owner = this;
            overlapWindow.Show();

            _gameInfo.OverlapWindow = overlapWindow;
        }
    }
}