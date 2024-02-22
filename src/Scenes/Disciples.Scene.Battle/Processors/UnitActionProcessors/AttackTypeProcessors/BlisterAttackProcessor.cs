using Disciples.Engine.Common.Enums.Units;
using Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors.Base;

namespace Disciples.Scene.Battle.Processors.UnitActionProcessors.AttackTypeProcessors;

/// <summary>
/// Процессор для атаки типа <see cref="UnitAttackType.Blister" />.
/// </summary>
internal class BlisterAttackProcessor : BaseDamageEffectAttackProcessor
{
    /// <inheritdoc />
    public override UnitAttackType AttackType => UnitAttackType.Blister;
}