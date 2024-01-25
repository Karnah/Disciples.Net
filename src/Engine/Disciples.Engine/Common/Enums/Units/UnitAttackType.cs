namespace Disciples.Engine.Common.Enums.Units;

/// <summary>
/// Тип того, что делает атака юнита.
/// </summary>
public enum UnitAttackType
{
    /// <summary>
    /// Повреждение.
    /// </summary>
    Damage = 1,

    /// <summary>
    /// Выпить жизненную силу и вылечить себя.
    /// </summary>
    DrainLife = 2,

    /// <summary>
    /// Паралич.
    /// </summary>
    Paralyze = 3,

    /// <summary>
    /// Исцеление.
    /// </summary>
    Heal = 6,

    /// <summary>
    /// Страх.
    /// </summary>
    Fear = 7,

    /// <summary>
    /// Увеличение урона.
    /// </summary>
    BoostDamage = 8,

    /// <summary>
    /// Окаменение.
    /// </summary>
    Petrify = 9,

    /// <summary>
    /// Снижение повреждения.
    /// </summary>
    ReduceDamage = 10,

    /// <summary>
    /// Снижение инициативы.
    /// </summary>
    ReduceInitiative = 11,

    /// <summary>
    /// Отравление.
    /// </summary>
    Poison = 12,

    /// <summary>
    /// Обморожение.
    /// </summary>
    Frostbite = 13,

    /// <summary>
    /// Воскрешение.
    /// </summary>
    Revive = 14,

    /// <summary>
    /// Выпить жизненную силу и вылечить себя и/или союзников.
    /// </summary>
    DrainLifeOverflow = 15,

    /// <summary>
    /// Снятие негативных эффектов.
    /// </summary>
    Cure = 16,

    /// <summary>
    /// Призыв.
    /// </summary>
    Summon = 17,

    /// <summary>
    /// Понизить уровень.
    /// </summary>
    DrainLevel = 18,

    /// <summary>
    /// Дать дополнительную атаку.
    /// </summary>
    GiveAdditionalAttack = 19,

    /// <summary>
    /// Превратить себя в выбранного юнита на поле боя.
    /// </summary>
    Doppelganger = 20,

    /// <summary>
    /// Превратить себя.
    /// </summary>
    TransformSelf = 21,

    /// <summary>
    /// Превратить другого.
    /// </summary>
    TransformOther = 22,

    /// <summary>
    /// Ожог.
    /// </summary>
    Blister = 23,

    /// <summary>
    /// Даровать защиту от типов/источников атак.
    /// </summary>
    GiveProtection = 24,

    /// <summary>
    /// Разбить броню.
    /// </summary>
    ReduceArmor = 25
}