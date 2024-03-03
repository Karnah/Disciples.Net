using Disciples.Engine.Common.Enums.Units;

namespace Disciples.Engine.Common.Models;

/// <summary>
/// Раса.
/// </summary>
public class Race
{
    /// <summary>
    /// Идентификатор расы.
    /// </summary>
    public string Id { get; init; } = null!;

    /// <summary>
    /// Наименование расы.
    /// </summary>
    public string Name { get; init; } = null!;

    /// <summary>
    /// Тип расы.
    /// </summary>
    public RaceType RaceType { get; init; }

    /// <summary>
    /// Можно ли играть данной расой.
    /// </summary>
    public bool IsPlayable { get; init; }
}