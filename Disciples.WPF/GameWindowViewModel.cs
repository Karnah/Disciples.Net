using System;
using System.Collections.Generic;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using Disciples.Engine.Base;
using Disciples.Engine.Common.SceneObjects;

namespace Disciples.WPF;

/// <summary>
/// ViewModel для окна игры.
/// </summary>
public class GameWindowViewModel : ReactiveObject
{
    private readonly IGameController _gameController;

    /// <inheritdoc />
    public GameWindowViewModel(IGameController gameController)
    {
        _gameController = gameController;
        _gameController.SceneChanged += OnSceneChanged;

        SceneObjects = _gameController.CurrentSceneContainer?.SceneObjects;
    }


    /// <summary>
    /// Объекты, которые отображаются на сцене.
    /// </summary>
    [Reactive]
    public IReadOnlyList<ISceneObject> SceneObjects { get; private set; }


    /// <summary>
    /// Обработать событие изменения сцены.
    /// </summary>
    private void OnSceneChanged(object sender, EventArgs e)
    {
        SceneObjects = _gameController.CurrentSceneContainer?.SceneObjects;
    }
}