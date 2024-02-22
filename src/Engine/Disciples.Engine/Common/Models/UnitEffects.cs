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
    private readonly List<UnitBattleEffect> _battleEffects;

    /// <summary>
    /// Создать объект типа <see cref="UnitEffects" />.
    /// </summary>
    public UnitEffects()
    {
        _battleEffects = new List<UnitBattleEffect>();
    }

    /// <summary>
    /// Признак, что юнит защитился.
    /// </summary>
    public bool IsDefended { get; set; }

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
    /// Признак, что на юните есть позитивные модификаторы.
    /// </summary>
    public bool HasPositiveModifiers => _battleEffects.Any(be => be.AttackType == UnitAttackType.GiveProtection);

    /// <summary>
    /// Добавить эффект в поединке.
    /// </summary>
    public void AddBattleEffect(UnitBattleEffect battleEffect)
    {
        _battleEffects.Add(battleEffect);
    }

    /// <summary>
    /// Проверить, что на юнита наложен эффект указанного типа.
    /// </summary>
    public bool ExistsBattleEffect(UnitAttackType effectAttackType)
    {
        return _battleEffects.Any(be => be.AttackType == effectAttackType);
    }

    /// <summary>
    /// Проверить, что на юнита наложен эффект указанного типа и получить его.
    /// </summary>
    public bool TryGetBattleEffect(UnitAttackType effectAttackType, [NotNullWhen(true)]out UnitBattleEffect? battleEffect)
    {
        battleEffect = _battleEffects.FirstOrDefault(be => be.AttackType == effectAttackType);
        return battleEffect != null;
    }

    /// <summary>
    /// Удалить эффект указанного типа.
    /// </summary>
    public void Remove(UnitBattleEffect effect)
    {
        _battleEffects.Remove(effect);
    }

    /// <summary>
    /// Удалить эффект защиты.
    /// </summary>
    public void RemoveBattleProtectionEffect(UnitAttackTypeProtection attackTypeProtection)
    {
        var protectionEffect = _battleEffects.FirstOrDefault(be => be.AttackType == UnitAttackType.GiveProtection
                                                                   && be.AttackTypeProtections.Contains(attackTypeProtection));
        if (protectionEffect == null)
            return;

        if (protectionEffect.AttackTypeProtections.Count == 1
            && protectionEffect.AttackSourceProtections.Count == 0)
        {
            Remove(protectionEffect);
        }
        else
        {
            protectionEffect.AttackTypeProtections.Remove(attackTypeProtection);
        }
    }

    /// <summary>
    /// Удалить эффект защиты.
    /// </summary>
    public void RemoveBattleProtectionEffect(UnitAttackSourceProtection attackSourceProtection)
    {
        var protectionEffect = _battleEffects.FirstOrDefault(be => be.AttackType == UnitAttackType.GiveProtection
                                                                   && be.AttackSourceProtections.Contains(attackSourceProtection));
        if (protectionEffect == null)
            return;

        if (protectionEffect.AttackTypeProtections.Count == 0
            && protectionEffect.AttackSourceProtections.Count == 1)
        {
            Remove(protectionEffect);
        }
        else
        {
            protectionEffect.AttackSourceProtections.Remove(attackSourceProtection);
        }
    }

    /// <summary>
    /// Получить эффекты, воздействующие на юнита.
    /// </summary>
    public IReadOnlyList<UnitBattleEffect> GetBattleEffects()
    {
        return _battleEffects.ToArray();
    }

    /// <summary>
    /// Получить эффекты указанного типа, воздействующие на юнита.
    /// </summary>
    public IReadOnlyList<UnitBattleEffect> GetBattleEffects(UnitAttackType effectAttackType)
    {
        return _battleEffects.Where(be => be.AttackType == effectAttackType).ToArray();
    }

    /// <summary>
    /// Получить модификатор брони.
    /// </summary>
    public int GetArmorModifier()
    {
        var modifier = 0;

        if (TryGetBattleEffect(UnitAttackType.ReduceArmor, out var reduceArmor))
        {
            modifier -= reduceArmor.Power!.Value;
        }

        return modifier;
    }

    /// <summary>
    /// Получить список защит от типов атак.
    /// </summary>
    public IReadOnlyList<UnitAttackTypeProtection> GetUnitAttackTypeProtections()
    {
        return _battleEffects
            .Where(be => be.AttackType == UnitAttackType.GiveProtection)
            .SelectMany(be => be.AttackTypeProtections)
            .ToArray();
    }

    /// <summary>
    /// Получить список защит от типов атак.
    /// </summary>
    public IReadOnlyList<UnitAttackSourceProtection> GetUnitAttackSourceProtections()
    {
        return _battleEffects
            .Where(be => be.AttackType == UnitAttackType.GiveProtection)
            .SelectMany(be => be.AttackSourceProtections)
            .ToArray();
    }

    /// <summary>
    /// Получить модификатор силы атаки.
    /// </summary>
    public decimal GetDamagePowerModifier()
    {
        var modifier = 1M;

        if (TryGetBattleEffect(UnitAttackType.IncreaseDamage, out var increaseDamage))
        {
            modifier *= increaseDamage.Power!.Value / 100M + 1;
        }

        if (TryGetBattleEffect(UnitAttackType.ReduceDamage, out var reduceDamage))
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

        if (TryGetBattleEffect(UnitAttackType.ReduceInitiative, out var reduceInitiative))
        {
            modifier *= 1 - reduceInitiative.Power!.Value / 100M;
        }

        return modifier - 1M;
    }
}