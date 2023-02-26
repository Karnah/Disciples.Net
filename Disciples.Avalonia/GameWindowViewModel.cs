using System;
using System.Collections.Generic;

using ReactiveUI;

using Disciples.Engine.Base;
using Disciples.Engine.Common.SceneObjects;

namespace Disciples.Avalonia
{
    /// <summary>
    /// ViewModel для окна игры.
    /// </summary>
    public class GameWindowViewModel : ReactiveObject
    {
        private readonly IGameController _gameController;
        private IReadOnlyList<ISceneObject>? _sceneObjects;

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
        public IReadOnlyList<ISceneObject>? SceneObjects {
            get => _sceneObjects;
            private set => this.RaiseAndSetIfChanged(ref _sceneObjects, value);
        }


        /// <summary>
        /// Обработать событие изменения сцены.
        /// </summary>
        private void OnSceneChanged(object? sender, EventArgs e)
        {
            SceneObjects = _gameController.CurrentSceneContainer?.SceneObjects;
        }
    }
}