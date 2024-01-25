using System.Collections.Generic;
using System.Linq;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Extensions;

namespace Disciples.Engine.Common.Models;

/// <summary>
/// Эффект, наложенный на юнита во время схватки.
/// </summary>
public class UnitBattleEffect
{
    /// <summary>
    /// Создать объект типа <see cref="UnitBattleEffect" />.
    /// </summary>
    public UnitBattleEffect(
        UnitAttackType attackType,
        UnitAttackSource attackSource,
        EffectDuration duration,
        Unit durationControlUnit,
        int? power,
        IReadOnlyList<UnitAttackTypeProtection> attackTypeProtections,
        IReadOnlyList<UnitAttackSourceProtection> attackSourceProtections)
    {
        AttackType = attackType;
        AttackSource = attackSource;
        Duration = duration;
        DurationControlUnit = durationControlUnit;
        Power = power;
        AttackTypeProtections = attackTypeProtections.ToList();
        AttackSourceProtections = attackSourceProtections.ToList();
    }

    /// <summary>
    /// Тип эффекта, оказываемого на юнита.
    /// </summary>
    public UnitAttackType AttackType { get; }

    /// <summary>
    /// Источник эффекта.
    /// </summary>
    public UnitAttackSource AttackSource { get; }

    /// <summary>
    /// Раунд, в котором эффект сработал в последний раз.
    /// </summary>
    public int RoundTriggered { get; set; }

    /// <summary>
    /// Длительность эффекта.
    /// </summary>
    public EffectDuration Duration { get; }

    /// <summary>
    /// Юнит, к ходу которого привязана длительность.
    /// </summary>
    public Unit DurationControlUnit { get; }

    /// <summary>
    /// Сила эффекта.
    /// </summary>
    public int? Power { get; }

    /// <summary>
    /// Защиты от типов атак.
    /// </summary>
    public List<UnitAttackTypeProtection> AttackTypeProtections { get; }

    /// <summary>
    /// Защиты от источников атак.
    /// </summary>
    public List<UnitAttackSourceProtection> AttackSourceProtections { get; }

    /// <summary>
    /// Признак, что эффект можно вылечить.
    /// </summary>
    public bool CanCure()
    {
        // Можно снимать только те эффекты, которые могут пройти со временем.
        if (Duration.IsInfinitive)
            return false;

        // Можно снимать только отрицательные эффекты.
        return AttackType.IsEnemyAttack();
    }
}