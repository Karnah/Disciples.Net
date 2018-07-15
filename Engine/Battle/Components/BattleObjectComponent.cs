using Avalonia;

using Engine.Battle.Enums;
using Engine.Components;


namespace Engine.Battle.Components
{
    public class BattleObjectComponent : Component
    {
        public BattleObjectComponent(GameObject gameObject) : base(gameObject)
        { }


        public Rect Position { get; set; }

        public BattleDirection Direction { get; set; }

        public BattleAction Action { get; set; }

        public bool IsSelected { get; set; }
    }
}
