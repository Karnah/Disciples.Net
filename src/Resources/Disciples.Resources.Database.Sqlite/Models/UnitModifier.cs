using Disciples.Resources.Database.Sqlite.Enums;

namespace Disciples.Resources.Database.Sqlite.Models;

/// <summary>
/// Модификатор характеристик или способностей юнита.
/// </summary>
public class UnitModifier : IEntity
{
    /// <summary>
    /// Идентификатор модификатора.
    /// </summary>
    public string Id { get; init; } = null!;

    /// <summary>
    /// Тип модификатора.
    /// </summary>
    public UnitModifierType Type { get; init; }

    /// <summary>
    /// Комментарий к модификатору.
    /// </summary>
    public string Comment { get; init; } = null!;

    /// <summary>
    /// Модификаторы отдельных характеристик.
    /// </summary>
    public ICollection<UnitModifierItem> ModifierItems { get; init; } = null!;
}