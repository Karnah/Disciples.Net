using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Disciples.Avalonia.Models;
using Disciples.Engine;
using Disciples.Engine.Game;
using DryIoc;
using Microsoft.Extensions.Logging;

namespace Disciples.Avalonia;

/// <summary>
/// ������� ���� ����.
/// </summary>
public partial class GameWindow : Window
{
    private readonly Game _game;
    private readonly AvaloniaGameInfo _gameInfo;
    private readonly ILogger<GameWindow> _logger;

    private bool _isClosing;

    /// <summary>
    /// ������� ������ ���� <see cref="GameWindow" />.
    /// </summary>
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public GameWindow()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
    }

    /// <summary>
    /// ������� ������ ���� <see cref="GameWindow" />.
    /// </summary>
    public GameWindow(Game game)
    {
        _game = game;
        _gameInfo = _game.Container.Resolve<AvaloniaGameInfo>();
        _logger = _game.Container.Resolve<ILogger<GameWindow>>();

        DataContext = _game.Container.Resolve<GameWindowViewModel>();

        Activated += OnActivated;

        InitializeComponent();

#if DEBUG
        this.AttachDevTools();
#endif
    }

    /// <summary>
    /// ���������� ������� ����������� ����.
    /// </summary>
    private void OnActivated(object? sender, EventArgs e)
    {
        var screen = Screens.ScreenFromVisual(this);

        // ���� ���� ������� � ��� �� �������, screen ����� null.
        if (screen == null)
            return;

        var bounds = screen.Bounds;
        _logger.LogDebug($"GameWindow. Windows size: {bounds}, scaling: {screen.Scaling}");

        // BUG: ����� � ���� � overlapWindow ������� ��� Linux:
        // ������-�� �� ��������������� �� ���� �����, ���� ����� �������.
        if (_gameInfo.IsLinux)
        {
            Width = bounds.Width;
            Height = bounds.Height;
        }

        // ������������ �������� ������ ������ � ��������������� ����������� �����������.
        var scale = Math.Min(Width / GameInfo.OriginalWidth, Height / GameInfo.OriginalHeight);
        scale /= screen.Scaling;

        var gameField = this.Find<Grid>("Field")!;
        gameField.RenderTransform = new ScaleTransform(scale, scale);
        _gameInfo.FieldTransform = this.TransformToVisual(gameField) ?? default;

        GameInfo.Width = scale * GameInfo.OriginalWidth;
        GameInfo.Height = scale * GameInfo.OriginalHeight;
        GameInfo.Scale = scale;

        // �������������� � ������ ���.
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (_gameInfo.OverlapWindow == null)
        {
            var overlapWindow = new OverlapWindow();

            if (_gameInfo.IsLinux)
            {
                overlapWindow.Activated += (_, _) =>
                {
                    overlapWindow.Width = Width;
                    overlapWindow.Height = Height;
                };
            }

            overlapWindow.Closed += (_, _) =>
            {
                if (_isClosing)
                    return;

                _isClosing = true;
                Close();
            };
            overlapWindow.Show(this);

            _gameInfo.OverlapWindow = overlapWindow;
            _game.Start();
        }
    }

    /// <inheritdoc />
    protected override void ArrangeCore(Rect finalRect)
    {
        base.ArrangeCore(finalRect);

        // ��� Linux ��-�� ������������� X11 ������������� ����� ��� �����.
        if (_gameInfo.IsLinux)
        {
            var gameField = this.Find<Grid>("Field")!;
            _gameInfo.FieldTransform = this.TransformToVisual(gameField) ?? default;
        }
    }

    /// <inheritdoc />
    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);

        _game.Stop();
    }
}