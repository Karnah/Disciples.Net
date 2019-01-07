using System.Collections.Generic;
using System.Linq;

using Avalonia.Collections;

using Disciples.Engine;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.SceneObjects;
using Disciples.Engine.Models;

namespace Disciples.Avalonia.Controllers
{
    /// <inheritdoc />
    public class Scene : IScene
    {
        private readonly IGame _game;
        private readonly AvaloniaList<ISceneObject> _visuals;
        private readonly IList<ISceneObject> _addVisualBuffer;
        private readonly IList<ISceneObject> _removeVisualBuffer;

        /// <inheritdoc />
        public Scene(IGame game)
        {
            _game = game;
            _visuals = new AvaloniaList<ISceneObject>();
            _addVisualBuffer = new List<ISceneObject>();
            _removeVisualBuffer = new List<ISceneObject>();

            _game.SceneRedraw += OnSceneRedraw;
        }


        /// <inheritdoc />
        public IReadOnlyList<ISceneObject> SceneObjects => _visuals;

        /// <inheritdoc />
        public void AddSceneObject(ISceneObject sceneObject)
        {
            _addVisualBuffer.Add(sceneObject);
        }

        /// <inheritdoc />
        public void RemoveSceneObject(ISceneObject sceneObject)
        {
            //Обрабатываем ситуацию, когда объект был добавлен и тут же удалён.
            if (_addVisualBuffer.Contains(sceneObject)) {
                _addVisualBuffer.Remove(sceneObject);
                return;
            }

            _removeVisualBuffer.Add(sceneObject);
        }


        /// <summary>
        /// Обновить список объектов на сцене во время отрисовки сцены.
        /// </summary>
        private void OnSceneRedraw(object sender, SceneUpdatingEventArgs args)
        {
            if (_removeVisualBuffer.Any()) {
                _visuals.RemoveAll(_removeVisualBuffer);

                foreach (var visualObject in _removeVisualBuffer) {
                    visualObject.Destroy();
                }
                _removeVisualBuffer.Clear();
            }

            if (_addVisualBuffer.Any()) {
                _visuals.AddRange(_addVisualBuffer);
                _addVisualBuffer.Clear();
            }
        }
    }
}