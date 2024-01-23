using System;
using System.Collections.Generic;
using System.Linq;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Enums.Units;

namespace Disciples.Engine.Common.Models;

/// <summary>
/// Информация о конкретном юните.
/// </summary>
public class Unit
{
    /// <summary>
    /// Создать объект типа <see cref="Unit" />.
    /// </summary>
    public Unit(string id, UnitType unitType, Player player, UnitSquadLinePosition squadLinePosition, UnitSquadFlankPosition squadFlankPosition)
    {
        Id = id;
        IsLeader = unitType.UnitCategory is UnitCategory.Leader or UnitCategory.LeaderThief;
        UnitType = unitType;
        Player = player;

        SquadLinePosition = squadLinePosition;
        SquadFlankPosition = squadFlankPosition;

        Level = UnitType.Level;
        Experience = 0;
        HitPoints = UnitType.HitPoints;
        Effects = new UnitEffects();

        AttackSourceProtections = unitType.AttackSourceProtections.ToList();
        AttackTypeProtections = unitType.AttackTypeProtections.ToList();
    }

    /// <summary>
    /// Уникальный идентификатор юнита.
    /// </summary>
    public string Id { get; }

    /// <summary>
    /// Является ли юнит лидером.
    /// </summary>
    public bool IsLeader { get; }

    /// <summary>
    /// Тип юнита.
    /// </summary>
    public UnitType UnitType { get; }

    /// <summary>
    /// Игрок, который управляет юнитом.
    /// </summary>
    public Player Player { get; }


    /// <summary>
    /// На какой линии располагается юнит в отряде.
    /// </summary>
    public UnitSquadLinePosition SquadLinePosition { get; set; }

    /// <summary>
    /// На какой позиции находится юнит в отряде.
    /// </summary>
    public UnitSquadFlankPosition SquadFlankPosition { get; set; }


    /// <summary>
    /// Имя юнита.
    /// todo Герои могут иметь собственные имена.
    /// </summary>
    public string Name => UnitType.Name;

    /// <summary>
    /// Уровень юнита.
    /// </summary>
    public int Level { get; set; }

    /// <summary>
    /// Накопленный за уровень опыт.
    /// </summary>
    public int Experience { get; set; }

    /// <summary>
    /// Количество оставшихся очков здоровья.
    /// </summary>
    public int HitPoints { get; set; }

    /// <summary>
    /// Максимальное количество очков здоровья.
    /// TODO Также зависит от эликсиров.
    /// </summary>
    public int MaxHitPoints => UnitType.HitPoints + CalculateLevelUpgrade(ulu => ulu.HitPoints);

    /// <summary>
    /// Базовая броня юнита.
    /// </summary>
    public int BaseArmor => UnitType.Armor + CalculateLevelUpgrade(ulu => ulu.Armor);

    /// <summary>
    /// Модификатор брони.
    /// </summary>
    public int ArmorModifier => Effects.GetArmorModifier();

    /// <summary>
    /// Текущая броня юнита.
    /// </summary>
    public int Armor => BaseArmor + ArmorModifier;

    /// <summary>
    /// Базовое значение силы первой атаки.
    /// </summary>
    public int MainAttackBasePower => GetAttackBasePower(UnitType.MainAttack);

    /// <summary>
    /// Модификатор значения силы первой атаки.
    /// </summary>
    public int MainAttackPowerModifier => UnitType.MainAttack.AttackType switch
    {
        UnitAttackType.Heal => 0,
        UnitAttackType.BoostDamage => 0,
        _ => (int)(MainAttackBasePower * Effects.GetDamagePowerModifier())
    };

    /// <summary>
    /// Текущее значение силы первой атаки.
    /// </summary>
    public int MainAttackPower => MainAttackBasePower + MainAttackPowerModifier;

    /// <summary>
    /// Базовое значение силы второй атаки.
    /// </summary>
    /// <remarks>
    /// На вторую атаку модификаторы не распространяются.
    /// </remarks>
    public int? SecondaryAttackPower => UnitType.SecondaryAttack != null
        ? GetAttackBasePower(UnitType.SecondaryAttack)
        : null;

    /// <summary>
    /// Базовое значение точности первой атаки.
    /// </summary>
    public int MainAttackBaseAccuracy => UnitType.MainAttack.Accuracy + CalculateLevelUpgrade(ulu => ulu.Accuracy);

    /// <summary>
    /// Модификатор точности первой атаки.
    /// </summary>
    public int MainAttackAccuracyModifier => UnitType.MainAttack.AttackType switch
    {
        UnitAttackType.Heal => 0,
        UnitAttackType.BoostDamage => 0,
        _ => (int) (MainAttackBaseAccuracy * Effects.GetAccuracyModifier())
    };

    /// <summary>
    /// Текущее значение точность первой атаки.
    /// </summary>
    public int MainAttackAccuracy => MainAttackBaseAccuracy + MainAttackAccuracyModifier;

    /// <summary>
    /// Значение точности второй атаки.
    /// </summary>
    /// <remarks>
    /// На вторую атаку модификаторы не распространяются.
    /// </remarks>
    public int? SecondaryAttackAccuracy => UnitType.SecondaryAttack?.Accuracy + CalculateLevelUpgrade(ulu => ulu.Accuracy);

    /// <summary>
    /// Базовая инициатива.
    /// </summary>
    public int BaseInitiative => UnitType.MainAttack.Initiative + CalculateLevelUpgrade(ulu => ulu.Initiative);

    /// <summary>
    /// Модификатор инициативы.
    /// </summary>
    public int InitiativeModifier => (int) (BaseInitiative * Effects.GetInitiativeModifier());

    /// <summary>
    /// Текущее значение инициативы.
    /// </summary>
    public int Initiative => BaseInitiative + InitiativeModifier;

    /// <summary>
    /// Признак, что юнита не нужно учитывать на поле боя.
    /// </summary>
    public bool IsDeadOrRetreated => IsDead || IsRetreated;

    /// <summary>
    /// Мёртв ли юнит.
    /// </summary>
    public bool IsDead { get; set; }

    /// <summary>
    /// Юнит сбежал.
    /// </summary>
    public bool IsRetreated { get; set; }

    /// <summary>
    /// Признак, что в этом бою юнит был воскрешен способностью <see cref="UnitAttackType.Revive" />.
    /// </summary>
    public bool IsRevived { get; set; }

    /// <summary>
    /// Эффекты, воздействующие на юнита.
    /// </summary>
    public UnitEffects Effects { get; }

    /// <summary>
    /// Защита от источников атак.
    /// </summary>
    public List<UnitAttackSourceProtection> AttackSourceProtections { get; init; }

    /// <summary>
    /// Защита от типов атак.
    /// </summary>
    public List<UnitAttackTypeProtection> AttackTypeProtections { get; init; }

    /// <summary>
    /// Получить силу атаки.
    /// </summary>
    private int GetAttackBasePower(UnitAttack attack)
    {
        return attack.AttackType switch
        {
            UnitAttackType.Heal => attack.HealPower + CalculateLevelUpgrade(ulu => ulu.HealPower),
            UnitAttackType.BoostDamage => GetBoostDamagePercent(attack.AttackPowerLevel),
            UnitAttackType.ReduceDamage => GetReduceDamagePercent(attack.AttackPowerLevel),
            UnitAttackType.ReduceInitiative => GetReduceInitiativePercent(attack.AttackPowerLevel),
            _ => attack.DamagePower + CalculateLevelUpgrade(ulu => ulu.DamagePower)
        };
    }

    /// <summary>
    /// Рассчитать повышение значения в зависимости от уровня юнита.
    /// </summary>
    private int CalculateLevelUpgrade(Func<UnitLevelUpgrade, int> propertyGetter)
    {
        var levelDiff = Level - UnitType.Level;
        var lowLevelDiff = Math.Min(levelDiff, UnitType.UpgradeChangeLevel - UnitType.Level);
        var highLevelDiff = Math.Max(0, levelDiff - lowLevelDiff);

        return
            propertyGetter.Invoke(UnitType.LowLevelUpgrade) * lowLevelDiff
            +
            propertyGetter.Invoke(UnitType.HighLevelUpgrade) * highLevelDiff;
    }

    /// <summary>
    /// Получить усиления атаки в процентах.
    /// </summary>
    /// <remarks>
    /// TODO Тащить эти значения из GVars, BATBOOSTD1/2/3/4.
    /// </remarks>
    private static int GetBoostDamagePercent(int boostPowerLevel)
    {
        return boostPowerLevel switch
        {
            1 => 25,
            2 => 50,
            3 => 75,
            4 => 100,
            _ => 0
        };
    }

    /// <summary>
    /// Получить ослабление атаки в процентах.
    /// </summary>
    /// <remarks>
    /// TODO Тащить эти значения из GVars, BATLOWERD1/2.
    /// </remarks>
    private static int GetReduceDamagePercent(int reduceDamageLevel)
    {
        return reduceDamageLevel switch
        {
            1 => 50,
            2 => 33,
            _ => 0
        };
    }

    /// <summary>
    /// Получить уменьшение инициативы в процентах.
    /// </summary>
    /// <remarks>
    /// TODO Тащить эти значения из GVars, BATLOWERI1.
    /// </remarks>
    private static int GetReduceInitiativePercent(int reduceInitiativeLevel)
    {
        return reduceInitiativeLevel switch
        {
            1 => 50,
            _ => 0
        };
    }
}