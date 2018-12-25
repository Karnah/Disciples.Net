using System.Collections.Generic;
using System.Linq;

using Avalonia.Collections;

using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.VisualObjects;
using Disciples.Engine.Models;

namespace Disciples.Engine.Implementation.Controllers
{
    /// <inheritdoc />
    public class MapVisual : IMapVisual
    {
        private readonly IGame _game;
        private readonly AvaloniaList<VisualObject> _visuals;
        private readonly IList<VisualObject> _addVisualBuffer;
        private readonly IList<VisualObject> _removeVisualBuffer;

        /// <inheritdoc />
        public MapVisual(IGame game)
        {
            _game = game;
            _visuals = new AvaloniaList<VisualObject>();
            _addVisualBuffer = new List<VisualObject>();
            _removeVisualBuffer = new List<VisualObject>();

            _game.SceneRedraw += OnSceneRedraw;
        }


        /// <inheritdoc />
        public IReadOnlyCollection<VisualObject> Visuals => _visuals;

        /// <inheritdoc />
        public void AddVisual(VisualObject visual)
        {
            _addVisualBuffer.Add(visual);
        }

        /// <inheritdoc />
        public void RemoveVisual(VisualObject visual)
        {
            //Обрабатываем ситуацию, когда объект был добавлен и тут же удалён.
            if (_addVisualBuffer.Contains(visual)) {
                _addVisualBuffer.Remove(visual);
                return;
            }

            _removeVisualBuffer.Add(visual);
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