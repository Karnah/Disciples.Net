namespace Disciples.Resources.Database.Sqlite.Models;

/// <summary>
/// Базовая сущность.
/// </summary>
public interface IEntity
{
    /// <summary>
    /// Идентификатор сущности.
    /// </summary>
    public string Id { get; }
}