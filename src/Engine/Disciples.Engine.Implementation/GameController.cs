using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DryIoc;
using Microsoft.Extensions.Logging;
using Disciples.Common.Models;
using Disciples.Engine.Base;
using Disciples.Engine.Common;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Enums;
using Disciples.Engine.Implementation.Extensions;
using Disciples.Engine.Models;
using Disciples.Engine.Platform.Enums;
using Disciples.Engine.Platform.Events;
using Disciples.Engine.Platform.Managers;

namespace Disciples.Engine.Implementation;

/// <inheritdoc />
public class GameController : IGameController
{
    private readonly IContainer _container;
    private readonly IGameTimer _gameTimer;
    private readonly ILogger<GameController> _logger;
    private readonly IInputManager _inputManager;
    private readonly ICursorController _cursorController;

    private readonly object _lock = new();
    private readonly Stopwatch _stopwatch = new();

    private IResolverContext? _sceneResolverContext;

    private long _ticks;
    private IScene? _currentScene;
    private string _currentSceneName = string.Empty;

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
    public GameController(IContainer container, IGameTimer gameTimer, ILogger<GameController> logger, IInputManager inputManager, ICursorController cursorController)
    {
        _container = container;
        _gameTimer = gameTimer;
        _logger = logger;
        _inputManager = inputManager;
        _cursorController = cursorController;

        _inputDeviceEvents = new();
    }

    /// <summary>
    /// Список игровых объектов на сцене.
    /// </summary>
    private IReadOnlyCollection<GameObject> GameObjects => _currentScene?.GameObjectContainer.GameObjects ?? Array.Empty<GameObject>();

    /// <inheritdoc />
    public IPlatformSceneObjectContainer? CurrentSceneContainer => _currentScene?.SceneObjectContainer.PlatformSceneObjectContainer;

    /// <inheritdoc />
    public event EventHandler? SceneChanged;

    /// <summary>
    /// Версия игры.
    /// </summary>
    public string? Version { get; } = Assembly.GetEntryAssembly()?.GetName().Version?.ToString();

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
    /// Выйти из игры.
    /// </summary>
    public void Quit()
    {
        Stop();
        Environment.Exit(0);
    }

    /// <inheritdoc />
    public GameContext LoadGame(string savePath)
    {
        var path = Path.Combine(savePath);
        try
        {
            return File
                .ReadAllText(path)
                .DeserializeFromJson<GameContext>();
        }
        catch (Exception e)
        {
            throw new Exception($"Cannot load save file: {savePath}", e);
        }
    }

    /// <inheritdoc />
    public async void ChangeScene<TScene, TSceneParameters>(TSceneParameters sceneParameters)
        where TScene : IScene<TSceneParameters>
        where TSceneParameters : SceneParameters
    {
        _logger.LogInformation("Change scene started, new scene: {sceneName}", typeof(TScene).Name);

        // Старая сцена должна прекращать обработку всех событий на время загрузки новой.
        _currentScene?.Unload();
        _sceneResolverContext?.Dispose();

        var sceneName = typeof(TScene).Name;
        _sceneResolverContext = _container.OpenScope(sceneName);

        var scene = _sceneResolverContext.Resolve<TScene>(new object[]
        {
            sceneParameters,
            new SceneChangeContext { PreviousSceneName = _currentSceneName }
        });

        await Task.Run(scene.Load);

        _cursorController.SetCursorState(scene.DefaultCursorType);
        _currentScene = scene;
        _currentSceneName = sceneName;
        _selectedGameObject = null;

        // TODO Вынести в сцену.
        CurrentSceneContainer!.UpdateContainer();

        SceneChanged?.Invoke(this, EventArgs.Empty);

        scene.AfterSceneLoaded();

        _logger.LogInformation("New scene loaded");
    }

    /// <summary>
    /// Обновить состояние сцены.
    /// </summary>
    private void UpdateScene(object? sender, EventArgs args)
    {
        lock (_lock)
        {
            var ticks = _stopwatch.ElapsedMilliseconds;
            var ticksCount = ticks - _ticks;
            _ticks = ticks;

            try
            {
                var platformMousePosition = _inputManager.MousePosition;
                var mousePosition = new PointD(platformMousePosition.X, platformMousePosition.Y);

                CheckInputDeviceSelection(mousePosition);

                var data = new UpdateSceneData(ticksCount, mousePosition, _inputDeviceEvents);
                _currentScene?.UpdateScene(data);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error in update scene cycle");
                throw;
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
    private void CheckInputDeviceSelection(PointD mousePosition)
    {
        // Объект выделения не менялся.
        if (_selectedGameObject != null &&
            _selectedGameObject.Bounds.Contains(mousePosition) &&
            !_selectedGameObject.IsHidden &&
            !_selectedGameObject.IsDestroyed &&
            !_selectedGameObject.IsDeactivated &&
            _selectedGameObject.SelectionComponent!.IsSelectionEnabled)
        {
            return;
        }

        // TODO Можно каждый раз не проходиться по всем элементам, а создать отдельную коллекцию.
        var selectedGameObject = GameObjects
            .FirstOrDefault(go => go.SelectionComponent != null &&
                                  go.SelectionComponent.IsSelectionEnabled &&
                                  go.Bounds.Contains(mousePosition) &&
                                  !go.IsHidden &&
                                  !go.IsDeactivated);

        // Здесь будет обработана ситуация, когда объекта выделения не было и не появилось.
        if (selectedGameObject == _selectedGameObject)
            return;

        if (_selectedGameObject != null)
            _inputDeviceEvents.Add(new InputDeviceEvent(InputDeviceActionType.Hover, InputDeviceActionState.Deactivated, _selectedGameObject));

        if (selectedGameObject != null)
            _inputDeviceEvents.Add(new InputDeviceEvent(InputDeviceActionType.Hover, InputDeviceActionState.Activated, selectedGameObject));

        _selectedGameObject = selectedGameObject;
    }

    /// <summary>
    /// Обработать событие от курсора.
    /// </summary>
    private void OnMouseStateChanged(object? sender, MouseButtonEventArgs args)
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
    private void OnKeyStateChanged(object? sender, KeyButtonEventArgs args)
    {
        // Обрабатываем только нажатию на клавишу.
        if (args.ButtonState != ButtonState.Pressed)
            return;

        _inputDeviceEvents.Add(new InputDeviceEvent(InputDeviceActionType.KeyboardButton, InputDeviceActionState.Activated, args.KeyboardButton));
    }
}