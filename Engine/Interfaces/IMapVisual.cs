using System.Collections.Generic;

using Inftastructure.Models;

namespace Inftastructure.Interfaces
{
    public interface IMapVisual
    {
        IReadOnlyCollection<VisualObject> Visuals { get; }


        void AddVisual(VisualObject visual);

        void RemoveVisual(VisualObject visual);
    }
}
