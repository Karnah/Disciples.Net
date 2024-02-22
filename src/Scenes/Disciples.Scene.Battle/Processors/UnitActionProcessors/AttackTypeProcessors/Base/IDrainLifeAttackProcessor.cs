using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Models;

namespace Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors.Base;

/// <summary>
/// Дополнительный интерфейс для атак типа <see cref="UnitAttackType.DrainLife" /> и <see cref="UnitAttackType.DrainLifeOverflow" />.
/// </summary>
internal interface IDrainLifeAttackProcessor : IAttackTypeProcessor
{
    /// <summary>
    /// Обработать лечение от выпитой жизни.
    /// </summary>
    IReadOnlyList<Unit> ProcessDrainLifeHeal(IReadOnlyList<CalculatedAttackResult> attackResults);
}