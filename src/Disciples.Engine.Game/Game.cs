using DryIoc;
using Disciples.Engine.Base;
using Disciples.Engine.Implementation;
using Disciples.Engine.Models;
using Disciples.Engine.Scenes;
using Disciples.Scene.Battle;
using Disciples.Scene.LoadingGame;
using Disciples.Scene.LoadingSave;
using Disciples.Scene.LoadSaga;
using Disciples.Scene.MainMenu;
using Disciples.Scene.SinglePlayerGameMenu;

namespace Disciples.Engine.Game;

/// <summary>
/// Класс игры.
/// </summary>
public class Game
{
    private GameController _gameController = null!;

    /// <summary>
    /// Создать объект типа <see cref="Game" />.
    /// </summary>
    public Game(IGameModule platformGameModule)
    {
        Container = CreateContainer(platformGameModule);
        Logger = Container.Resolve<ILogger>();
    }

    /// <summary>
    /// Логгер приложения.
    /// </summary>
    public ILogger Logger { get; }

    /// <summary>
    /// IoC контейнер.
    /// </summary>
    public IContainer Container { get; }

    /// <summary>
    /// Запустить игру.
    /// </summary>
    public void Start()
    {
        _gameController = Container.Resolve<GameController>();
        _gameController.Start();
        _gameController.ChangeScene<ILoadingGameScene, SceneParameters>(SceneParameters.Empty);
    }

    /// <summary>
    /// Остановить игру.
    /// </summary>
    public void Stop()
    {
        _gameController.Stop();
    }

    /// <summary>
    /// Создать IoC контейнер.
    /// </summary>
    private static IContainer CreateContainer(IGameModule platformGameModule)
    {
        var container = new Container();
        var modules = new[]
        {
            new CommonModule(),
            new LoadingGameSceneModule(),
            new MainMenuSceneModule(),
            new SinglePlayerGameMenuSceneModule(),
            new LoadSagaSceneModule(),
            new LoadingSaveSceneModule(),
            new BattleSceneModule(),
            platformGameModule
        };

        foreach (var gameModule in modules)
        {
            gameModule.Initialize(container);
        }

        return container;
    }
}