using Disciples.Resources.Database.Sqlite.Components;

namespace Disciples.Resources.Database.Sqlite.Models;

/// <summary>
/// Данные о том, как растут характеристики юнита с повышением уровня.
/// </summary>
public class UnitLevelUpgrade : IEntity
{
    /// <summary>
    /// Идентификатор.
    /// </summary>
    public string Id { get; init; } = null!;

    /// <summary>
    /// Рост стоимости юнита.
    /// </summary>
    public ResourceSet RecruitCost { get; init; } = null!;

    /// <summary>
    /// Рост количества жизней.
    /// </summary>
    public int HitPoints { get; init; }

    /// <summary>
    /// Рост базовой защиты юнита.
    /// </summary>
    public int Armor { get; init; }

    /// <summary>
    /// Рост базового восстановления % жизней за ход.
    /// </summary>
    public int HitPointsRegeneration { get; init; }

    /// <summary>
    /// Рост стоимости воскрешения юнита.
    /// </summary>
    public ResourceSet ReviveCost { get; init; } = null!;

    /// <summary>
    /// Рост стоимости восстановления 1 единицы здоровья.
    /// </summary>
    public ResourceSet HealCost { get; init; } = null!;

    /// <summary>
    /// Рост стоимости обучения 1 очка опыта юнита у инструктора.
    /// </summary>
    public ResourceSet TrainingCost { get; init; } = null!;

    /// <summary>
    /// Рост количества опыта за убийство юнита.
    /// </summary>
    public int XpKilled { get; init; }

    /// <summary>
    /// Рост количества опыта, необходимо для получения следующего уровня.
    /// </summary>
    public int XpNext { get; init; }

    /// <summary>
    /// Рост количества очков движения для юнита-героя.
    /// </summary>
    public int? LeaderMovePoints { get; init; }

    /// <summary>
    /// Рост снижения шанса успеха, когда вор применяет навык на отряд этого героя.
    /// </summary>
    public int? LeaderThiefProtection { get; init; }

    /// <summary>
    /// Рост наносимого урона.
    /// </summary>
    public int DamagePower { get; init; }

    /// <summary>
    /// Рост силы исцеления.
    /// </summary>
    public int HealPower { get; init; }

    /// <summary>
    /// Рост инициативы.
    /// </summary>
    public int Initiative { get; init; }

    /// <summary>
    /// Рост точности.
    /// </summary>
    public int Accuracy { get; init; }
}