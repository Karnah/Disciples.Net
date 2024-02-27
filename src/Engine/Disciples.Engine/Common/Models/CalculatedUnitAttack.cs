using System.Collections.Generic;
using Disciples.Engine.Common.Enums.Units;

namespace Disciples.Engine.Common.Models;

/// <summary>
/// Рассчитанная информации об атаке юнита с учётом эффектов.
/// </summary>
public class CalculatedUnitAttack
{
    /// <summary>
    /// Создать объект типа <see cref="CalculatedUnitAttack" />.
    /// </summary>
    public CalculatedUnitAttack(int basePower, decimal powerModifier, int baseAccuracy, decimal accuracyModifier, UnitAttack attack)
    {
        BasePower = basePower;
        PowerBonus = (int)(basePower * powerModifier);
        BaseAccuracy = baseAccuracy;
        AccuracyBonus = (int)(baseAccuracy * accuracyModifier);
        Attack = attack;
    }

    /// <summary>
    /// Наименование атаки.
    /// </summary>
    public string Name => Attack.Name;

    /// <summary>
    /// Описание атаки.
    /// </summary>
    public string Description => Attack.Description;

    /// <summary>
    /// Базовое значение силы атаки.
    /// </summary>
    public int BasePower { get; }

    /// <summary>
    /// Бонус к силе атаки.
    /// </summary>
    public int PowerBonus { get; }

    /// <summary>
    /// Итоговое значение силы атаки.
    /// </summary>
    public int TotalPower => BasePower + PowerBonus;

    /// <summary>
    /// Базовое значение точности атаки.
    /// </summary>
    public int BaseAccuracy { get; }

    /// <summary>
    /// Бонус к точности атаки.
    /// </summary>
    public int AccuracyBonus { get; }

    /// <summary>
    /// Итоговое значение точности первой атаки.
    /// </summary>
    public int TotalAccuracy => BaseAccuracy + AccuracyBonus;

    /// <summary>
    /// Информация об атаке.
    /// </summary>
    public UnitAttack Attack { get; }

    /// <summary>
    /// Тип атаки.
    /// </summary>
    public UnitAttackType AttackType => Attack.AttackType;

    /// <summary>
    /// Источник атаки.
    /// </summary>
    public UnitAttackSource AttackSource => Attack.AttackSource;

    /// <summary>
    /// Достижимость целей атаки.
    /// </summary>
    public UnitAttackReach Reach => Attack.Reach;

    /// <summary>
    /// Признак, что эффект, накладываемый атакой, длится до конца боя.
    /// </summary>
    public bool IsInfinitive => Attack.IsInfinitive;

    /// <summary>
    /// Наложение защиты от типов атак.
    /// </summary>
    public IReadOnlyList<UnitAttackTypeProtection> AttackTypeProtections => Attack.AttackTypeProtections;

    /// <summary>
    /// Наложение защиты от источников атак.
    /// </summary>
    public IReadOnlyList<UnitAttackSourceProtection> AttackSourceProtections => Attack.AttackSourceProtections;

    /// <summary>
    /// Признак, что при ударе наносится критический урон.
    /// </summary>
    /// <remarks>
    /// +5% от силы атаки.
    /// </remarks>
    public bool IsCritical => Attack.IsCritical;

    /// <summary>
    /// Призываемые / превращаемые типы юнитов.
    /// </summary>
    /// <remarks>
    /// Для атаки типа <see cref="UnitAttackType.Summon" />, идентификаторы вызываемых юнитов.
    /// Для атак типа <see cref="UnitAttackType.TransformSelf" /> и <see cref="UnitAttackType.TransformEnemy" /> идентификаторы во что идёт превращение.
    /// </remarks>>
    public IReadOnlyList<UnitType> SummonTransformUnitTypes => Attack.SummonTransformUnitTypes;
}