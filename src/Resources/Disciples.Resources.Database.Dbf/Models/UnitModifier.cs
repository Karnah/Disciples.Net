using System.ComponentModel.DataAnnotations.Schema;
using Disciples.Resources.Database.Dbf.Enums;

namespace Disciples.Resources.Database.Dbf.Models;

/// <summary>
/// Модификатор характеристик или способностей юнита.
/// </summary>

[Table("Gmodif")]
public class UnitModifier : IEntity
{
    /// <summary>
    /// Идентификатор модификатора.
    /// </summary>
    [Column("MODIF_ID")]
    public string Id { get; init; } = null!;

    /// <summary>
    /// Тип модификатора.
    /// </summary>
    [Column("SOURCE")]
    public UnitModifierType Type { get; init; }

    /// <summary>
    /// Комментарий к модификатору.
    /// </summary>
    [Column("COMMENTS")]
    public string Comment { get; init; } = null!;
}