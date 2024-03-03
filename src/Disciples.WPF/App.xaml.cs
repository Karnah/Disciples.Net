using System;
using System.Windows;
using DryIoc;
using Microsoft.Extensions.Logging;
using Disciples.Engine.Game;

namespace Disciples.WPF;

public partial class App : Application
{
    private Game _game = null!;

    /// <inheritdoc />
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        ILogger? logger = null;

        try
        {
            _game = new Game(new WpfModule());
            logger = _game.Logger;

            var gameWindow = _game.Container.Resolve<GameWindow>();
            gameWindow.Closing += (sender, eventArgs) => { _game.Stop(); };

            _game.Start();

            gameWindow.ShowDialog();
        }
        catch (Exception exception)
        {
            logger?.LogError(exception, "Cannot start game");
            MessageBox.Show(exception.Message);
            Shutdown();
        }
    }
}