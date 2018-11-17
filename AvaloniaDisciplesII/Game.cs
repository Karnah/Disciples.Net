using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input.Raw;
using Avalonia.Threading;

using Engine;
using Engine.Common.GameObjects;
using Engine.Models;

namespace AvaloniaDisciplesII
{
    /// <inheritdoc />
    public class Game : IGame
    {
        private const int TICKS_PER_SECOND = 60;

        private readonly ILogger _logger;
        private readonly LinkedList<GameObject> _gameObjects;

        private long _ticks;
        private DispatcherTimer _timer;
        private Stopwatch _stopwatch;

        /// <summary>
        /// Объект, который в данный момент выделен курсором.
        /// </summary>
        private GameObject _selectedGameObject;
        /// <summary>
        /// Позиция курсора на игровом поле.
        /// </summary>
        private Point _mousePosition;
        /// <summary>
        /// Смещение поля относительно экрана.
        /// </summary>
        private Point? _screenOffset;
        /// <summary>
        /// Нажата ли ЛКМ.
        /// </summary>
        private bool _isLeftMouseButtonPressed;
        /// <summary>
        /// Была ли отпущена ЛКМ.
        /// </summary>
        private bool _isLeftMouseButtonReleased;

        /// <inheritdoc />
        public Game(ILogger logger)
        {
            _logger = logger;
            _gameObjects = new LinkedList<GameObject>();
        }


        /// <inheritdoc />
        public IReadOnlyCollection<GameObject> GameObjects => _gameObjects;


        /// <inheritdoc />
        public event EventHandler<SceneUpdatingEventArgs> SceneEndUpdating;

        /// <inheritdoc />
        public event EventHandler<SceneUpdatingEventArgs> SceneRedraw;

        /// <inheritdoc />
        public event EventHandler<GameObjectActionEventArgs> GameObjectAction;


        /// <summary>
        /// Запустить внутренний таймер, который обновляет объекты на сцене.
        /// </summary>
        public void Start()
        {
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
            _ticks = _stopwatch.ElapsedMilliseconds;

            _timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 1000 / TICKS_PER_SECOND) };
            _timer.Tick += UpdateScene;
            _timer.Start();

            // Перед началом боя обновляем сцену, чтобы все объекты успели отрисоваться на старте.
            SceneRedraw?.Invoke(this, new SceneUpdatingEventArgs(0));

            // Подписываемся на события перемещения курсора и нажатия кнопок.
            Application.Current.InputManager.PostProcess.OfType<RawMouseEventArgs>().Subscribe(OnMouseStateChanged);
        }

        /// <summary>
        /// Останавливает внутренний таймер, который обновляет объекты на сцене.
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
                ProcessCursorEvents();
                UpdateGameObjects(ticksCount);

                SceneEndUpdating?.Invoke(this, new SceneUpdatingEventArgs(ticksCount));
                SceneRedraw?.Invoke(this, new SceneUpdatingEventArgs(ticksCount));
            }
            catch (Exception e) {
                _logger.Log(e.ToString());
            }
        }

        private void UpdateGameObjects(long ticksCount)
        {
            for (var gameObjectNode = _gameObjects.First; gameObjectNode != null;) {
                var nextNode = gameObjectNode.Next;
                var gameObject = gameObjectNode.Value;

                if (gameObject.IsDestroyed) {
                    _gameObjects.Remove(gameObjectNode);
                }
                else {
                    gameObject.OnUpdate(ticksCount);
                }

                gameObjectNode = nextNode;
            }
        }

        private void ProcessCursorEvents()
        {
            var selectedGameObject = GameObjects
                .OrderBy(go => go.Y)
                .FirstOrDefault(go => go.IsInteractive &&
                                      go.X * GameInfo.Scale <= _mousePosition.X && _mousePosition.X < (go.X + go.Width) * GameInfo.Scale &&
                                      go.Y * GameInfo.Scale <= _mousePosition.Y && _mousePosition.Y < (go.Y + go.Height) * GameInfo.Scale);

            if (selectedGameObject != _selectedGameObject)
            {
                if (_selectedGameObject != null)
                    GameObjectAction?.Invoke(this, new GameObjectActionEventArgs(GameObjectActionType.Unselected, _selectedGameObject));

                if (selectedGameObject != null)
                    GameObjectAction?.Invoke(this, new GameObjectActionEventArgs(GameObjectActionType.Selected, selectedGameObject));

                _selectedGameObject = selectedGameObject;
            }

            if (_selectedGameObject != null)
            {
                if (_isLeftMouseButtonPressed)
                    GameObjectAction?.Invoke(this, new GameObjectActionEventArgs(GameObjectActionType.LeftButtonPressed, _selectedGameObject));

                if (_isLeftMouseButtonReleased)
                    GameObjectAction?.Invoke(this, new GameObjectActionEventArgs(GameObjectActionType.LeftButtonReleased, _selectedGameObject));
            }

            _isLeftMouseButtonPressed = false;
            _isLeftMouseButtonReleased = false;
        }

        private void OnMouseStateChanged(RawMouseEventArgs args)
        {
            switch (args.Type) {
                case RawMouseEventType.Move:
                    // Один раз рассчитываем смещение игрового поля относительно левого края экрана.
                    // todo Возможно стоит запоминать только Grid, если размеры экрана будут меняться.
                    if (_screenOffset == null) {
                        var window = args.Root as GameWindow;
                        var field = window.Find<Grid>("Field");
                        _screenOffset = new Point(field.Bounds.X, field.Bounds.Y);
                    }

                    _mousePosition = args.Position - _screenOffset.Value;
                    break;
                case RawMouseEventType.LeftButtonDown:
                    _isLeftMouseButtonPressed = true;
                    break;
                case RawMouseEventType.LeftButtonUp:
                    _isLeftMouseButtonReleased = true;
                    break;
            }
        }


        /// <inheritdoc />
        public void CreateObject(GameObject gameObject)
        {
            gameObject.OnInitialize();
            _gameObjects.AddLast(gameObject);
        }

        /// <inheritdoc />
        public void DestroyObject(GameObject gameObject)
        {
            gameObject.Destroy();
        }


        /// <inheritdoc />
        public void ClearScene()
        {
            _gameObjects.Clear();
        }
    }
}
