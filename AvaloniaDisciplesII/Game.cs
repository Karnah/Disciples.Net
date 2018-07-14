using System;
using System.Collections.Generic;
using System.Diagnostics;

using Avalonia;
using Avalonia.Threading;

using Animation.Implementation;
using Inftastructure;
using Inftastructure.Components;
using Inftastructure.Enums;
using Inftastructure.Interfaces;

using Action = Inftastructure.Enums.Action;

namespace Animation
{
    public class Game
    {
        private const int TicksPerSecond = 60;

        private readonly IBitmapResources _bitmapResources;
        private readonly IMapVisual _mapVisual;
        private readonly IAudioService _audioService;


        private long _ticks;
        private DispatcherTimer _timer;
        private Stopwatch _stopwatch;

        public Game(IBitmapResources bitmapResources, IMapVisual mapVisual, IAudioService audioService)
        {
            _bitmapResources = bitmapResources;
            _mapVisual = mapVisual;
            _audioService = audioService;

            CreateGameObjects();

            _stopwatch = new Stopwatch();
            _stopwatch.Start();
            _ticks = _stopwatch.ElapsedMilliseconds;

            _timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 1000 / TicksPerSecond) };
            _timer.Tick += UpdateScene;
            _timer.Start();
        }

        private void CreateGameObjects()
        {
            GameObjects = new[] {
                CreateUnit("g000uu0010", 0, 2, Direction.Southeast),
                CreateUnit("g000uu0021", 0, 1, Direction.Southeast),
                CreateUnit("g000uu0017", 0, 0, Direction.Southeast),

                CreateUnit("g000uu0015", 1, 2, Direction.Southeast),
                CreateUnit("g000uu0015", 1, 1, Direction.Southeast),
                CreateUnit("g000uu0015", 1, 0, Direction.Southeast),

                CreateUnit("g000uu0093", 2, 2, Direction.Northwest),
                CreateUnit("g000uu0087", 2, 1, Direction.Northwest),
                CreateUnit("g000uu0087", 2, 0, Direction.Northwest),

                //CreateUnit("g000uu0154", 3, 2, Direction.Northwest),
                CreateUnit("g000uu0079", 3, 1, Direction.Northwest),
                CreateUnit("g000uu0080", 3, 0, Direction.Northwest),
            };

            //GameObjects = new[]
            //{
            //    CreateUnit("Mage", 0, 2, Direction.Southeast, new []{"UNIT10A", "UNIT10B", "UNIT10C"}),
            //    CreateUnit("Archmage", 0, 1, Direction.Southeast, new []{"UNIT10A", "UNIT10B", "UNIT10C"}),
            //    CreateUnit("Matriarch", 0, 0, Direction.Southeast, new []{"UNIT10A", "UNIT10B", "UNIT10C"}),

            //    CreateUnit("Paladin", 1, 2, Direction.Southeast, new []{"ANIM03A", "ANIM03B", "ANIM03C"}),
            //    CreateUnit("Paladin", 1, 1, Direction.Southeast, new []{"ANIM03A", "ANIM03B", "ANIM03C"}),
            //    CreateUnit("Paladin", 1, 0, Direction.Southeast, new []{"ANIM03A", "ANIM03B", "ANIM03C"}),
            //};


            foreach (var gameObject in GameObjects) {
                gameObject.OnInitialize();
            }
        }

        private GameObject CreateUnit(string id, double x, double y, Direction direction)
        {
            var coor = GameInfo.OffsetCoordinates(x, y);
            var go = new GameObject();
            go.Components = new IComponent[] {
                new MapObject(go) {
                    Position = new Rect(coor.X, coor.Y, 100, 100),
                    Direction = direction,
                    Action = Action.Waiting,
                },
                new AnimationComponent(go, _mapVisual, _bitmapResources, id, "S1"),
                new AnimationComponent(go, _mapVisual, _bitmapResources, id, "A1"),
                new AnimationComponent(go, _mapVisual, _bitmapResources, id, "A2"),
                //new SoundsComponent(go, AudioService, attackSounds),
            };

            return go;
        }


        private void UpdateScene(object sender, EventArgs e)
        {
            var ticks = _stopwatch.ElapsedMilliseconds;
            var ticksCount = ticks - _ticks;
            _ticks = ticks;

            foreach (var gameObject in GameObjects) {
                foreach (var gameObjectComponent in gameObject.Components) {
                    gameObjectComponent.OnUpdate(ticksCount);
                }
            }
        }

        public IList<GameObject> GameObjects { get; private set; }
    }
}
