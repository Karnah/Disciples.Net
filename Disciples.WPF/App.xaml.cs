using System;
using System.Diagnostics;
using System.Windows;
using Disciples.Engine.Base;
using Disciples.Engine.Battle;
using Disciples.Engine.Battle.Controllers;
using Disciples.Engine.Battle.Models;
using Disciples.Engine.Battle.Providers;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Implementation;
using Disciples.Engine.Implementation.Battle;
using Disciples.Engine.Implementation.Battle.Controllers;
using Disciples.Engine.Implementation.Battle.Providers;
using Disciples.Engine.Implementation.Common.Providers;
using Disciples.Engine.Implementation.Loading;
using Disciples.Engine.Loading;
using Disciples.Engine.Models;
using Disciples.Engine.Platform.Factories;
using Disciples.Engine.Platform.Managers;
using Disciples.WPF.Factories;
using Disciples.WPF.Managers;
using DryIoc;

namespace Disciples.WPF
{
    public partial class App : Application
    {
        private static IContainer Container;

        private static GameController _gameController;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                Container = CreateContainer();
                _gameController = Container.Resolve<GameController>();

                var logger = Container.Resolve<ILogger>();

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
                var loadingSceneController = Container.Resolve<ILoadingSceneController>();
                _gameController.ChangeScene(loadingSceneController, SceneParameters.Empty);

                // Следующая сцена будет сцена битвы.
                var battleSceneController = Container.Resolve<IBattleSceneController>();
                _gameController.ChangeScene(battleSceneController, new BattleSceneParameters(
                    Container.Resolve<IBattleController>(),
                    Container.Resolve<IBattleInterfaceController>(),
                    CreateAttackingSquad(),
                    CreateDefendingSquad()));

                gw.ShowDialog();
            }
            catch (Exception exception) {
                MessageBox.Show(exception.Message);
                Shutdown();
            }
        }

        private static IContainer CreateContainer()
        {
            var container = new Container();

            var logger = new Logger();
            container.RegisterInstance<ILogger>(logger);

            // Регистрируем устройства ввода.
            container.Register<IInputManager, WpfInputManager>(Reuse.Singleton);

            // Регистрируем таймер.
            container.Register<IGameTimer, WpfGameTimer>(Reuse.Singleton);

            // Регистрируем фабрики.
            container.Register<IBitmapFactory, WpfBitmapFactory>();
            container.Register<ISceneFactory, WpfSceneFactory>();

            container.RegisterMany<GameController>(Reuse.Singleton);

            container.Register<ITextProvider, TextProvider>(Reuse.Singleton);
            container.Register<IInterfaceProvider, InterfaceProvider>(Reuse.Singleton);
            container.Register<IUnitInfoProvider, UnitInfoProvider>(Reuse.Singleton);

            container.Register<ILoadingSceneController, LoadingSceneController>(Reuse.Singleton);

            container.Register<IBattleResourceProvider, BattleResourceProvider>(Reuse.Singleton);
            container.Register<IBattleInterfaceProvider, BattleInterfaceProvider>(Reuse.Singleton);
            container.Register<IBattleUnitResourceProvider, BattleUnitResourceProvider>(Reuse.Singleton);
            container.Register<IBattleController, BattleController>(Reuse.Singleton);
            container.Register<IBattleInterfaceController, BattleInterfaceController>(Reuse.Singleton);
            container.Register<IBattleSceneController, BattleSceneController>(Reuse.Singleton);

            // Регистрируем View и ViewModel.
            container.Register<GameWindow>();
            container.Register<GameWindowViewModel>();

            return container;
        }


        public static Squad CreateAttackingSquad()
        {
            var unitInfoProvider = Container.Resolve<IUnitInfoProvider>();
            var player = new Player(0, false);


            var mage = unitInfoProvider.GetUnitType("g000uu0010");
            var u02 = new Unit(Guid.NewGuid().ToString(), mage, player, 0, 2);

            var wight = unitInfoProvider.GetUnitType("g000uu0178");
            //var pathfinder = unitInfoProvider.GetUnitType("g000uu0020");
            var u01 = new Unit(Guid.NewGuid().ToString(), wight, player, 0, 1);

            var abbess = unitInfoProvider.GetUnitType("g000uu0017");
            var u00 = new Unit(Guid.NewGuid().ToString(), abbess, player, 0, 0);


            var knight = unitInfoProvider.GetUnitType("g000uu0002");
            var u12 = new Unit(Guid.NewGuid().ToString(), knight, player, 1, 2);

            var imperialKnight = unitInfoProvider.GetUnitType("g000uu0003");
            var u11 = new Unit(Guid.NewGuid().ToString(), imperialKnight, player, 1, 1);

            var u10 = new Unit(Guid.NewGuid().ToString(), knight, player, 1, 0);


            return new Squad(new[] { u02, u01, u00, u12, u11, u10 });
        }

        public static Squad CreateDefendingSquad()
        {
            var unitInfoProvider = Container.Resolve<IUnitInfoProvider>();
            var player = new Player(0, false);


            var hillGiant = unitInfoProvider.GetUnitType("g000uu0029");
            var u12 = new Unit(Guid.NewGuid().ToString(), hillGiant, player, 1, 2);

            var masterOfRunes = unitInfoProvider.GetUnitType("g000uu0165");
            var u11 = new Unit(Guid.NewGuid().ToString(), masterOfRunes, player, 1, 1);

            var gnomesKing = unitInfoProvider.GetUnitType("g000uu0039");
            var u10 = new Unit(Guid.NewGuid().ToString(), gnomesKing, player, 1, 0);


            var crossbowman = unitInfoProvider.GetUnitType("g000uu0027");
            var u01 = new Unit(Guid.NewGuid().ToString(), crossbowman, player, 0, 1);

            var gornDefender = unitInfoProvider.GetUnitType("g000uu0162");
            var u02 = new Unit(Guid.NewGuid().ToString(), gornDefender, player, 0, 0);


            return new Squad(new[] { u12, u11, u10, u02, u01 });
        }
    }
}