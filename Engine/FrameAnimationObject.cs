using System.Collections.Generic;

using Avalonia;

using Engine.Battle.Components;
using Engine.Components;
using Engine.Interfaces;
using Engine.Models;

namespace Engine
{
    public class FrameAnimationObject : GameObject
    {
        public FrameAnimationObject(IMapVisual mapVisual, IReadOnlyList<Frame> frames, double x, double y, int layer)
        {
            BattleObjectComponent = new BattleObjectComponent(this) {
                Position = new Point(x, y)
            };
            FrameAnimationComponent = new FrameAnimationComponent(this, mapVisual, frames, layer);

            Components = new IComponent[] {
                BattleObjectComponent,
                FrameAnimationComponent
            };
        }


        public BattleObjectComponent BattleObjectComponent { get; }

        public FrameAnimationComponent FrameAnimationComponent { get; }
    }
}
