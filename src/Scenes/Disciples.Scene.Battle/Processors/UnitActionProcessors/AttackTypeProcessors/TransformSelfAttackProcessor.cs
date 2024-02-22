using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors.Base;

namespace Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors;

/// <summary>
/// Процессор для атаки типа <see cref="UnitAttackType.TransformSelf" />.
/// </summary>
/// <remarks>
/// TODO Реализовать.
/// </remarks>
internal class TransformSelfAttackProcessor : BaseEffectAttackProcessor
{
    /// <inheritdoc />
    public override UnitAttackType AttackType => UnitAttackType.TransformSelf;

    /// <inheritdoc />
    protected override EffectDuration GetEffectDuration(UnitAttack unitAttack, bool isMaximum)
    {
        return unitAttack.IsInfinitive
            ? EffectDuration.CreateInfinitive()
            : EffectDuration.Create(1);
    }
}