using System;
using System.Collections.Generic;

using Avalonia;

using Engine.Battle.Components;
using Engine.Battle.Enums;
using Engine.Battle.Providers;
using Engine.Components;
using Engine.Extensions;
using Engine.Interfaces;
using Engine.Models;

namespace Engine
{
    public class BattleUnit : GameObject
    {
        // Разброс атаки при ударе
        private const int AttackRange = 5;


        public BattleUnit(IMapVisual mapVisual, IBattleUnitResourceProvider battleUnitResourceProvider, Unit unit, int x, int y, BattleDirection direction)
        {
            Unit = unit;

            var coor = GameInfo.OffsetCoordinates(x, y);
            BattleObjectComponent = new BattleObjectComponent(this) {
                Position = new Rect(coor.X, coor.Y, 100, 100),
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



        private Func<bool> _callbackCondition;
        private Action _mainAction;
        private Action _callbackAction;

        public void AttackUnits(IReadOnlyCollection<BattleUnit> targetUnits, Action afterAttack)
        {
            this.BattleObjectComponent.Action = BattleAction.Attacking;

            _callbackCondition = () => this.BattleObjectComponent.Action == BattleAction.Waiting;
            _mainAction = () => {
                // todo нужно научиться определять на каком фрейме происходит удар
                //if (this.BattleUnitAnimationComponent.FrameIndex >= 14) {
                if (this.BattleUnitAnimationComponent.FramesCount - this.BattleUnitAnimationComponent.FrameIndex <= 12) {
                    var isAttacking = Unit.HasEnemyAbility();

                    foreach (var targetUnit in targetUnits) {
                        // Проверяем меткость юнита
                        var chanceAttack = RandomGenerator.Next(0, 100);
                        if (chanceAttack > Unit.UnitType.FirstAttack.Accuracy)
                            continue;

                        // Если юнит атакует врага, а не лечит союзника, то цели вызываем анимацию получения повреждений
                        if (isAttacking) {
                            targetUnit.BattleObjectComponent.Action = BattleAction.TakingDamage;

                            var damage = Unit.UnitType.FirstAttack.DamagePower + RandomGenerator.Next(AttackRange);
                            damage = (int)(damage * (1 - targetUnit.Unit.UnitType.Armor / 100.0));

                            targetUnit.Unit.ChangeHitPoints(- damage);
                        }
                        else {
                            // todo пока только целители
                            var heal = Unit.UnitType.FirstAttack.HealPower;
                            targetUnit.Unit.ChangeHitPoints(heal);
                        }
                    }

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
