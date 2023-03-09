﻿using System;
using System.Diagnostics;
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

            // Следующая сцена будет сцена битвы.
            _gameController.ChangeScene<IBattleScene, BattleSceneParameters>(new BattleSceneParameters(
                CreateAttackingSquad(Container),
                CreateDefendingSquad(Container)));

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
    /// Создать нападающий отряд.
    /// </summary>
    private static Squad CreateAttackingSquad(IContainer container)
    {
        var unitInfoProvider = container.Resolve<IUnitInfoProvider>();
        var player = new Player(0, false);


        var assassin = unitInfoProvider.GetUnitType("G000UU0154");
        //var mage = unitInfoProvider.GetUnitType("G000UU0010");
        var u02 = new Unit(Guid.NewGuid().ToString(), assassin, player, UnitSquadLinePosition.Back, UnitSquadFlankPosition.Top);

        var wight = unitInfoProvider.GetUnitType("G000UU0178");
        //var pathfinder = unitInfoProvider.GetUnitType("G000UU0020");
        var u01 = new Unit(Guid.NewGuid().ToString(), wight, player, UnitSquadLinePosition.Back, UnitSquadFlankPosition.Center);

        var abbess = unitInfoProvider.GetUnitType("G000UU0017");
        var u00 = new Unit(Guid.NewGuid().ToString(), abbess, player, UnitSquadLinePosition.Back, UnitSquadFlankPosition.Bottom);


        var knight = unitInfoProvider.GetUnitType("G000UU0002");
        var u12 = new Unit(Guid.NewGuid().ToString(), knight, player, UnitSquadLinePosition.Front, UnitSquadFlankPosition.Top);

        var imperialKnight = unitInfoProvider.GetUnitType("G000UU0003");
        var u11 = new Unit(Guid.NewGuid().ToString(), imperialKnight, player, UnitSquadLinePosition.Front, UnitSquadFlankPosition.Center);

        var u10 = new Unit(Guid.NewGuid().ToString(), knight, player, UnitSquadLinePosition.Front, UnitSquadFlankPosition.Bottom);


        return new Squad(new []{ u02, u01, u00, u12, u11, u10});
    }

    /// <summary>
    /// Создать защищающийся отряд.
    /// </summary>
    private static Squad CreateDefendingSquad(IContainer container)
    {
        var unitInfoProvider = container.Resolve<IUnitInfoProvider>();
        var player = new Player(0, true);


        var hillGiant = unitInfoProvider.GetUnitType("G000UU0029");
        var u12 = new Unit(Guid.NewGuid().ToString(), hillGiant, player, UnitSquadLinePosition.Front, UnitSquadFlankPosition.Top);

        var masterOfRunes = unitInfoProvider.GetUnitType("G000UU0165");
        var u11 = new Unit(Guid.NewGuid().ToString(), masterOfRunes, player, UnitSquadLinePosition.Front, UnitSquadFlankPosition.Center);

        var gnomesKing = unitInfoProvider.GetUnitType("G000UU0039");
        var u10 = new Unit(Guid.NewGuid().ToString(), gnomesKing, player, UnitSquadLinePosition.Front, UnitSquadFlankPosition.Bottom);


        var crossbowman = unitInfoProvider.GetUnitType("G000UU0027");
        var u01 = new Unit(Guid.NewGuid().ToString(), crossbowman, player, UnitSquadLinePosition.Back, UnitSquadFlankPosition.Center);

        var gornDefender = unitInfoProvider.GetUnitType("G000UU0162");
        var u02 = new Unit(Guid.NewGuid().ToString(), gornDefender, player, UnitSquadLinePosition.Back, UnitSquadFlankPosition.Bottom);


        return new Squad(new []{ u12, u11, u10, u02, u01});
    }
}