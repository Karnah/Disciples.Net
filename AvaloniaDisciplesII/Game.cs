using System;
using System.Collections.Generic;
using System.Diagnostics;

using Avalonia.Threading;

using Engine;
using Engine.Common.GameObjects;

namespace AvaloniaDisciplesII
{
    public class Game : IGame
    {
        private const int TICKS_PER_SECOND = 60;

        private readonly ILogger _logger;
        private readonly LinkedList<GameObject> _gameObjects;

        private long _ticks;
        private DispatcherTimer _timer;
        private Stopwatch _stopwatch;

        public Game(ILogger logger)
        {
            _logger = logger;
            _gameObjects = new LinkedList<GameObject>();
        }


        public IReadOnlyCollection<GameObject> GameObjects => _gameObjects;


        public event EventHandler SceneEndUpdating;

        public event EventHandler SceneRedraw;


        /// <summary>
        /// Запускает внутренний таймер, который обновляет объекты на сцене
        /// </summary>
        public void Start()
        {
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
            _ticks = _stopwatch.ElapsedMilliseconds;

            _timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 1000 / TICKS_PER_SECOND) };
            _timer.Tick += UpdateScene;
            _timer.Start();

            // Перед началом боя обновляем сцену, чтобы все объекты успели отрисоваться на старте
            SceneRedraw?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Останавливает внутренний таймер, который обновляет объекты на сцене
        /// </summary>
        public void Stop()
        {
            _stopwatch.Stop();
            _stopwatch = null;

            _timer.Stop();
            _timer = null;
        }

        private void UpdateScene(object sender, EventArgs args)
        {
            var ticks = _stopwatch.ElapsedMilliseconds;
            var ticksCount = ticks - _ticks;
            _ticks = ticks;

            try {
                for (var gameObjectNode = _gameObjects.First; gameObjectNode != null;) {
                    var nextNode = gameObjectNode.Next;

                    if (gameObjectNode.Value.IsDestroyed) {
                        _gameObjects.Remove(gameObjectNode);
                    }
                    else {
                        gameObjectNode.Value.OnUpdate(ticksCount);
                    }

                    gameObjectNode = nextNode;
                }

                SceneEndUpdating?.Invoke(this, EventArgs.Empty);
                SceneRedraw?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception e) {
                _logger.Log(e.ToString());
            }
        }

        public void CreateObject(GameObject gameObject)
        {
            gameObject.OnInitialize();
            _gameObjects.AddLast(gameObject);
        }

        public void DestroyObject(GameObject gameObject)
        {
            gameObject.Destroy();
        }


        public void ClearScene()
        {
            _gameObjects.Clear();
        }
    }
}
