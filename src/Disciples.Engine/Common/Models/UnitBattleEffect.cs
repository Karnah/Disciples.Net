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
    public UnitBattleEffect(UnitAttackType attackType, UnitAttackSource attackSource, int roundDuration)
    {
        AttackType = attackType;
        AttackSource = attackSource;
        RoundDuration = roundDuration;
    }

    /// <summary>
    /// Создать объект типа <see cref="UnitBattleEffect" />.
    /// </summary>
    public UnitBattleEffect(UnitAttackType attackType, UnitAttackSource attackSource, int roundDuration, int? power)
    {
        AttackType = attackType;
        AttackSource = attackSource;
        RoundDuration = roundDuration;
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
    /// Длительность эффекта в раундах.
    /// </summary>
    public int RoundDuration { get; set; }

    /// <summary>
    /// Сила эффекта.
    /// </summary>
    public int? Power { get; }
}