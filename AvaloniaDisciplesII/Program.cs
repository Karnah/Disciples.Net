using System;

using Avalonia;
using Avalonia.Logging.Serilog;
using Unity;
using Unity.Resolution;

using AvaloniaDisciplesII.Battle;
using AvaloniaDisciplesII.Implementation;
using Engine;
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

                return new GameWindowViewModel(
                    Container.Resolve<BattleViewModel>(
                        new ParameterOverride("attackSquad", CreateAttackingSquad()),
                        new ParameterOverride("defendSquad", CreateDefendingSquad())));
            });
        }


        private static void CreateContainer()
        {
            Container = new UnityContainer();

            var bitmapResources = new BitmapResources();
            Container.RegisterInstance<IBitmapResources>(bitmapResources);

            var mapVisual = new MapVisual();
            Container.RegisterInstance<IMapVisual>(mapVisual);

            var audioService = new AudioPlaybackEngine();
            Container.RegisterInstance<IAudioService>(audioService);

            var game = Container.Resolve<Game>();
            Container.RegisterInstance<IGame>(game);
        }

        public static Squad CreateAttackingSquad()
        {
            var mage = new UnitType("g000uu0010", "Маг", null);
            var u02 = new Unit(Guid.NewGuid().ToString(), mage, 0, 2);

            var pathfinder = new UnitType("g000uu0020", "Следопыт", null);
            var u01 = new Unit(Guid.NewGuid().ToString(), pathfinder, 0, 1);

            var abbess = new UnitType("g000uu0017", "Аббатиса", null);
            var u00 = new Unit(Guid.NewGuid().ToString(), abbess, 0, 0);


            var knight = new UnitType("g000uu0002", "Рыцарь", null);
            var u12 = new Unit(Guid.NewGuid().ToString(), knight, 1, 2);

            var imperiaKnight = new UnitType("g000uu0003", "Имперский рыцарь", null);
            var u11 = new Unit(Guid.NewGuid().ToString(), imperiaKnight, 1, 1);

            var u10 = new Unit(Guid.NewGuid().ToString(), knight, 1, 0);


            return new Squad(new []{ u02, u01, u00, u12, u11, u10});
        }

        public static Squad CreateDefendingSquad()
        {
            var masterOfRunes = new UnitType("g000uu0165", "Мастер рун", null);
            var u11 = new Unit(Guid.NewGuid().ToString(), masterOfRunes, 1, 1);

            var gnomesKing = new UnitType("g000uu0039", "Король гномов", null);
            var u10 = new Unit(Guid.NewGuid().ToString(), gnomesKing, 1, 0);


            var gornDefender = new UnitType("g000uu0162", "Защитник горна", null);
            var u02 = new Unit(Guid.NewGuid().ToString(), gornDefender, 0, 2);

            var crossbowman = new UnitType("g000uu0027", "Арбалетчик", null);
            var u01 = new Unit(Guid.NewGuid().ToString(), crossbowman, 0, 1);

            return new Squad(new []{ u11, u10, u02, u01});
        }


        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .UseReactiveUI()
                //.UseDirect2D1()
                .UseSkia()
                .LogToDebug();
    }
}
