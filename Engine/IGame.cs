using System.Collections.Generic;

namespace Engine
{
    public interface IGame
    {
        IList<GameObject> GameObjects { get; }
    }
}
