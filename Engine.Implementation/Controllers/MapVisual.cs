using System;
using System.Collections.Generic;
using System.Linq;

using ReactiveUI;

using Engine.Common.Controllers;
using Engine.Common.Models;

namespace Engine.Implementation.Controllers
{
    public class MapVisual : IMapVisual
    {
        private readonly IGame _game;
        private readonly ReactiveList<VisualObject> _visuals;
        private readonly IList<VisualObject> _addVisualBuffer;
        private readonly IList<VisualObject> _removeVisualBuffer;

        public MapVisual(IGame game)
        {
            _game = game;
            _visuals = new ReactiveList<VisualObject>();
            _addVisualBuffer = new List<VisualObject>();
            _removeVisualBuffer = new List<VisualObject>();

            _game.SceneRedraw += OnSceneRedraw;
        }


        public IReadOnlyCollection<VisualObject> Visuals => _visuals;

        public void AddVisual(VisualObject visual)
        {
            _addVisualBuffer.Add(visual);
        }

        public void RemoveVisual(VisualObject visual)
        {
            _removeVisualBuffer.Add(visual);
        }


        // Все объекты складываются в буфер, который потом централизовано обновляется
        private void OnSceneRedraw(object sender, EventArgs eventArgs)
        {
            if (_removeVisualBuffer.Any()) {
                _visuals.RemoveAll(_removeVisualBuffer);
                _removeVisualBuffer.Clear();
            }

            if (_addVisualBuffer.Any()) {
                _visuals.AddRange(_addVisualBuffer);
                _addVisualBuffer.Clear();
            }
        }
    }
}
