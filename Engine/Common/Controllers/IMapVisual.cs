using System.Collections.Generic;

using Engine.Common.Models;

namespace Engine.Common.Controllers
{
    public interface IMapVisual
    {
        IReadOnlyCollection<VisualObject> Visuals { get; }


        void AddVisual(VisualObject visual);

        void RemoveVisual(VisualObject visual);
    }
}
