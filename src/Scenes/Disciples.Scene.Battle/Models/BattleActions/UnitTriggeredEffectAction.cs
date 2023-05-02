using Disciples.Engine.Common.Enums.Units;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.GameObjects;

namespace Disciples.Scene.Battle.Models.BattleActions;

/// <summary>
/// Срабатывание эффекта при ходе юнита.
/// </summary>
internal class UnitTriggeredEffectAction : UnitBattleAction
{
    /// <summary>
    /// Создать объект типа <see cref="UnitTriggeredEffectAction" />.
    /// </summary>
    public UnitTriggeredEffectAction(BattleUnit targetUnit, UnitAttackType attackType, int? power = null, AnimationBattleAction? animationBattleAction = null)
        : base(targetUnit, UnitActionType.TriggeredEffect, attackType, power, animationBattleAction)
    {
    }
}