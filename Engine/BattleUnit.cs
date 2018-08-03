using Avalonia;

using Engine.Battle.Components;
using Engine.Battle.Enums;
using Engine.Battle.Providers;
using Engine.Components;
using Engine.Interfaces;
using Engine.Models;

namespace Engine
{
    public class BattleUnit : GameObject
    {
        public BattleUnit(IMapVisual mapVisual, IBattleUnitResourceProvider battleUnitResourceProvider, Unit unit, int x, int y, BattleDirection direction)
        {
            Unit = unit;

            var coor = GameInfo.OffsetCoordinates(x, y);
            BattleObjectComponent = new BattleObjectComponent(this) {
                Position = new Point(coor.X, coor.Y),
                Direction = direction,
                Action = BattleAction.Waiting,
            };

            BattleUnitAnimationComponent = new BattleUnitAnimationComponent(this, mapVisual, battleUnitResourceProvider, unit.UnitType.UnitTypeId);

            this.Components = new IComponent[] {
                BattleObjectComponent, BattleUnitAnimationComponent
            };
        }


        public Unit Unit { get; }

        public BattleObjectComponent BattleObjectComponent { get; }

        public BattleUnitAnimationComponent BattleUnitAnimationComponent { get; }
    }
}
