using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using DryIoc;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Implementation;
using Disciples.Engine.Models;
using Disciples.Scene.Battle;
using Disciples.Scene.Loading;

namespace Disciples.Avalonia;

public partial class App : Application
{
    private GameController _gameController = null!;

    /// <summary>
    /// IoC контейнер.
    /// </summary>
    public IContainer Container { get; private set; } = null!;

    /// <inheritdoc />
    public override void Initialize()
    {
        Container = CreateContainer();
        _gameController = Container.Resolve<GameController>();

        AvaloniaXamlLoader.Load(this);
    }

    /// <inheritdoc />
    public override void OnFrameworkInitializationCompleted()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var logger = Container.Resolve<ILogger>();

        logger.Log("Create GameWindow");


        var gameWindow = new GameWindow();
        gameWindow.Closing += (sender, eventArgs) => { _gameController.Stop(); };

        ((IClassicDesktopStyleApplicationLifetime)ApplicationLifetime!).MainWindow = gameWindow;
        base.OnFrameworkInitializationCompleted();


        logger.Log("Battle begin");
        logger.Log($"Loading time: {stopwatch.ElapsedMilliseconds / 1000.0} s.");

        stopwatch.Stop();

        Container.Resolve<ITextProvider>().Load();
        Container.Resolve<IUnitInfoProvider>().Load();

        _gameController.Start();

        // Сразу отображаем сцену загрузки.
        _gameController.ChangeScene<ILoadingScene, SceneParameters>(SceneParameters.Empty);

        var gameContext = _gameController.LoadGame();

        // Следующая сцена будет сцена битвы.
        _gameController.ChangeScene<IBattleScene, BattleSceneParameters>(new BattleSceneParameters(
            CreateSquad(gameContext.Players[0], gameContext.Players[0].Squads[0]),
            CreateSquad(gameContext.Players[1], gameContext.Players[1].Squads[0])));
    }

    /// <summary>
    /// Создать IoC контейнер.
    /// </summary>
    private static IContainer CreateContainer()
    {
        var container = new Container();
        var modules = new IGameModule[]
        {
            new CommonModule(),
            new BattleSceneModule(),
            new LoadingSceneModule(),
            new AvaloniaModule()
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