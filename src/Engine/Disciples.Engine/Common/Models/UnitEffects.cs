using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Disciples.Engine.Common.Enums.Units;

namespace Disciples.Engine.Common.Models;

/// <summary>
/// Эффекты, которые действуют на юнита.
/// </summary>
public class UnitEffects
{
    /// <summary>
    /// Эффекты, которые наложены и действуют во время схватки.
    /// </summary>
    private readonly Dictionary<UnitAttackType, UnitBattleEffect> _battleEffects;

    /// <summary>
    /// Создать объект типа <see cref="UnitEffects" />.
    /// </summary>
    public UnitEffects()
    {
        _battleEffects = new Dictionary<UnitAttackType, UnitBattleEffect>();
    }

    /// <summary>
    /// Есть ли на юните эффекты, которые наложены во время битвы.
    /// </summary>
    public bool HasBattleEffects => IsDefended || IsRetreating || _battleEffects.Count > 0;

    /// <summary>
    /// Признак, что юнит защитился.
    /// </summary>
    public bool IsDefended { get; set; }

    // TODO Переделать на эффект страха?
    /// <summary>
    /// Признак, что юнит собирается сбежать.
    /// </summary>
    public bool IsRetreating { get; set; }

    /// <summary>
    /// Признак, что юнит парализован и не может выполнить ход.
    /// </summary>
    public bool IsParalyzed =>
        ExistsBattleEffect(UnitAttackType.Paralyze) || ExistsBattleEffect(UnitAttackType.Petrify);

    /// <summary>
    /// Признак, что юнит не сможет выполнить следующий ход.
    /// </summary>
    public bool IsDisabled => IsParalyzed || IsRetreating;

    /// <summary>
    /// Добавить эффект в поединке.
    /// </summary>
    public void AddBattleEffect(UnitBattleEffect battleEffect)
    {
        _battleEffects[battleEffect.AttackType] = battleEffect;
    }

    /// <summary>
    /// Проверить, что на юнита наложен эффект указанного типа.
    /// </summary>
    public bool ExistsBattleEffect(UnitAttackType effectAttackType)
    {
        return _battleEffects.ContainsKey(effectAttackType);
    }

    /// <summary>
    /// Проверить, что на юнита наложен эффект указанного типа и получить его.
    /// </summary>
    public bool TryGetBattleEffect(UnitAttackType effectAttackType, [NotNullWhen(true)]out UnitBattleEffect? battleEffect)
    {
        return _battleEffects.TryGetValue(effectAttackType, out battleEffect);
    }

    /// <summary>
    /// Удалить эффект указанного типа.
    /// </summary>
    public void Remove(UnitAttackType effectAttackType)
    {
        _battleEffects.Remove(effectAttackType);
    }

    /// <summary>
    /// Получить эффекты, воздействующие на юнита.
    /// </summary>
    public IReadOnlyList<UnitBattleEffect> GetBattleEffects()
    {
        return _battleEffects.Values.ToArray();
    }

    /// <summary>
    /// Получить модификатор силы атаки.
    /// </summary>
    public decimal GetDamagePowerModifier()
    {
        var modifier = 1M;

        if (_battleEffects.TryGetValue(UnitAttackType.BoostDamage, out var boostDamage))
        {
            modifier *= boostDamage.Power!.Value / 100M + 1;
        }

        if (_battleEffects.TryGetValue(UnitAttackType.ReduceDamage, out var reduceDamage))
        {
            modifier *= 1 - reduceDamage.Power!.Value / 100M;
        }

        return modifier - 1M;
    }

    /// <summary>
    /// Получить модификатор инициативы.
    /// </summary>
    /// <remarks>
    /// Нет атак юнитов, которые влияют на точность.
    /// Только зелья или заклинания. Поэтому сейчас 0.
    /// </remarks>
    public decimal GetAccuracyModifier()
    {
        return 0;
    }

    /// <summary>
    /// Получить модификатор инициативы.
    /// </summary>
    public decimal GetInitiativeModifier()
    {
        var modifier = 1M;

        if (_battleEffects.TryGetValue(UnitAttackType.ReduceInitiative, out var reduceInitiative))
        {
            modifier *= 1 - reduceInitiative.Power!.Value / 100M;
        }

        return modifier - 1M;
    }

    /// <summary>
    /// Удалить все действующие эффекты.
    /// </summary>
    public void Clear()
    {
        IsDefended = false;
        IsRetreating = false;
        _battleEffects.Clear();
    }
}