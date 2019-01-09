using System.Collections.Generic;
using System.Linq;

using ReactiveUI.Legacy;

using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.SceneObjects;

// Используем ReactiveList не смотря на то, что он помечен устаревшим.
#pragma warning disable 618

namespace Disciples.WPF.Controllers
{
    /// <inheritdoc />
    public class WpfSceneContainer : ISceneContainer
    {
        private readonly ReactiveList<ISceneObject> _visuals;
        private readonly IList<ISceneObject> _addVisualBuffer;
        private readonly IList<ISceneObject> _removeVisualBuffer;

        /// <inheritdoc />
        public WpfSceneContainer()
        {
            _visuals = new ReactiveList<ISceneObject>();
            _addVisualBuffer = new List<ISceneObject>();
            _removeVisualBuffer = new List<ISceneObject>();
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
            // Обрабатываем ситуацию, когда объект был добавлен и тут же удалён.
            if (_addVisualBuffer.Contains(sceneObject)) {
                _addVisualBuffer.Remove(sceneObject);
                return;
            }

            _removeVisualBuffer.Add(sceneObject);
        }

        /// <inheritdoc />
        public void UpdateContainer()
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