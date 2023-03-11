namespace Disciples.Engine.Common.Models;

/// <summary>
/// Набор ресурсов (используется для стоимости найма юнита/постройки здания, ежедневный прирост ресурсов и т.д.)
/// </summary>
public class ResourceSet
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