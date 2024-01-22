using Disciples.Engine.Common.Models;

namespace Disciples.Scene.Battle.Models;

/// <summary>
/// Юнит, который исцеляется с помощью вампиризма.
/// </summary>
internal class DrainLifeHealUnit
{
    /// <summary>
    /// Создать объект типа <see cref="DrainLifeHealUnit" />.
    /// </summary>
    public DrainLifeHealUnit(Unit targetUnit, int healPower)
    {
        TargetUnit = targetUnit;
        HealPower = healPower;
    }

    /// <summary>
    /// Юнит, который получает лечение.
    /// </summary>
    public Unit TargetUnit { get; }

    /// <summary>
    /// Сила лечения.
    /// </summary>
    public int HealPower { get; }
}