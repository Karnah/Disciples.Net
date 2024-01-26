using Disciples.Resources.Database.Sqlite.Enums;

namespace Disciples.Resources.Database.Sqlite.Models;

/// <summary>
/// Юнит, который вызывается / в кого превращается цель при атаке.
/// </summary>
/// <remarks>
/// Для атаки типа <see cref="UnitAttackType.Summon" />, идентификаторы вызываемых юнитов.
/// Для атак типа <see cref="UnitAttackType.TransformSelf" /> и <see cref="UnitAttackType.TransformOther" /> идентификаторы во что идёт превращение.
/// </remarks>>
public class UnitAttackSummonTransform : IEntity
{
    /// <summary>
    /// Идентификатор атаки юнита.
    /// </summary>
    /// <remarks>
    /// Идентификатор НЕ уникален.
    /// </remarks>
    public string Id { get; init; } = null!;

    /// <summary>
    /// Атака, которая вызывает/превращает юнита.
    /// </summary>
    public UnitAttack UnitAttack { get; init; } = null!;

    /// <summary>
    /// Идентификатор вызываемого юнита / юнита,в которого превратится цель.
    /// </summary>
    public string UnitTypeId { get; init; } = null!;

    /// <summary>
    /// Тип юнита, который вызывается / в кого превращается цель.
    /// </summary>
    public UnitType UnitType { get; init; } = null!;
}