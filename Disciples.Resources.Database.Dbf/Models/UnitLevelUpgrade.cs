using System.ComponentModel.DataAnnotations.Schema;
using Disciples.Resources.Database.Dbf.Components;

namespace Disciples.Resources.Database.Dbf.Models;

/// <summary>
/// Данные о том, как растут характеристики юнита с повышением уровня.
/// </summary>
[Table("GDynUpgr")]
public class UnitLevelUpgrade : IEntity
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    [Column("UPGRADE_ID")]
    public string Id { get; init; } = null!;

    /// <summary>
    /// Рост стоимости юнита.
    /// </summary>
    [Column("ENROLL_C")]
    public ResourceSet RecruitCost { get; init; } = null!;

    /// <summary>
    /// Рост количества жизней.
    /// </summary>
    [Column("HIT_POINT")]
    public int HitPoints { get; init; }

    /// <summary>
    /// Рост базовой защиты юнита.
    /// </summary>
    [Column("ARMOR")]
    public int Armor { get; init; }

    /// <summary>
    /// Рост базового восстановления % жизней за ход.
    /// </summary>
    [Column("REGEN")]
    public int HitPointsRegeneration { get; init; }

    /// <summary>
    /// Рост стоимости воскрешения юнита.
    /// </summary>
    [Column("REVIVE_C")]
    public ResourceSet ReviveCost { get; init; } = null!;

    /// <summary>
    /// Рост стоимости восстановления 1 единицы здоровья.
    /// </summary>
    [Column("HEAL_C")]
    public ResourceSet HealCost { get; init; } = null!;

    /// <summary>
    /// Рост стоимости обучения 1 очка опыта юнита у инструктора.
    /// </summary>
    [Column("TRAINING_C")]
    public ResourceSet TrainingCost { get; init; } = null!;

    /// <summary>
    /// Рост количества опыта за убийство юнита.
    /// </summary>
    [Column("XP_KILLED")]
    public int XpKilled { get; init; }

    /// <summary>
    /// Рост количества опыта, необходимо для получения следующего уровня.
    /// </summary>
    [Column("XP_NEXT")]
    public int XpNext { get; init; }

    /// <summary>
    /// Рост количества очков движения для юнита-героя.
    /// </summary>
    [Column("MOVE")]
    public int? LeaderMovePoints { get; init; }

    /// <summary>
    /// TODO Умение вести переговоры? Что-то для воров?
    /// </summary>
    [Column("NEGOTIATE")]
    public int? LeaderNegotiate { get; init; }

    /// <summary>
    /// Рост наносимого урона.
    /// </summary>
    [Column("DAMAGE")]
    public int DamagePower { get; init; }

    /// <summary>
    /// Рост силы исцеления.
    /// </summary>
    [Column("HEAL")]
    public int HealPower { get; init; }

    /// <summary>
    /// Рост инициативы.
    /// </summary>
    [Column("INITIATIVE")]
    public int Initiative { get; init; }

    /// <summary>
    /// Рост точности.
    /// </summary>
    [Column("POWER")]
    public int Accuracy { get; init; }
}