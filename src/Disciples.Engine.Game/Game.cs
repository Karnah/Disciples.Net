using Disciples.Engine.Base;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Implementation;
using Disciples.Engine.Models;
using Disciples.Scene.Battle;
using Disciples.Scene.LoadingGame;
using DryIoc;

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

        Container.Resolve<ITextProvider>().Load();
        Container.Resolve<IUnitInfoProvider>().Load();

        _gameController.Start();

        // Сразу отображаем сцену загрузки.
        _gameController.ChangeScene<ILoadingGameScene, SceneParameters>(SceneParameters.Empty);

        var gameContext = _gameController.LoadGame();

        // Следующая сцена будет сцена битвы.
        _gameController.ChangeScene<IBattleScene, BattleSceneParameters>(new BattleSceneParameters(
            CreateSquad(gameContext.Players[0], gameContext.Players[0].Squads[0]),
            CreateSquad(gameContext.Players[1], gameContext.Players[1].Squads[0])));
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
            new BattleSceneModule(),
            new LoadingGameSceneModule(),
            platformGameModule
        };

        foreach (var gameModule in modules)
        {
            gameModule.Initialize(container);
        }

        return container;
    }

    /// <summary>
    /// Создать отряд.
    /// </summary>
    private Squad CreateSquad(Player player, PlayerSquad playerSquad)
    {
        var unitInfoProvider = Container.Resolve<IUnitInfoProvider>();
        var units = playerSquad
            .Units
            .Where(u => !u.IsDead)
            .Select(u => new Unit(u.Id.ToString(), unitInfoProvider.GetUnitType(u.UnitTypeId), player, u.SquadLinePosition, u.SquadFlankPosition) { Level = u.Level, Experience = u.Experience, HitPoints = u.HitPoints})
            .ToArray();
        return new Squad(units);
    }
}