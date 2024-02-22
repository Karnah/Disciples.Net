using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors.Base;

namespace Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors;

/// <summary>
/// Процессор для атаки типа <see cref="UnitAttackType.Summon" />.
/// </summary>
/// <remarks>
/// TODO Реализовать.
/// </remarks>
internal class SummonAttackProcessor : IAttackTypeProcessor
{
    /// <inheritdoc />
    public UnitAttackType AttackType => UnitAttackType.Summon;

    /// <inheritdoc />
    public bool CanMainAttackBeSkipped => false;

    /// <inheritdoc />
    public bool CanAttack(AttackProcessorContext context, UnitAttack unitAttack, int? power)
    {
        return false;
    }

    /// <inheritdoc />
    public CalculatedAttackResult CalculateAttackResult(AttackProcessorContext context, UnitAttack unitAttack,
        int? power, int? basePower)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void ProcessAttack(CalculatedAttackResult attackResult)
    {
        throw new NotImplementedException();
    }
}