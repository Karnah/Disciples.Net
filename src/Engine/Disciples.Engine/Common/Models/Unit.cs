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
    /// todo Рассчитывать, зависит от эффектов.
    /// </summary>
    public int ArmorModifier => 0;

    /// <summary>
    /// Текущая броня юнита.
    /// </summary>
    public int Armor => BaseArmor + ArmorModifier;

    /// <summary>
    /// Базовое значение силы первой атаки.
    /// </summary>
    public int BaseFirstAttackPower => UnitType.MainAttack.HealPower > 0
        ? UnitType.MainAttack.HealPower + CalculateLevelUpgrade(ulu => ulu.HealPower)
        : UnitType.MainAttack.DamagePower + CalculateLevelUpgrade(ulu => ulu.DamagePower);

    /// <summary>
    /// Модификатор значения силы первой атаки.
    /// todo Рассчитывать, зависит от эффектов.
    /// </summary>
    public int FirstAttackPowerModifier => 0;

    /// <summary>
    /// Текущее значение силы первой атаки.
    /// </summary>
    public int FirstAttackPower => BaseFirstAttackPower + FirstAttackPowerModifier;

    /// <summary>
    /// Базовое значение силы второй атаки.
    /// </summary>
    /// <remarks>
    /// На вторую атаку модификаторы не распространяются.
    /// </remarks>
    public int? SecondAttackPower => UnitType.SecondaryAttack?.HealPower > 0
        ? UnitType.SecondaryAttack?.HealPower + CalculateLevelUpgrade(ulu => ulu.HealPower)
        : UnitType.SecondaryAttack?.DamagePower + CalculateLevelUpgrade(ulu => ulu.DamagePower);

    /// <summary>
    /// Базовое значение точности первой атаки.
    /// </summary>
    public int BaseFirstAttackAccuracy => UnitType.MainAttack.Accuracy + CalculateLevelUpgrade(ulu => ulu.Accuracy);

    /// <summary>
    /// Модификатор точности первой атаки.
    /// todo Рассчитывать, зависит эффектов.
    /// </summary>
    public int FirstAttackAccuracyModifier => 0;

    /// <summary>
    /// Текущее значение точность первой атаки.
    /// </summary>
    public int MainAttackAccuracy => BaseFirstAttackAccuracy + FirstAttackAccuracyModifier;

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
    /// todo Рассчитывать, зависит от эффектов.
    /// </summary>
    public int InitiativeModifier => 0;

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
}