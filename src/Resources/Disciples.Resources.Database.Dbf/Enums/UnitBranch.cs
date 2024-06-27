namespace Disciples.Resources.Database.Dbf.Enums;

/// <summary>
/// Ветка юнита.
/// </summary>
/// <remarks>
/// Описание содержится в файле LunitB.dbf.
/// </remarks>
public enum UnitBranch
{
    /// <summary>
    /// Боец ближнего боя.
    /// </summary>
    Fighter = 0,

    /// <summary>
    /// Стрелок.
    /// </summary>
    Archer = 1,

    /// <summary>
    /// Маг.
    /// </summary>
    Mage = 2,

    /// <summary>
    /// Юнит поддержки.
    /// </summary>
    Support = 3,

    /// <summary>
    /// Особый юнит расы.
    /// </summary>
    /// <remarks>
    /// Для империи - титан.
    /// Для нежити - оборотень.
    /// И так далее. Требует отдельного здания для найма.
    /// </remarks>
    Special = 4,

    /// <summary>
    /// Герой.
    /// </summary>
    Leader = 5,

    /// <summary>
    /// Лидер-вор.
    /// </summary>
    LeaderThief = 6,

    /// <summary>
    /// Вызванный юнит.
    /// </summary>
    Summon = 7
}