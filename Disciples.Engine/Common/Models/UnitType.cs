using System.Collections.Generic;
using Disciples.Engine.Common.Enums.Units;

namespace Disciples.Engine.Common.Models;

/// <summary>
/// Тип юнита.
/// </summary>
public class UnitType
{
    /// <summary>
    /// Идентификатор типа юнита.
    /// </summary>
    public string Id { get; init; } = null!;

    /// <summary>
    /// Категория юнита.
    /// </summary>
    public UnitCategory UnitCategory { get; init; }

    /// <summary>
    /// Базовый уровень юнита.
    /// </summary>
    public int Level { get; init; }

    /// <summary>
    /// Предыдущий тип юнита в иерархии.
    /// </summary>
    public UnitType? PreviousUnitType { get; init; }

    /// <summary>
    /// Раса юнита.
    /// </summary>
    public string RaceId { get; init; } = null!;

    /// <summary>
    /// Подраса юнита.
    /// </summary>
    public Subrace Subrace { get; init; }

    /// <summary>
    /// Ветвь юнита.
    /// </summary>
    public UnitBranch Branch { get; init; }

    /// <summary>
    /// Занимает ли юнит одну клетку. Занимает две клетки, если <see langword="false" />.
    /// </summary>
    public bool IsSmall { get; init; }

    /// <summary>
    /// Является ли юнит мужчиной.
    /// </summary>
    public bool IsMale { get; init; }

    /// <summary>
    /// Стоимость найма юнита.
    /// </summary>
    public ResourceCost RecruitCost { get; init; } = null!;

    /// <summary>
    /// Здание, которое нужно построить в столице, чтобы нанимать юнита.
    /// </summary>
    /// <remarks>
    /// Используется для специальных расовых юнитов, типа "Титан" у Империи и "Оборотень" у Нежити.
    /// </remarks>
    public string? RecruitBuildingId { get; init; }

    /// <summary>
    /// Имя типа юнита.
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    /// Описание типа юнита.
    /// </summary>
    public string Description { get; init; } = null!;

    /// <summary>
    /// Описание основной способности героя.
    /// </summary>
    /// <remarks>
    /// Например: "Воин", "Разведчик", "Маг", "Жезл власти", "Вор", "Ничего".
    /// </remarks>
    public string Ability { get; init; } = null!;

    /// <summary>
    /// Основная атака.
    /// </summary>
    public UnitAttack MainAttack { get; init; } = null!;

    /// <summary>
    /// Дополнительная атака.
    /// </summary>
    public UnitAttack? SecondaryAttack { get; init; }

    /// <summary>
    /// Атакует ли юнит дважды.
    /// </summary>
    public bool IsAttackTwice { get; init; }

    /// <summary>
    /// Количество жизней.
    /// </summary>
    public int HitPoints { get; init; }

    /// <summary>
    /// Базовый тип юнита для юнита-героя.
    /// </summary>
    public UnitType? LeaderBaseUnit { get; init; }

    /// <summary>
    /// Базовая защита юнита.
    /// </summary>
    public int Armor { get; init; }

    /// <summary>
    /// Базовое восстановления % жизней за ход.
    /// </summary>
    public int HitPointsRegeneration { get; init; }

    /// <summary>
    /// Стоимость воскрешения юнита.
    /// </summary>
    public ResourceCost ReviveCost { get; init; } = null!;

    /// <summary>
    /// Стоимость восстановления 1 единицы здоровья.
    /// </summary>
    public ResourceCost HealCost { get; init; } = null!;

    /// <summary>
    /// Стоимость обучения 1 очка опыта юнита у инструктора.
    /// </summary>
    public ResourceCost TrainingCost { get; init; } = null!;

    /// <summary>
    /// Количество опыта за убийство юнита.
    /// </summary>
    public int XpKilled { get; init; }

    /// <summary>
    /// Здание, которое позволяет тренировать данный тип юнита.
    /// </summary>
    public string? UpgradeBuildingId { get; init; }

    /// <summary>
    /// Количество опыта, необходимо для получения следующего уровня.
    /// </summary>
    public int XpNext { get; init; }

    /// <summary>
    /// Количество очков движения для юнита-героя.
    /// </summary>
    public int? LeaderMovePoints { get; init; }

    /// <summary>
    /// Дальность обзора для юнита-героя.
    /// </summary>
    public int? LeaderScoutPoints { get; init; }

    /// <summary>
    /// Количество ходов, которое живёт юнит.
    /// </summary>
    /// <remarks>
    /// Актуально для героев призыва (они живут один ход) и иллюзий (они живут три хода).
    /// </remarks>
    public int? LeaderLifeTime { get; init; }

    /// <summary>
    /// Лидерство героя.
    /// </summary>
    /// <remarks>
    /// Количество юнитов считается вместе с самим героем (т.е. 4 - это герой + 3 маленьких юнита).
    /// </remarks>
    public int? Leadership { get; init; }

    /// <summary>
    /// TODO Умение вести переговоры? Что-то для воров?
    /// </summary>
    public int? LeaderNegotiate { get; init; }

    /// <summary>
    /// Категория лидера.
    /// TODO Разобраться, обернуть в Enum.
    /// </summary>
    public int? LeaderCategory { get; init; }

    /// <summary>
    /// Идентификатор записи, которая указывает рост характеристик юнита при повышении уровня.
    /// Используется для расчета уровня меньше или равным <see cref="UpgradeChangeLevel" />.
    /// </summary>
    public string LowLevelUpgradeId { get; init; } = null!;

    /// <summary>
    /// Последний уровень, когда рост характеристик рассчитывается по формуле <see cref="LowLevelUpgradeId" />,
    /// А потом переходит к <see cref="HighLevelUpgradeId" />.
    /// </summary>
    public int UpgradeChangeLevel { get; init; }

    /// <summary>
    /// Идентификатор записи, которая указывает рост характеристик юнита при повышении уровня.
    /// Используется для расчета уровня выше <see cref="UpgradeChangeLevel" />.
    /// </summary>
    public string HighLevelUpgradeId { get; init; } = null!;

    /// <summary>
    /// Признак, что юнит перемещается только по воде.
    /// </summary>
    /// <remarks>
    /// Актуально только для юнитов-героев.
    /// Морские юниты могут перемещаться по суше в составе сухопутного героя.
    /// </remarks>
    public bool IsLeaderWaterOnly { get; init; }

    /// <summary>
    /// Анимация, которая отображается при смерти юнита.
    /// TODO Enum?
    /// </summary>
    public int DeathAnimation { get; init; }

    /// <summary>
    /// Защита от источников атак.
    /// </summary>
    public IReadOnlyList<UnitAttackSourceProtection> AttackSourceProtections { get; init; } = null!;

    /// <summary>
    /// Защита от типов атак.
    /// </summary>
    public IReadOnlyList<UnitAttackTypeProtection> AttackTypeProtections { get; init; } = null!;
}