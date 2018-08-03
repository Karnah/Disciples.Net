using System;

using Avalonia;
using Avalonia.Logging.Serilog;
using Unity;
using Unity.Resolution;

using AvaloniaDisciplesII.Battle;
using Engine;
using Engine.Battle.Providers;
using Engine.Implementation.Game;
using Engine.Implementation.Resources;
using Engine.Interfaces;
using Engine.Models;

namespace AvaloniaDisciplesII
{
    class Program
    {
        private static UnityContainer Container;

        static void Main(string[] args)
        {
            BuildAvaloniaApp().Start<GameWindow>(() => {
                CreateContainer();

                var battleAttackController = Container.Resolve<BattleAttackController>(
                    new ParameterOverride("attackSquad", CreateAttackingSquad()),
                    new ParameterOverride("defendSquad", CreateDefendingSquad()));

                return new GameWindowViewModel(
                    Container.Resolve<BattleViewModel>(new ParameterOverride("battleAttackController", battleAttackController)));
            });
        }


        private static void CreateContainer()
        {
            Container = new UnityContainer();

            var battleResourceProvider = new BattleResourceProvider();
            Container.RegisterInstance<IBattleResourceProvider>(battleResourceProvider);

            var unitInfoProvider = new UnitInfoProvider();
            Container.RegisterInstance<IUnitInfoProvider>(unitInfoProvider);

            var battleUnitResourceProvider = new BattleUnitResourceProvider(unitInfoProvider, battleResourceProvider);
            Container.RegisterInstance<IBattleUnitResourceProvider>(battleUnitResourceProvider);

            var mapVisual = new MapVisual();
            Container.RegisterInstance<IMapVisual>(mapVisual);

            var audioService = new AudioPlaybackEngine();
            Container.RegisterInstance<IAudioService>(audioService);

            var game = Container.Resolve<Game>();
            Container.RegisterInstance<IGame>(game);
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

            var masterOfRunes = unitInfoProvider.GetUnitType("g000uu0165");
            var u11 = new Unit(Guid.NewGuid().ToString(), masterOfRunes, player, 1, 1);

            var gnomesKing = unitInfoProvider.GetUnitType("g000uu0039");
            var u10 = new Unit(Guid.NewGuid().ToString(), gnomesKing, player, 1, 0);


            var gornDefender = unitInfoProvider.GetUnitType("g000uu0162");
            var u02 = new Unit(Guid.NewGuid().ToString(), gornDefender, player, 0, 2);

            var crossbowman = unitInfoProvider.GetUnitType("g000uu0027");
            var u01 = new Unit(Guid.NewGuid().ToString(), crossbowman, player, 0, 1);

            return new Squad(new []{ u11, u10, u02, u01});
        }


        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .UseReactiveUI()
                .UseDirect2D1()
                //.UseSkia()
                .LogToDebug();
    }
}
