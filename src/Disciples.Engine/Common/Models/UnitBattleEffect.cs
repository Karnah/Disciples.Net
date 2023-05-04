using Disciples.Engine.Common.Enums.Units;

namespace Disciples.Engine.Common.Models;

/// <summary>
/// Эффект, наложенный на юнита во время схватки.
/// </summary>
public class UnitBattleEffect
{
    /// <summary>
    /// Создать объект типа <see cref="UnitBattleEffect" />.
    /// </summary>
    public UnitBattleEffect(UnitAttackType attackType, UnitAttackSource attackSource, EffectDuration duration)
    {
        AttackType = attackType;
        AttackSource = attackSource;
        Duration = duration;
    }

    /// <summary>
    /// Создать объект типа <see cref="UnitBattleEffect" />.
    /// </summary>
    public UnitBattleEffect(UnitAttackType attackType, UnitAttackSource attackSource, EffectDuration duration, int? power)
    {
        AttackType = attackType;
        AttackSource = attackSource;
        Duration = duration;
        Power = power;
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
    /// Сила эффекта.
    /// </summary>
    public int? Power { get; }
}