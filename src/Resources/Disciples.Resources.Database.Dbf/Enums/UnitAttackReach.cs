namespace Disciples.Resources.Database.Dbf.Enums;

/// <summary>
/// Юниты, которые являются целями.
/// </summary>
/// <remarks>
/// Описание содержится в файле LAttR.dbf.
/// </remarks>
public enum UnitAttackReach
{
    /// <summary>
    /// Все юниты.
    /// </summary>
    All = 1,

    /// <summary>
    /// Любой юнит.
    /// </summary>
    Any = 2,

    /// <summary>
    /// Ближайший.
    /// </summary>
    Adjacent = 3
}