namespace Disciples.Engine.Common.Enums.Units;

/// <summary>
/// Тип модификатора характеристики или способности юнита.
/// </summary>
public enum UnitModifierItemType
{
    /// <summary>
    /// TODO Модификатор количества целей?
    /// </summary>
    QtyTarget = 0,

    /// <summary>
    /// Дальность обзора лидера.
    /// </summary>
    ScoutPoints = 1,

    /// <summary>
    /// Лидерство.
    /// </summary>
    Leadership = 2,

    /// <summary>
    /// Сила атаки.
    /// </summary>
    Power = 3,

    /// <summary>
    /// Сила урона.
    /// </summary>
    /// <remarks>
    /// TODO В чем отличие от Power?
    /// </remarks>
    Damage = 4,

    /// <summary>
    /// Броня.
    /// </summary>
    Armor = 5,

    /// <summary>
    /// Очки здоровья.
    /// </summary>
    HitPoints = 6,

    /// <summary>
    /// Очки движения лидера.
    /// </summary>
    MovePoints = 7,

    /// <summary>
    /// Инициатива.
    /// </summary>
    Initiative = 9,

    /// <summary>
    /// Проходимость по определённому типу ландшафта (леса, вода и т.д.)
    /// </summary>
    MoveGround = 10,

    /// <summary>
    /// Добавляет новую способность лидеру.
    /// </summary>
    LeaderAbility = 11,

    /// <summary>
    /// Защита/иммунитет от источника атаки.
    /// </summary>
    AttackSourceProtection = 12,

    /// <summary>
    /// Регенерация здоровья каждый ход на глобальной карте.
    /// </summary>
    HitPointsRegeneration = 13,

    /// <summary>
    /// Защита/иммунитет от типа атаки.
    /// </summary>
    AttackTypeProtection = 14,

    /// <summary>
    /// Меняет тип атаки на <see cref="UnitAttackType.DrainLife" />.
    /// </summary>
    DrainLifeAttack = 15,

    /// <summary>
    /// Позволяет мгновенно отступать с поля боя.
    /// </summary>
    FastRetreat = 16,

    /// <summary>
    /// Уменьшение стоимости покупки в магазинах.
    /// </summary>
    ReduceCost = 17
}