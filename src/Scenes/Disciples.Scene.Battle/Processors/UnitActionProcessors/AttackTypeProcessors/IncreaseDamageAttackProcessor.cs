using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Extensions;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors.Base;

namespace Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors;

/// <summary>
/// Процессор для атаки типа <see cref="UnitAttackType.IncreaseDamage" />.
/// </summary>
internal class IncreaseDamageAttackProcessor : BaseEffectAttackProcessor
{
    /// <inheritdoc />
    public override UnitAttackType AttackType => UnitAttackType.IncreaseDamage;

    /// <inheritdoc />
    protected override bool CanAttackFriends => true;

    public override bool CanAttack(AttackProcessorContext context, UnitAttack unitAttack, int? power)
    {
        return base.CanAttack(context, unitAttack, power) &&
               // Можно усиливать только юнитов с прямым уроном от первой атаки.
               context.TargetUnit.UnitType.MainAttack.AttackType.IsDirectDamage();
    }

    /// <inheritdoc />
    protected override EffectDuration GetEffectDuration(UnitAttack unitAttack, bool isMaximum)
    {
        return unitAttack.IsInfinitive
            ? EffectDuration.CreateInfinitive()
            : EffectDuration.Create(1);
    }
}