using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using DryIoc;
using Disciples.Engine.Game;

namespace Disciples.Avalonia;

/// <summary>
/// Disciples �� Avalonia.
/// </summary>
public partial class App : Application
{
    private Game? _game;

    /// <summary>
    /// IoC ���������.
    /// </summary>
    public IContainer Container => _game!.Container;

    /// <inheritdoc />
    public override void Initialize()
    {
        _game = new Game(new AvaloniaModule());

        AvaloniaXamlLoader.Load(this);
    }

    /// <inheritdoc />
    public override void OnFrameworkInitializationCompleted()
    {
        var gameWindow = new GameWindow();
        gameWindow.Closing += (sender, eventArgs) => { _game!.Stop(); };

        ((IClassicDesktopStyleApplicationLifetime)ApplicationLifetime!).MainWindow = gameWindow;
        base.OnFrameworkInitializationCompleted();

        _game!.Start();
    }
}