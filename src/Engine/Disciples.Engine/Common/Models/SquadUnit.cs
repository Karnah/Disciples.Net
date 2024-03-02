using Disciples.Engine.Common.Enums;

namespace Disciples.Engine.Common.Models;

/// <summary>
/// Юнит в отряде.
/// </summary>
public class SquadUnit
{
    /// <summary>
    /// Идентификатор юнита.
    /// </summary>
    public string Id { get; init; } = null!;

    /// <summary>
    /// Имя юнита.
    /// </summary>
    /// <remarks>
    /// Не обязательное значение. <see langword="null" />, если используется стандартное имя юнита.
    /// </remarks>
    public string? Name { get; init; }

    /// <summary>
    /// Идентификатор типа юнита.
    /// </summary>
    public string UnitTypeId { get; set; } = null!;

    /// <summary>
    /// На какой линии располагается юнит в отряде.
    /// </summary>
    public UnitSquadLinePosition SquadLinePosition { get; set; }

    /// <summary>
    /// На какой позиции находится юнит в отряде.
    /// </summary>
    public UnitSquadFlankPosition SquadFlankPosition { get; set; }

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
    /// Признак, что юнит мёртв.
    /// </summary>
    public bool IsDead => HitPoints == 0;
}