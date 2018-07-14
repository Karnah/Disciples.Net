using System.Collections.Generic;

using Engine.Enums;
using Engine.Models;

namespace Engine.Interfaces
{
    public interface IBitmapResources
    {
        IReadOnlyList<Frame> GetBitmapResources(string name, string code, Action action, Direction direction);
    }
}
