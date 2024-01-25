namespace Disciples.Resources.Database.Dbf.Enums;

/// <summary>
/// Анимация смерти юнита.
/// </summary>
/// <remarks>
/// Описание содержится в файле LDthAnim.dbf.
/// </remarks>
public enum UnitDeathAnimationType
{
    /// <summary>
    /// Анимация отсутствует.
    /// </summary>
    None = 0,

    /// <summary>
    /// Анимация смерти людей.
    /// </summary>
    Human = 1,

    /// <summary>
    /// Анимация смерти легионов проклятых.
    /// </summary>
    Heretic = 2,

    /// <summary>
    /// Анимация смерти гномов.
    /// </summary>
    Dwarf = 3,

    /// <summary>
    /// Анимация смерти орд нежити.
    /// </summary>
    Undead = 4,

    /// <summary>
    /// Анимация смерти нейтралов.
    /// </summary>
    Neutral = 5,

    /// <summary>
    /// Анимация смерти драконов.
    /// </summary>
    Dragon = 6,

    /// <summary>
    /// Анимация смерти призраков.
    /// </summary>
    Ghost = 7,

    /// <summary>
    /// Анимация смерти эльфов.
    /// </summary>
    Elf = 8
}