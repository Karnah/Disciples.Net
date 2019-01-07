using System;
using System.Diagnostics;
using System.Windows;

using Unity;
using Unity.Resolution;

using Disciples.Engine;
using Disciples.Engine.Battle.Providers;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Implementation;
using Disciples.Engine.Implementation.Controllers;
using Disciples.Engine.Implementation.Resources;
using Disciples.Engine.Platform.Factories;
using Disciples.Engine.Platform.Managers;
using Disciples.WPF.Controllers;
using Disciples.WPF.Factories;
using Disciples.WPF.Managers;

namespace Disciples.WPF
{
    public partial class App : Application
    {
        private static UnityContainer Container;

        private static Game _game;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();

                CreateContainer();

                var logger = Container.Resolve<ILogger>();

                logger.Log("Create BattleAttackController");
                var battleAttackController = Container.Resolve<BattleAttackController>(
                    new ParameterOverride("attackSquad", CreateAttackingSquad()),
                    new ParameterOverride("defendSquad", CreateDefendingSquad()));
                logger.Log($"End create BattleAttackController{Environment.NewLine}");

                var battleInterfaceController = Container.Resolve<BattleInterfaceController>(
                    new ParameterOverride("battleAttackController", battleAttackController));
                battleInterfaceController.Initialize();

                logger.Log("Create GameWindow");
                var gw = Container.Resolve<GameWindow>();
                gw.Closing += (sender, eventArgs) => { _game.Stop(); };

                logger.Log("Battle begin");
                logger.Log($"Loading time: {stopwatch.ElapsedMilliseconds / 1000.0} s.");

                stopwatch.Stop();

                _game.Start();

                gw.ShowDialog();
            }
            catch (Exception exception) {
                MessageBox.Show(exception.Message);
            }
        }


        private static void CreateContainer()
        {
            Container = new UnityContainer();

            var logger = new Logger();
            Container.RegisterInstance<ILogger>(logger);

            // Регистрируем устройства ввода.
            Container.RegisterSingleton<IInputManager, WpfInputManager>();

            // Регистрируем таймер.
            Container.RegisterSingleton<IGameTimer, WpfGameTimer>();

            // Регистрируем фабрики.
            Container.RegisterType<IBitmapFactory, WpfBitmapFactory>();
            Container.RegisterType<ISceneObjectFactory, WpfSceneObjectFactory>();

            logger.Log("Start load TextProvider");
            var textProvider = Container.Resolve<TextProvider>();
            Container.RegisterInstance<ITextProvider>(textProvider);
            logger.Log($"End load TextProvider{Environment.NewLine}");

            logger.Log("Start load BattleResource");
            var battleResourceProvider = Container.Resolve<BattleResourceProvider>();
            Container.RegisterInstance<IBattleResourceProvider>(battleResourceProvider);
            logger.Log($"End load BattleResource{Environment.NewLine}");

            logger.Log("Start load UnitInfo");
            var unitInfoProvider = Container.Resolve<UnitInfoProvider>();
            Container.RegisterInstance<IUnitInfoProvider>(unitInfoProvider);
            logger.Log($"End load UnitInfo{Environment.NewLine}");

            logger.Log("Start load BattleUnitResource");
            var battleUnitResourceProvider = Container.Resolve<BattleUnitResourceProvider>();
            Container.RegisterInstance<IBattleUnitResourceProvider>(battleUnitResourceProvider);
            logger.Log($"End load BattleUnitResource{Environment.NewLine}");

            logger.Log("Start load BattleInterfaceProvider");
            var battleInterfaceProvider = Container.Resolve<BattleInterfaceProvider>();
            Container.RegisterInstance<IBattleInterfaceProvider>(battleInterfaceProvider);
            logger.Log($"End load BattleInterfaceProvider{Environment.NewLine}");

            _game = Container.Resolve<Game>();
            Container.RegisterInstance<IGame>(_game);

            Container.RegisterSingleton<IScene, Scene>();
            Container.RegisterSingleton<IVisualSceneController, VisualSceneController>();
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