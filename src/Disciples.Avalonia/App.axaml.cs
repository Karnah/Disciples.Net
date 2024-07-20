using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Disciples.Engine.Game;

namespace Disciples.Avalonia;

/// <summary>
/// Disciples на Avalonia.
/// </summary>
public partial class App : Application
{
    private Game _game = null!;

    /// <inheritdoc />
    public override void Initialize()
    {
        _game = new Game(new AvaloniaModule());

        AvaloniaXamlLoader.Load(this);
    }

    /// <inheritdoc />
    public override void OnFrameworkInitializationCompleted()
    {
        var gameWindow = new GameWindow(_game);
        ((IClassicDesktopStyleApplicationLifetime)ApplicationLifetime!).MainWindow = gameWindow;
        base.OnFrameworkInitializationCompleted();
    }
}