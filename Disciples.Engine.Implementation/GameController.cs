using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Disciples.Engine.Base;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Enums;
using Disciples.Engine.Models;
using Disciples.Engine.Platform.Enums;
using Disciples.Engine.Platform.Events;
using Disciples.Engine.Platform.Managers;

namespace Disciples.Engine.Implementation;

/// <inheritdoc />
public class GameController : IGameController
{
    private readonly IGameTimer _gameTimer;
    private readonly ILogger _logger;
    private readonly IInputManager _inputManager;
    private readonly LinkedList<GameObject> _gameObjects;

    private readonly object _lock = new();
    private readonly Stopwatch _stopwatch = new();

    private long _ticks;
    private IScene? _currentScene;

    /// <summary>
    /// Объект, который в данный момент выделен курсором.
    /// </summary>
    private GameObject? _selectedGameObject;
    /// <summary>
    /// События от устройств ввода.
    /// </summary>
    private readonly List<InputDeviceEvent> _inputDeviceEvents;

    /// <summary>
    /// Создать объект типа <see cref="GameController" />.
    /// </summary>
    public GameController(IGameTimer gameTimer, ILogger logger, IInputManager inputManager)
    {
        _gameTimer = gameTimer;
        _logger = logger;
        _inputManager = inputManager;

        _gameObjects = new LinkedList<GameObject>();
        _inputDeviceEvents = new();
    }


    /// <inheritdoc />
    public IReadOnlyCollection<GameObject> GameObjects => _gameObjects;

    /// <inheritdoc />
    public ISceneContainer? CurrentSceneContainer => _currentScene?.SceneContainer;


    /// <inheritdoc />
    public event EventHandler? SceneChanged;


    /// <summary>
    /// Запустить внутренний таймер, который обновляет объекты на сцене.
    /// </summary>
    public void Start()
    {
        _stopwatch.Restart();
        _ticks = _stopwatch.ElapsedMilliseconds;

        _gameTimer.TimerTick += UpdateScene;
        _gameTimer.Start();

        // Подписываемся на события перемещения курсора и нажатия кнопок.
        _inputManager.MouseButtonEvent += OnMouseStateChanged;
        _inputManager.KeyButtonEvent += OnKeyStateChanged;
    }

    /// <summary>
    /// Остановить внутренний таймер, который обновляет объекты на сцене.
    /// </summary>
    public void Stop()
    {
        _stopwatch.Stop();

        _gameTimer.Stop();
        _gameTimer.TimerTick -= UpdateScene;
    }

    /// <summary>
    /// Обновить состояние сцены.
    /// </summary>
    private void UpdateScene(object sender, EventArgs args)
    {
        lock (_lock)
        {
            var ticks = _stopwatch.ElapsedMilliseconds;
            var ticksCount = ticks - _ticks;
            _ticks = ticks;

            try
            {
                CheckInputDeviceSelection();

                var data = new UpdateSceneData(ticksCount, _inputDeviceEvents);

                _currentScene?.BeforeSceneUpdate(data);
                UpdateGameObjects(ticksCount);
                _currentScene?.AfterSceneUpdate(data);

                CurrentSceneContainer?.UpdateContainer();
            }
            catch (Exception e)
            {
                _logger.LogError("Ошибка в цикле", e);
            }
            finally
            {
                _inputDeviceEvents.Clear();
            }
        }
    }

    /// <summary>
    /// Проверить, если было изменение выделенного объекта.
    /// </summary>
    private void CheckInputDeviceSelection()
    {
        var mousePosition = _inputManager.MousePosition;
        var selectedGameObject = GameObjects
            .OrderBy(go => go.Y)
            .FirstOrDefault(go => go.IsInteractive &&
                                  go.X <= mousePosition.X && mousePosition.X < (go.X + go.Width) &&
                                  go.Y <= mousePosition.Y && mousePosition.Y < (go.Y + go.Height));

        // Если объект не менялся, то ничего делать не нужно.
        if (selectedGameObject == _selectedGameObject)
            return;

        if (_selectedGameObject != null)
            _inputDeviceEvents.Add(new InputDeviceEvent(InputDeviceActionType.Selection, InputDeviceActionState.Deactivated, _selectedGameObject));

        if (selectedGameObject != null)
            _inputDeviceEvents.Add(new InputDeviceEvent(InputDeviceActionType.Selection, InputDeviceActionState.Activated, selectedGameObject));

        _selectedGameObject = selectedGameObject;
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
                gameObject.Update(ticksCount);
            }

            gameObjectNode = nextNode;
        }
    }

    /// <summary>
    /// Обработать событие от курсора.
    /// </summary>
    private void OnMouseStateChanged(object sender, MouseButtonEventArgs args)
    {
        var actionType = args.MouseButton == MouseButton.Left
            ? InputDeviceActionType.MouseLeft
            : InputDeviceActionType.MouseRight;
        var actionState = args.ButtonState == ButtonState.Pressed
            ? InputDeviceActionState.Activated
            : InputDeviceActionState.Deactivated;

        _inputDeviceEvents.Add(new InputDeviceEvent(actionType, actionState, _selectedGameObject));
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

        _inputDeviceEvents.Add(new InputDeviceEvent(InputDeviceActionType.UiButton, InputDeviceActionState.Activated, button));
    }

    /// <inheritdoc />
    public void CreateObject(GameObject gameObject)
    {
        lock (_lock) {
            gameObject.Initialize();
            _gameObjects.AddLast(gameObject);
        }
    }

    /// <inheritdoc />
    public void DestroyObject(GameObject gameObject)
    {
        lock (_lock) {
            gameObject.Destroy();
        }
    }

    /// <inheritdoc />
    public async void ChangeScene<TSceneController, TData>(TSceneController sceneController, TData data)
        where TSceneController : IScene, ISupportLoadingWithParameters<TData>
        where TData : SceneParameters
    {
        _currentScene?.Unload();

        // todo сцена "подождите, идёт загрузка"?

        sceneController.InitializeParameters(data);

        await Task.Run(sceneController.Load);

        _currentScene = sceneController;

        CurrentSceneContainer!.UpdateContainer();

        SceneChanged?.Invoke(this, EventArgs.Empty);
    }
}