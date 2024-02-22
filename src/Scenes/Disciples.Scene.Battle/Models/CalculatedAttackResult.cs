using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;

namespace Disciples.Scene.Battle.Models;

/// <summary>
/// Вычисленный результат атаки.
/// </summary>
internal class CalculatedAttackResult
{
    /// <summary>
    /// Создать объект типа <see cref="CalculatedAttackResult" /> для прямого результата атаки.
    /// </summary>
    public CalculatedAttackResult(AttackProcessorContext context, UnitAttackType attackType, UnitAttackSource attackSource, int? power, int? criticalDamage = null)
    {
        Context = context;
        AttackType = attackType;
        AttackSource = attackSource;
        Power = power;
        CriticalDamage = criticalDamage;
        AttackTypeProtections = Array.Empty<UnitAttackTypeProtection>();
        AttackSourceProtections = Array.Empty<UnitAttackSourceProtection>();
        CuredEffects = Array.Empty<UnitBattleEffect>();
    }

    /// <summary>
    /// Создать объект типа <see cref="CalculatedAttackResult" /> для эффекта.
    /// </summary>
    public CalculatedAttackResult(AttackProcessorContext context,
        UnitAttackType attackType, UnitAttackSource attackSource, int? power,
        EffectDuration effectDuration, Unit effectDurationControlUnit,
        IReadOnlyList<UnitAttackTypeProtection>? attackTypeProtections = null,
        IReadOnlyList<UnitAttackSourceProtection>? attackSourceProtections = null,
        TransformedEnemyUnit? transformedUnit = null)
    {
        Context = context;
        AttackType = attackType;
        AttackSource = attackSource;
        Power = power;
        EffectDuration = effectDuration;
        EffectDurationControlUnit = effectDurationControlUnit;
        AttackTypeProtections = attackTypeProtections ?? Array.Empty<UnitAttackTypeProtection>();
        AttackSourceProtections = attackSourceProtections ?? Array.Empty<UnitAttackSourceProtection>();
        TransformedUnit = transformedUnit;
        CuredEffects = Array.Empty<UnitBattleEffect>();
    }

    /// <summary>
    /// Создать объект типа <see cref="CalculatedAttackResult" /> для исцеления от негативных эффектов.
    /// </summary>
    public CalculatedAttackResult(AttackProcessorContext context, UnitAttackType attackType, UnitAttackSource attackSource, int? power, IReadOnlyList<UnitBattleEffect> curedEffects)
    {
        Context = context;
        AttackType = attackType;
        AttackSource = attackSource;
        Power = power;
        CuredEffects = curedEffects;
        AttackTypeProtections = Array.Empty<UnitAttackTypeProtection>();
        AttackSourceProtections = Array.Empty<UnitAttackSourceProtection>();
    }

    /// <summary>
    /// Контекст для выполнения атаки одного юнита на другого.
    /// </summary>
    public AttackProcessorContext Context { get; }

    /// <summary>
    /// Тип атаки.
    /// </summary>
    public UnitAttackType AttackType { get; }

    /// <summary>
    /// Источник атаки.
    /// </summary>
    public UnitAttackSource AttackSource { get; }

    /// <summary>
    /// Сила воздействия.
    /// </summary>
    public int? Power { get; }

    /// <summary>
    /// Критический урон.
    /// </summary>
    public int? CriticalDamage { get; }

    /// <summary>
    /// Длительность эффекта.
    /// </summary>
    public EffectDuration? EffectDuration { get; }

    /// <summary>
    /// Юнит, к ходу которого привязана длительность.
    /// </summary>
    public Unit? EffectDurationControlUnit { get; }

    /// <summary>
    /// Защита от типов атак.
    /// </summary>
    public IReadOnlyList<UnitAttackTypeProtection> AttackTypeProtections { get; }

    /// <summary>
    /// Защита от источников атак.
    /// </summary>
    public IReadOnlyList<UnitAttackSourceProtection> AttackSourceProtections { get; }

    /// <summary>
    /// Превращённый юнит.
    /// </summary>
    public TransformedEnemyUnit? TransformedUnit { get; }

    /// <summary>
    /// Эффекты, которые были сняты.
    /// </summary>
    public IReadOnlyList<UnitBattleEffect> CuredEffects { get; }
}