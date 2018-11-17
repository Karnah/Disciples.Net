using System;

using Avalonia;
using Avalonia.Logging.Serilog;
using Unity;
using Unity.Resolution;

using AvaloniaDisciplesII.Battle;
using Engine;
using Engine.Battle.Providers;
using Engine.Common.Controllers;
using Engine.Common.Models;
using Engine.Common.Providers;
using Engine.Implementation;
using Engine.Implementation.Controllers;
using Engine.Implementation.Resources;

namespace AvaloniaDisciplesII
{
    class Program
    {
        private static UnityContainer Container;

        private static Game _game;

        static void Main(string[] args)
        {
            try {
                var app = BuildAvaloniaApp().SetupWithoutStarting();

                CreateContainer();

                var logger = Container.Resolve<ILogger>();

                logger.Log("Create BattleAttackController");
                var battleAttackController = Container.Resolve<BattleAttackController>(
                    new ParameterOverride("attackSquad", CreateAttackingSquad()),
                    new ParameterOverride("defendSquad", CreateDefendingSquad()));
                logger.Log($"End create BattleAttackController{Environment.NewLine}");


                var battleInterfaceController = Container.Resolve<BattleInterfaceController>(
                    new ParameterOverride("battleAttackController", battleAttackController));

                logger.Log("Create BattleViewModel");
                var battleViewModel = Container.Resolve<BattleViewModel>(
                    new ParameterOverride("battleAttackController", battleAttackController),
                    new ParameterOverride("battleInterfaceController", battleInterfaceController));
                logger.Log($"End create BattleViewModel{Environment.NewLine}");


                logger.Log("Create GameWindow");
                var gw = new GameWindow(new GameWindowViewModel(battleViewModel));
                gw.Closing += (sender, eventArgs) => { _game.Stop(); };
                gw.ShowDialog();
                logger.Log("Battle begin");

                _game.Start();
                app.Instance.Run(gw);
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
        }


        private static void CreateContainer()
        {
            Container = new UnityContainer();

            var logger = new Logger();
            Container.RegisterInstance<ILogger>(logger);


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


            var audioPlaybackEngine = new AudioPlaybackEngine();
            Container.RegisterInstance<IAudioController>(audioPlaybackEngine);

            _game = Container.Resolve<Game>();
            Container.RegisterInstance<IGame>(_game);

            Container.RegisterSingleton<IMapVisual, MapVisual>();
            Container.RegisterSingleton<IVisualSceneController, VisualSceneController>();
        }


        public static Squad CreateAttackingSquad()
        {
            var unitInfoProvider = Container.Resolve<IUnitInfoProvider>();
            var player = new Player(0, false);


            var mage = unitInfoProvider.GetUnitType("g000uu0010");
            var u02 = new Unit(Guid.NewGuid().ToString(), mage, player, 0, 2);

            var pathfinder = unitInfoProvider.GetUnitType("g000uu0020");
            var u01 = new Unit(Guid.NewGuid().ToString(), pathfinder, player, 0, 1);

            var abbess = unitInfoProvider.GetUnitType("g000uu0017");
            var u00 = new Unit(Guid.NewGuid().ToString(), abbess, player, 0, 0);


            var knight = unitInfoProvider.GetUnitType("g000uu0002");
            var u12 = new Unit(Guid.NewGuid().ToString(), knight, player, 1, 2);

            var imperiaKnight = unitInfoProvider.GetUnitType("g000uu0003");
            var u11 = new Unit(Guid.NewGuid().ToString(), imperiaKnight, player, 1, 1);

            var u10 = new Unit(Guid.NewGuid().ToString(), knight, player, 1, 0);


            return new Squad(new []{ u02, u01, u00, u12, u11, u10});
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


            return new Squad(new []{ u12, u11, u10, u02, u01});
        }


        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .UseReactiveUI()
                //.UseDirect2D1()
                .UseSkia()
                //.UseWin32()
                .LogToDebug();
    }
}
