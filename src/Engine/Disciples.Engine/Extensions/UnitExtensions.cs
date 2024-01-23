using Disciples.Engine.Common.Models;

namespace Disciples.Engine.Extensions;

/// <summary>
/// Набор методов для упрощения работы с юнитами.
/// </summary>
public static class UnitExtensions
{
    /// <summary>
    /// Проверить, что атака юнита может быть направлена на союзников.
    /// </summary>
    public static bool HasAllyAbility(this Unit unit)
    {
        return unit.UnitType.MainAttack.AttackType.IsAllyAttack();
    }

    /// <summary>
    /// Проверить, что атака юнита может быть направлена на врагов.
    /// </summary>
    public static bool HasEnemyAbility(this Unit unit)
    {
        return unit.UnitType.MainAttack.AttackType.IsEnemyAttack();
    }
}