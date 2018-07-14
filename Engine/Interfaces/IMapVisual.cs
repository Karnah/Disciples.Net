using System.Collections.Generic;

using Engine.Models;

namespace Engine.Interfaces
{
    public interface IMapVisual
    {
        IReadOnlyCollection<VisualObject> Visuals { get; }


        void AddVisual(VisualObject visual);

        void RemoveVisual(VisualObject visual);
    }
}
