using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Models;
using Disciples.Engine.Platform.Enums;
using Disciples.Engine.Platform.Events;
using Disciples.Engine.Platform.Managers;

namespace Disciples.Engine.Implementation
{
    /// <inheritdoc />
    public class Game : IGame
    {
        private readonly IGameTimer _gameTimer;
        private readonly ILogger _logger;
        private readonly IInputManager _inputManager;
        private readonly LinkedList<GameObject> _gameObjects;

        private long _ticks;
        private Stopwatch _stopwatch;

        /// <summary>
        /// Объект, который в данный момент выделен курсором.
        /// </summary>
        private GameObject _selectedGameObject;
        /// <summary>
        /// Нажата ли ЛКМ.
        /// </summary>
        private bool _isLeftMouseButtonPressed;
        /// <summary>
        /// Была ли отпущена ЛКМ.
        /// </summary>
        private bool _isLeftMouseButtonReleased;
        /// <summary>
        /// Нажата ли ПКМ.
        /// </summary>
        private bool _isRightMouseButtonPressed;
        /// <summary>
        /// Была ли отпущена ПКМ.
        /// </summary>
        private bool _isRightMouseButtonReleased;

        /// <inheritdoc />
        public Game(IGameTimer gameTimer, ILogger logger, IInputManager inputManager)
        {
            _gameTimer = gameTimer;
            _logger = logger;
            _inputManager = inputManager;

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

            _gameTimer.TimerTick += UpdateScene;
            _gameTimer.Start();

            // Перед началом боя обновляем сцену, чтобы все объекты успели отрисоваться на старте.
            SceneRedraw?.Invoke(this, new SceneUpdatingEventArgs(0));

            // Подписываемся на события перемещения курсора и нажатия кнопок.
            _inputManager.MouseButtonEvent += OnMouseStateChanged;
            _inputManager.KeyButtonEvent += OnKeyStateChanged;
        }

        /// <summary>
        /// Остановить внутренний таймер, который обновляет объекты на сцене.
        /// </summary>
        public void Stop()
        {
            _stopwatch?.Stop();
            _stopwatch = null;

            _gameTimer.Stop();
            _gameTimer.TimerTick -= UpdateScene;
        }

        /// <summary>
        /// Обновить состояние сцены.
        /// </summary>
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

        /// <summary>
        /// Обработать все события курсора, которые произошли с момента последнего обновления сцены.
        /// </summary>
        private void ProcessCursorEvents()
        {
            var mousePosition = _inputManager.MousePosition;
            var selectedGameObject = GameObjects
                .OrderBy(go => go.Y)
                .FirstOrDefault(go => go.IsInteractive &&
                                      go.X * GameInfo.Scale <= mousePosition.X && mousePosition.X < (go.X + go.Width) * GameInfo.Scale &&
                                      go.Y * GameInfo.Scale <= mousePosition.Y && mousePosition.Y < (go.Y + go.Height) * GameInfo.Scale);

            // Если изменился выбранный объект, то отправляем события снятия выделения/выделения.
            if (selectedGameObject != _selectedGameObject)
            {
                if (_selectedGameObject != null)
                    GameObjectAction?.Invoke(this, new GameObjectActionEventArgs(GameObjectActionType.Unselected, _selectedGameObject));

                if (selectedGameObject != null)
                    GameObjectAction?.Invoke(this, new GameObjectActionEventArgs(GameObjectActionType.Selected, selectedGameObject));

                _selectedGameObject = selectedGameObject;
            }

            // Обрабатываем события нажатия ЛКМ и ПКМ.
            if (_selectedGameObject != null)
            {
                if (_isRightMouseButtonPressed)
                    GameObjectAction?.Invoke(this, new GameObjectActionEventArgs(GameObjectActionType.RightButtonPressed, _selectedGameObject));

                if (_isLeftMouseButtonPressed)
                    GameObjectAction?.Invoke(this, new GameObjectActionEventArgs(GameObjectActionType.LeftButtonPressed, _selectedGameObject));
            }

            // Обрабатываем события того, что ПКМ или ЛКМ были отпущены.
            if (_isRightMouseButtonReleased)
                GameObjectAction?.Invoke(this, new GameObjectActionEventArgs(GameObjectActionType.RightButtonReleased, _selectedGameObject));

            if (_isLeftMouseButtonReleased)
                GameObjectAction?.Invoke(this, new GameObjectActionEventArgs(GameObjectActionType.LeftButtonReleased, _selectedGameObject));


            _isLeftMouseButtonPressed = false;
            _isLeftMouseButtonReleased = false;
            _isRightMouseButtonPressed = false;
            _isRightMouseButtonReleased = false;
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

        /// <summary>
        /// Обработать событие от курсора.
        /// </summary>
        private void OnMouseStateChanged(object sender, MouseButtonEventArgs args)
        {
            if (args.MouseButton == MouseButton.Left) {
                if (args.ButtonState == ButtonState.Pressed)
                    _isLeftMouseButtonPressed = true;
                else
                    _isLeftMouseButtonReleased = true;

                return;
            }

            if (args.MouseButton == MouseButton.Right) {
                if (args.ButtonState == ButtonState.Pressed)
                    _isRightMouseButtonPressed = true;
                else
                    _isRightMouseButtonReleased = true;

                return;
            }
        }

        /// <summary>
        /// Обработать событие от клавиатуры.
        /// </summary>
        private void OnKeyStateChanged(object sender, KeyButtonEventArgs args)
        {
            // Обрабатываем только нажатию на клавишу.
            if (args.ButtonState != ButtonState.Pressed)
                return;

            var button = _gameObjects.OfType<ButtonObject>().FirstOrDefault(b => b.Hotkey == args.KeyboardButton);
            if (button == null || button.ButtonState == SceneButtonState.Disabled)
                return;

            button.OnButtonClicked();
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