using System.ComponentModel.DataAnnotations.Schema;
using Disciples.Resources.Database.Components;
using Disciples.Resources.Database.Enums;

namespace Disciples.Resources.Database.Models;

/// <summary>
/// Информация о типе юнита.
/// </summary>
[Table("Gunits")]
public class UnitType : IEntity
{
    /// <summary>
    /// Идентификатор типа юнита.
    /// </summary>
    [Column("UNIT_ID")]
    public string Id { get; init; } = null!;

    /// <summary>
    /// Категория юнита.
    /// </summary>
    [Column("UNIT_CAT")]
    public UnitCategory UnitCategory { get; init; }

    /// <summary>
    /// Базовый уровень юнита.
    /// </summary>
    [Column("LEVEL")]
    public int Level { get; init; }

    /// <summary>
    /// Предыдущий тип юнита в иерархии.
    /// </summary>
    [Column("PREV_ID")]
    public string? PreviousUnitTypeId { get; init; }

    /// <summary>
    /// Раса юнита.
    /// </summary>
    [Column("RACE_ID")]
    public string RaceId { get; init; } = null!;

    /// <summary>
    /// Подраса юнита.
    /// </summary>
    [Column("SUBRACE")]
    public Subrace Subrace { get; init; }

    /// <summary>
    /// Ветвь юнита.
    /// </summary>
    [Column("BRANCH")]
    public UnitBranch Branch { get; init; }

    /// <summary>
    /// Занимает ли юнит одну клетку. Занимает две клетки, если <see langword="false" />.
    /// </summary>
    [Column("SIZE_SMALL")]
    public bool IsSmall { get; init; }

    /// <summary>
    /// Является ли юнит мужчиной.
    /// </summary>
    [Column("SEX_M")]
    public bool IsMale { get; init; }

    /// <summary>
    /// Стоимость найма юнита.
    /// </summary>
    [Column("ENROLL_C")]
    public ResourceCost RecruitCost { get; init; } = null!;

    /// <summary>
    /// Здание, которое нужно построить в столице, чтобы нанимать юнита.
    /// </summary>
    /// <remarks>
    /// Используется для специальных расовых юнитов, типа "Титан" у Империи и "Оборотень" у Нежити.
    /// </remarks>
    [Column("ENROLL_B")]
    public string? RecruitBuildingId { get; init; }

    /// <summary>
    /// Имя типа юнита.
    /// </summary>
    [Column("NAME_TXT")]
    public string NameTextId { get; init; } = null!;

    /// <summary>
    /// Описание типа юнита.
    /// </summary>
    [Column("DESC_TXT")]
    public string DescriptionTextId { get; init; } = null!;

    /// <summary>
    /// Описание основной способности героя.
    /// </summary>
    /// <remarks>
    /// Например: "Воин", "Разведчик", "Маг", "Жезл власти", "Вор", "Ничего".
    /// </remarks>
    [Column("ABIL_TXT")]
    public string AbilityTextId { get; init; } = null!;

    /// <summary>
    /// Основная атака.
    /// </summary>
    [Column("ATTACK_ID")]
    public string MainUserAttackId { get; init; } = null!;

    /// <summary>
    /// Дополнительная атака.
    /// </summary>
    [Column("ATTACK2_ID")]
    public string? SecondaryUserAttackId { get; init; }

    /// <summary>
    /// Атакует ли юнит дважды.
    /// </summary>
    [Column("ATCK_TWICE")]
    public bool IsAttackTwice { get; init; }

    /// <summary>
    /// Количество жизней.
    /// </summary>
    [Column("HIT_POINT")]
    public int HitPoints { get; init; }

    /// <summary>
    /// Базовый тип юнита для юнита-героя.
    /// </summary>
    [Column("BASE_UNIT")]
    public string? LeaderBaseUnitId { get; init; }

    /// <summary>
    /// Базовая защита юнита.
    /// </summary>
    [Column("ARMOR")]
    public int Armor { get; init; }

    /// <summary>
    /// Базовое восстановления % жизней за ход.
    /// </summary>
    [Column("REGEN")]
    public int HitPointsRegeneration { get; init; }

    /// <summary>
    /// Стоимость воскрешения юнита.
    /// </summary>
    [Column("REVIVE_C")]
    public ResourceCost ReviveCost { get; init; } = null!;

    /// <summary>
    /// Стоимость восстановления 1 единицы здоровья.
    /// </summary>
    [Column("HEAL_C")]
    public ResourceCost HealCost { get; init; } = null!;

    /// <summary>
    /// Стоимость обучения 1 очка опыта юнита у инструктора.
    /// </summary>
    [Column("TRAINING_C")]
    public ResourceCost TrainingCost { get; init; } = null!;

    /// <summary>
    /// Количество опыта за убийство юнита.
    /// </summary>
    [Column("XP_KILLED")]
    public int XpKilled { get; init; }

    /// <summary>
    /// Здание, которое позволяет тренировать данный тип юнита.
    /// </summary>
    [Column("UPGRADE_B")]
    public string? UpgradeBuildingId { get; init; }

    /// <summary>
    /// Количество опыта, необходимо для получения следующего уровня.
    /// </summary>
    [Column("XP_NEXT")]
    public int XpNext { get; init; }

    /// <summary>
    /// Количество очков движения для юнита-героя.
    /// </summary>
    [Column("MOVE")]
    public int? LeaderMovePoints { get; init; }

    /// <summary>
    /// Дальность обзора для юнита-героя.
    /// </summary>
    [Column("SCOUT")]
    public int? LeaderScoutPoints { get; init; }

    /// <summary>
    /// Количество ходов, которое живёт юнит.
    /// </summary>
    /// <remarks>
    /// Актуально для героев призыва (они живут один ход) и иллюзий (они живут три хода).
    /// </remarks>
    [Column("LIFE_TIME")]
    public int? LeaderLifeTime { get; init; }

    /// <summary>
    /// Лидерство героя.
    /// </summary>
    /// <remarks>
    /// Количество юнитов считается вместе с самим героем (т.е. 4 - это герой + 3 маленьких юнита).
    /// </remarks>
    [Column("LEADERSHIP")]
    public int? Leadership { get; init; }

    /// <summary>
    /// TODO Умение вести переговоры? Что-то для воров?
    /// </summary>
    [Column("NEGOTIATE")]
    public int? LeaderNegotiate { get; init; }

    /// <summary>
    /// Категория лидера.
    /// TODO Разобраться, обернуть в Enum.
    /// </summary>
    [Column("LEADER_CAT")]
    public int? LeaderCategory { get; init; }

    /// <summary>
    /// Идентификатор записи, которая указывает рост характеристик юнита при повышении уровня.
    /// Используется для расчета уровня меньше или равным <see cref="UpgradeChangeLevel" />.
    /// </summary>
    [Column("DYN_UPG1")]
    public string LowLevelUpgradeId { get; init; } = null!;

    /// <summary>
    /// Последний уровень, когда рост характеристик рассчитывается по формуле <see cref="LowLevelUpgradeId" />,
    /// А потом переходит к <see cref="HighLevelUpgradeId" />.
    /// </summary>
    [Column("DYN_UPG_LV")]
    public int UpgradeChangeLevel { get; init; }

    /// <summary>
    /// Идентификатор записи, которая указывает рост характеристик юнита при повышении уровня.
    /// Используется для расчета уровня выше <see cref="UpgradeChangeLevel" />.
    /// </summary>
    [Column("DYN_UPG2")]
    public string HighLevelUpgradeId { get; init; } = null!;

    /// <summary>
    /// Признак, что юнит перемещается только по воде.
    /// </summary>
    /// <remarks>
    /// Актуально только для юнитов-героев.
    /// Морские юниты могут перемещаться по суше в составе сухопутного героя.
    /// </remarks>
    [Column("WATER_ONLY")]
    public bool IsLeaderWaterOnly { get; init; }

    /// <summary>
    /// Анимация, которая отображается при смерти юнита.
    /// TODO Enum?
    /// </summary>
    [Column("DEATH_ANIM")]
    public int DeathAnimation { get; init; }
}