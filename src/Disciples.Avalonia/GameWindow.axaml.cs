using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Disciples.Engine;
using DryIoc;

namespace Disciples.Avalonia;

/// <summary>
/// ������� ���� ����.
/// </summary>
public partial class GameWindow : Window
{
    /// <summary>
    /// ������� ������ ���� <see cref="GameWindow" />.
    /// </summary>
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
    /// ������ �������� ����.
    /// </summary>
    public Grid GameField { get; private set; } = null!;

    /// <summary>
    /// ���������� ������� ����������� ����.
    /// </summary>
    private void OnActivated(object? sender, EventArgs e)
    {
        var screen = Screens.ScreenFromVisual(this);

        // ���� ���� ������� � ��� �� �������, screen ����� null.
        if (screen == null)
            return;

        // ������������ �������� ������ ������ � ��������������� ����������� �����������.
        var scale = Math.Min(screen.WorkingArea.Width / GameInfo.OriginalWidth, screen.WorkingArea.Height / GameInfo.OriginalHeight);

        GameField = this.Find<Grid>("Field")!;
        GameField.RenderTransform = new ScaleTransform(scale, scale);

        GameInfo.Width = scale * GameInfo.OriginalWidth;
        GameInfo.Height = scale * GameInfo.OriginalHeight;
        GameInfo.Scale = scale;
    }
}