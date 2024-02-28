using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors.Base;

namespace Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors;

/// <summary>
/// Процессор для атаки типа <see cref="UnitAttackType.ReduceLevel" />.
/// </summary>
internal class ReduceLevelAttackProcessor : BaseTransformAttackProcessor
{
    /// <inheritdoc />
    public override UnitAttackType AttackType => UnitAttackType.ReduceLevel;

    /// <inheritdoc />
    protected override bool CanAttackEnemies => true;

    /// <inheritdoc />
    public override bool CanAttack(AttackProcessorContext context, CalculatedUnitAttack unitAttack)
    {
        return base.CanAttack(context, unitAttack) &&
               context.TargetUnit.Level > 1;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Тип юнита "Ассасин Империи" имеет третий уровень.
    /// Если юнит этого типа имеет четвертый уровень (т.е. успел получить один дополнительный уровень), то после понижения уровня,
    /// Он останется того же типа, но будет иметь третий уровень.
    /// Если же он имел третий уровень, то он будет превращён в юнита предыдущего типа, т.е. в "Стрелка".
    /// </remarks>
    protected override ITransformedUnit? GetTransformedUnit(Unit attackingUnit,
        Unit targetUnit, CalculatedUnitAttack unitAttack)
    {
        if (targetUnit.Level == targetUnit.UnitType.Level)
            return new FullTransformUnit(targetUnit, targetUnit.UnitType.PreviousUnitType!);

        var transformUnit = new FullTransformUnit(targetUnit, targetUnit.UnitType)
        {
            Level = targetUnit.Level - 1
        };
        return transformUnit;
    }

    /// <inheritdoc />
    protected override EffectDuration GetEffectDuration(CalculatedUnitAttack unitAttack, bool isMaximum)
    {
        return unitAttack.IsInfinitive
            ? EffectDuration.CreateInfinitive()
            : EffectDuration.Create(1);
    }
}