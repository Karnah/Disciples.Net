using System;

using Avalonia;

using Engine.Battle.Components;
using Engine.Battle.Enums;
using Engine.Battle.Providers;
using Engine.Components;
using Engine.Models;

namespace Engine
{
    public class BattleUnit : GameObject
    {
        public BattleUnit(IBattleUnitResourceProvider battleUnitResourceProvider, Unit unit, int x, int y, BattleDirection direction)
        {
            Unit = unit;

            var coor = GameInfo.OffsetCoordinates(x, y);
            BattleObjectComponent = new BattleObjectComponent(this) {
                Position = new Rect(coor.X, coor.Y, 100, 100),
                Direction = direction,
                Action = BattleAction.Waiting,
            };

            BattleUnitAnimationComponent = new BattleUnitAnimationComponent(this, battleUnitResourceProvider, unit.UnitType.UnitTypeId);

            this.Components = new IComponent[] {
                BattleObjectComponent, BattleUnitAnimationComponent
            };
        }


        public Unit Unit { get; }

        public BattleObjectComponent BattleObjectComponent { get; }

        public BattleUnitAnimationComponent BattleUnitAnimationComponent { get; }



        private Func<bool> _callbackCondition;
        private Action _mainAction;
        private Action _callbackAction;

        public void AttackUnit(BattleUnit attackUnit, Action afterAttack)
        {
            this.BattleObjectComponent.Action = BattleAction.Attacking;

            _callbackCondition = () => this.BattleObjectComponent.Action == BattleAction.Waiting;
            _mainAction = () => {
                //if (this.BattleUnitAnimationComponent.FrameIndex >= 14) {
                if (this.BattleUnitAnimationComponent.FramesCount - this.BattleUnitAnimationComponent.FrameIndex <= 12) {
                    attackUnit.BattleObjectComponent.Action = BattleAction.TakingDamage;

                    _mainAction = null;
                }
            };
            _callbackAction = afterAttack;
        }


        public override void OnUpdate(long tickCount)
        {
            if (_callbackCondition?.Invoke() == true) {
                _callbackAction.Invoke();

                _callbackCondition = null;
                _mainAction = null;
                _callbackAction = null;
            }

            base.OnUpdate(tickCount);

            _mainAction?.Invoke();
        }
    }
}
