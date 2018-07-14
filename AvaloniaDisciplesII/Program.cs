using Avalonia;
using Avalonia.Logging.Serilog;
using Unity;
using Unity.Resolution;

using Animation;
using Animation.Implementation;
using AvaloniaDisciplesII.Battle;
using Inftastructure.Interfaces;


namespace AvaloniaDisciplesII
{
    class Program
    {
        private static UnityContainer Container;

        static void Main(string[] args)
        {
            BuildAvaloniaApp().Start<GameWindow>(() => {
                CreateContainer();

                var game = Container.Resolve<Game>();

                return new GameWindowViewModel(
                    Container.Resolve<BattleViewModel>(new ParameterOverride("gameObjects", game.GameObjects)));
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
