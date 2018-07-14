using Avalonia;

using Engine.Enums;

namespace Engine.Components
{
    public class MapObject : Component
    {
        public MapObject(GameObject gameObject) : base(gameObject)
        { }


        public Rect Position { get; set; }

        public Direction Direction { get; set; }

        public Action Action { get; set; }

        public bool IsSelected { get; set; }
    }
}
