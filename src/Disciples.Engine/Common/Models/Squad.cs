namespace Disciples.Engine.Common.Models;

/// <summary>
/// Отряд.
/// </summary>
public class Squad
{
    /// <summary>
    /// Создать объект типа <see cref="Squad" />.
    /// </summary>
    /// <param name="units">Юниты в отряде.</param>
    public Squad(Unit[] units)
    {
        Units = units;
    }

    /// <summary>
    /// Юниты в отряде.
    /// </summary>
    public Unit[] Units { get; }
}