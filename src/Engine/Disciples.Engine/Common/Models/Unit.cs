using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper.Internal;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Enums.Units;

namespace Disciples.Engine.Common.Models;

/// <summary>
/// Информация о конкретном юните.
/// </summary>
public class Unit
{
    /// <summary>
    /// Создать юнита, который на уровень выше, чем предыдущий.
    /// </summary>
    public static Unit CreateNextLevelUnit(Unit oldUnit)
    {
        var newUnit = new Unit(oldUnit.Id, oldUnit.UnitType, oldUnit.Player, oldUnit.SquadLinePosition, oldUnit.SquadFlankPosition);
        newUnit.Level = oldUnit.Level + 1;
        newUnit.HitPoints = newUnit.MaxHitPoints;

        return newUnit;
    }

    /// <summary>
    /// Создать объект типа <see cref="Unit" />.
    /// </summary>
    public Unit(
        string id,
        UnitType unitType,
        Player player,
        UnitSquadLinePosition squadLinePosition,
        UnitSquadFlankPosition squadFlankPosition)
    {
        Id = id;
        IsLeader = unitType.UnitCategory is UnitCategory.Leader or UnitCategory.NeutralLeader or UnitCategory.LeaderThief;
        UnitType = unitType;
        Player = player;

        SquadLinePosition = squadLinePosition;
        SquadFlankPosition = squadFlankPosition;

        Level = UnitType.Level;
        Experience = 0;
        HitPoints = UnitType.HitPoints;
        Effects = new UnitEffects();

        BaseAttackTypeProtections = unitType.AttackTypeProtections.ToList();
        BaseAttackSourceProtections = unitType.AttackSourceProtections.ToList();
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
    /// Необходимый опыт для того, чтобы перейти на следующий уровень.
    /// </summary>
    public int NextLevelExperience => UnitType.XpNext + CalculateLevelUpgrade(ulu => ulu.XpNext);

    /// <summary>
    /// Количество опыта, которое получит вражеский отряд после смерти юнита.
    /// </summary>
    public virtual int DeathExperience => UnitType.XpKilled + CalculateLevelUpgrade(ulu => ulu.XpKilled);

    /// <summary>
    /// Накопленный опыт во время битвы.
    /// </summary>
    public virtual int BattleExperience { get; set; }

    /// <summary>
    /// Количество оставшихся очков здоровья.
    /// </summary>
    public int HitPoints { get; set; }

    /// <summary>
    /// Максимальное количество очков здоровья.
    /// TODO Также зависит от эликсиров.
    /// </summary>
    public virtual int MaxHitPoints => UnitType.HitPoints + CalculateLevelUpgrade(ulu => ulu.HitPoints);

    /// <summary>
    /// Базовая броня юнита.
    /// </summary>
    public int BaseArmor => UnitType.Armor + CalculateLevelUpgrade(ulu => ulu.Armor);

    /// <summary>
    /// Модификатор брони.
    /// </summary>
    public int ArmorModifier => Effects.GetArmorBonus();

    /// <summary>
    /// Текущая броня юнита.
    /// </summary>
    public int Armor => BaseArmor + ArmorModifier;

    /// <summary>
    /// Основная атака юнита.
    /// </summary>
    public CalculatedUnitAttack MainAttack => CalculateUnitAttack(UnitType.MainAttack, true);

    /// <summary>
    /// Альтернативная основная атака юнита.
    /// </summary>
    public CalculatedUnitAttack? AlternativeAttack => UnitType.MainAttack.AlternativeAttack == null
        ? null
        : CalculateUnitAttack(UnitType.MainAttack.AlternativeAttack, true);

    /// <summary>
    /// Вторая атака юнита.
    /// </summary>
    public CalculatedUnitAttack? SecondaryAttack => UnitType.SecondaryAttack == null
        ? null
        : CalculateUnitAttack(UnitType.SecondaryAttack, false);

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
    public virtual bool IsDead { get; set; }

    /// <summary>
    /// Юнит сбежал.
    /// </summary>
    public virtual bool IsRetreated { get; set; }

    /// <summary>
    /// Признак, что в этом бою юнит был воскрешен способностью <see cref="UnitAttackType.Revive" />.
    /// </summary>
    public bool IsRevived { get; set; }

    /// <summary>
    /// Эффекты, воздействующие на юнита.
    /// </summary>
    public virtual UnitEffects Effects { get; }

    /// <summary>
    /// Базовая для типа юнита защита от типов атак.
    /// </summary>
    public List<UnitAttackTypeProtection> BaseAttackTypeProtections { get; }

    /// <summary>
    /// Защита от типов атак.
    /// </summary>
    public IReadOnlyList<UnitAttackTypeProtection> AttackTypeProtections => BaseAttackTypeProtections
            .Concat(Effects.GetUnitAttackTypeProtections())
            .ToArray();

    /// <summary>
    /// Базовая для типа юнита защита от источников атак.
    /// </summary>
    public List<UnitAttackSourceProtection> BaseAttackSourceProtections { get; }

    /// <summary>
    /// Защита от источников атак.
    /// </summary>
    public IReadOnlyList<UnitAttackSourceProtection> AttackSourceProtections => BaseAttackSourceProtections
        .Concat(Effects.GetUnitAttackSourceProtections())
        .ToArray();

    /// <summary>
    /// Вычислить атаку юнита.
    /// </summary>
    private CalculatedUnitAttack CalculateUnitAttack(UnitAttack unitAttack, bool shouldUseModifiers)
    {
        return new CalculatedUnitAttack(
            GetAttackBasePower(unitAttack),
            shouldUseModifiers ? GetAttackPowerModifier(unitAttack) : 0,
            unitAttack.Accuracy + CalculateLevelUpgrade(ulu => ulu.Accuracy),
            shouldUseModifiers ? GetAttackAccuracyModifier(unitAttack) : 0,
            unitAttack);
    }

    /// <summary>
    /// Получить силу атаки.
    /// </summary>
    private int GetAttackBasePower(UnitAttack attack)
    {
        return attack.AttackType switch
        {
            UnitAttackType.Heal => attack.HealPower + CalculateLevelUpgrade(ulu => ulu.HealPower),
            UnitAttackType.IncreaseDamage => GetBoostDamagePercent(attack.AttackPowerLevel),
            UnitAttackType.ReduceDamage => GetReduceDamagePercent(attack.AttackPowerLevel),
            UnitAttackType.ReduceInitiative => GetReduceInitiativePercent(attack.AttackPowerLevel),
            _ => attack.DamagePower + CalculateLevelUpgrade(ulu => ulu.DamagePower)
        };
    }

    /// <summary>
    /// Получить модификатор силы атаки.
    /// </summary>
    private decimal GetAttackPowerModifier(UnitAttack attack)
    {
        switch (attack.AttackType)
        {
            case UnitAttackType.Damage:
            case UnitAttackType.DrainLife:
            case UnitAttackType.Poison:
            case UnitAttackType.Frostbite:
            case UnitAttackType.DrainLifeOverflow:
            case UnitAttackType.Blister:
                return Effects.GetDamagePowerModifier();

            case UnitAttackType.Paralyze:
            case UnitAttackType.Heal:
            case UnitAttackType.Fear:
            case UnitAttackType.IncreaseDamage:
            case UnitAttackType.Petrify:
            case UnitAttackType.ReduceDamage:
            case UnitAttackType.ReduceInitiative:
            case UnitAttackType.Revive:
            case UnitAttackType.Cure:
            case UnitAttackType.Summon:
            case UnitAttackType.ReduceLevel:
            case UnitAttackType.GiveAdditionalAttack:
            case UnitAttackType.Doppelganger:
            case UnitAttackType.TransformSelf:
            case UnitAttackType.TransformEnemy:
            case UnitAttackType.GiveProtection:
            case UnitAttackType.ReduceArmor:
            default:
                return 0;
        }
    }

    /// <summary>
    /// Получить модификатор точности.
    /// </summary>
    private decimal GetAttackAccuracyModifier(UnitAttack attack)
    {
        switch (attack.AttackType)
        {
            case UnitAttackType.Damage:
            case UnitAttackType.DrainLife:
            case UnitAttackType.Paralyze:
            case UnitAttackType.Fear:
            case UnitAttackType.Petrify:
            case UnitAttackType.ReduceDamage:
            case UnitAttackType.ReduceInitiative:
            case UnitAttackType.Poison:
            case UnitAttackType.Frostbite:
            case UnitAttackType.DrainLifeOverflow:
            case UnitAttackType.ReduceLevel:
            case UnitAttackType.TransformEnemy:
            case UnitAttackType.Blister:
            case UnitAttackType.ReduceArmor:
                return Effects.GetAccuracyModifier();

            case UnitAttackType.Heal:
            case UnitAttackType.IncreaseDamage:
            case UnitAttackType.Revive:
            case UnitAttackType.Cure:
            case UnitAttackType.Summon:
            case UnitAttackType.GiveAdditionalAttack:
            case UnitAttackType.Doppelganger:
            case UnitAttackType.TransformSelf:
            case UnitAttackType.GiveProtection:
            default:
                return 0;
        }
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