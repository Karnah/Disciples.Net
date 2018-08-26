using System.Collections.Generic;
using System.Collections.ObjectModel;

using Engine.Common.Controllers;
using Engine.Common.Models;

namespace Engine.Implementation.Controllers
{
    public class MapVisual : IMapVisual
    {
        private readonly ObservableCollection<VisualObject> _visuals;

        public MapVisual()
        {
            _visuals = new ObservableCollection<VisualObject>();
        }


        public IReadOnlyCollection<VisualObject> Visuals => _visuals;

        public void AddVisual(VisualObject visual)
        {
            _visuals.Add(visual);
        }

        public void RemoveVisual(VisualObject visual)
        {
            _visuals.Remove(visual);
        }
    }
}
