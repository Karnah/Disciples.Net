using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Disciples.Avalonia.Models;
using Disciples.Engine;
using DryIoc;

namespace Disciples.Avalonia;

/// <summary>
/// Главное окно игры.
/// </summary>
public partial class GameWindow : Window
{
    private readonly AvaloniaGameInfo _gameInfo;

    private bool _isClosing;

    /// <summary>
    /// Создать объект типа <see cref="GameWindow" />.
    /// </summary>
    public GameWindow()
    {
        var container = ((App)Application.Current!).Container;
        _gameInfo = container.Resolve<AvaloniaGameInfo>();

        DataContext = container.Resolve<GameWindowViewModel>();

        Activated += OnActivated;

        AvaloniaXamlLoader.Load(this);

#if DEBUG
        this.AttachDevTools();
#endif
    }

    /// <inheritdoc />
    protected override void OnLoaded()
    {
        base.OnLoaded();

        // FieldTransform задаём именно в OnLoaded, в OnActivated будут некорректные координаты.
        // Но OnActivated срабатывает раньше.
        var gameField = this.Find<Grid>("Field")!;
        _gameInfo.FieldTransform = this.TransformToVisual(gameField) ?? default;
    }

    /// <summary>
    /// Обработать событие отображения окна.
    /// </summary>
    private void OnActivated(object? sender, EventArgs e)
    {
        var screen = Screens.ScreenFromVisual(this);

        // Если окно открыть и тут же закрыть, screen будет null.
        if (screen == null)
            return;

        // Рассчитываем реальные размер экрана и пропорционально растягиваем изображение.
        var scale = Math.Min(screen.WorkingArea.Width / GameInfo.OriginalWidth, screen.WorkingArea.Height / GameInfo.OriginalHeight);

        var gameField = this.Find<Grid>("Field")!;
        gameField.RenderTransform = new ScaleTransform(scale, scale);

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
            overlapWindow.Show(this);

            _gameInfo.OverlapWindow = overlapWindow;
        }
    }
}