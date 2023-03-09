namespace Disciples.Engine.Common.Models;

/// <summary>
/// Стоимость найма юнита/постройки здания и так далее.
/// </summary>
public class ResourceCost
{
    /// <summary>
    /// Стоимость в золоте.
    /// </summary>
    public int Gold { get; init; }

    /// <summary>
    /// Стоимость в мане смерти.
    /// </summary>
    public int DeathMana { get; init; }

    /// <summary>
    /// Стоимость в мане рун.
    /// </summary>
    public int RuneMana { get; init; }

    /// <summary>
    /// Стоимость в мане жизни.
    /// </summary>
    public int LifeMana { get; init; }

    /// <summary>
    /// Стоимость в мане ада.
    /// </summary>
    public int InfernalMana { get; init; }

    /// <summary>
    /// Стоимость в мане рощи.
    /// </summary>
    public int GroveMana { get; init; }
}