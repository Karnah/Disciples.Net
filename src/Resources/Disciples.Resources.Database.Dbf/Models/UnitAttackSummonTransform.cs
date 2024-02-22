using System.ComponentModel.DataAnnotations.Schema;
using Disciples.Resources.Database.Dbf.Enums;

namespace Disciples.Resources.Database.Dbf.Models;

/// <summary>
/// Юнит, который вызывается / в кого превращается цель при атаке.
/// </summary>
/// <remarks>
/// Для атаки типа <see cref="UnitAttackType.Summon" />, идентификаторы вызываемых юнитов.
/// Для атак типа <see cref="UnitAttackType.TransformSelf" /> и <see cref="UnitAttackType.TransformEnemy" /> идентификаторы во что идёт превращение.
/// </remarks>>
[Table("Gtransf")]
public class UnitAttackSummonTransform : IEntity
{
    /// <summary>
    /// Идентификатор атаки юнита.
    /// </summary>
    /// <remarks>
    /// Идентификатор НЕ уникален.
    /// </remarks>
    [Column("ATTACK_ID")]
    public string Id { get; init; } = null!;

    /// <summary>
    /// Идентификатор вызываемого юнита / юнита,в которого превратится цель.
    /// </summary>
    [Column("TRANSF_ID")]
    public string UnitTypeId { get; init; } = null!;
}