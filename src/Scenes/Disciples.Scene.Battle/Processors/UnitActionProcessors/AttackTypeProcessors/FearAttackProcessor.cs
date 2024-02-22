using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors.Base;
using static Disciples.Scene.Battle.Extensions.UnitAttackProcessorExtensions;

namespace Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors;

/// <summary>
/// Процессор для атаки типа <see cref="UnitAttackType.Fear" />.
/// </summary>
/// <remarks>
/// TODO Если отступать нельзя (например, в городе), то страх действует как паралич.
/// </remarks>
internal class FearAttackProcessor : IAttackTypeProcessor
{
    /// <inheritdoc />
    public UnitAttackType AttackType => UnitAttackType.Fear;

    /// <inheritdoc />
    public bool CanMainAttackBeSkipped => false;

    /// <inheritdoc />
    public bool CanAttack(AttackProcessorContext context, UnitAttack unitAttack, int? power)
    {
        return CanAttackEnemy(context) &&
               !context.TargetUnit.Effects.IsRetreating;
    }

    /// <inheritdoc />
    public CalculatedAttackResult CalculateAttackResult(AttackProcessorContext context, UnitAttack unitAttack,
        int? power,
        int? basePower)
    {
        return new CalculatedAttackResult(
            context,
            unitAttack.AttackType,
            unitAttack.AttackSource,
            power);
    }

    /// <inheritdoc />
    public void ProcessAttack(CalculatedAttackResult attackResult)
    {
        var targetUnit = attackResult.Context.TargetUnit;
        targetUnit.Effects.IsRetreating = true;
    }
}