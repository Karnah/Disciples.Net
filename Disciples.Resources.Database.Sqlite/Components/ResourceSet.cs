namespace Disciples.Resources.Database.Sqlite.Components;

/// <summary>
/// Набор ресурсов (используется для стоимости найма юнита/постройки здания, ежедневный прирост ресурсов и т.д.)
/// </summary>
public class ResourceSet
{
    /// <summary>
    /// Количество золота.
    /// </summary>
    public int Gold { get; init; }

    /// <summary>
    /// Количество маны смерти.
    /// </summary>
    public int DeathMana { get; init; }

    /// <summary>
    /// Количество маны рун.
    /// </summary>
    public int RuneMana { get; init; }

    /// <summary>
    /// Количество маны жизни.
    /// </summary>
    public int LifeMana { get; init; }

    /// <summary>
    /// Количество маны ада.
    /// </summary>
    public int InfernalMana { get; init; }

    /// <summary>
    /// Количество маны рощи.
    /// </summary>
    public int GroveMana { get; init; }
}