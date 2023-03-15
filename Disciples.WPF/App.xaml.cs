using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Implementation;
using Disciples.Engine.Models;
using Disciples.Scene.Battle;
using Disciples.Scene.Loading;
using DryIoc;

namespace Disciples.WPF;

public partial class App : Application
{
    private static IContainer Container;

    private static GameController _gameController;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        ILogger logger = null;

        try
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Container = CreateContainer();
            _gameController = Container.Resolve<GameController>();

            logger = Container.Resolve<ILogger>();

            logger.Log("Create GameWindow");
            var gw = Container.Resolve<GameWindow>();
            gw.Closing += (sender, eventArgs) => { _gameController.Stop(); };

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

            gw.ShowDialog();
        }
        catch (Exception exception)
        {
            logger?.LogError("Ошибка при запуске приложения", exception);
            MessageBox.Show(exception.Message);
            Shutdown();
        }
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
            new WpfModule()
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