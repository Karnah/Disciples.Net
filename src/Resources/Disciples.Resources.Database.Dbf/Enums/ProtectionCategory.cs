namespace Disciples.Resources.Database.Dbf.Enums;

/// <summary>
/// Категория защиты от атаки.
/// </summary>
/// <remarks>
/// Описание содержится в файле LImmune.dbf.
/// </remarks>
public enum ProtectionCategory
{
    /// <summary>
    /// Нет защиты.
    /// </summary>
    NoProtection = 1,

    /// <summary>
    /// Защита от одного удара.
    /// </summary>
    Ward = 2,

    /// <summary>
    /// Полная невосприимчивость.
    /// </summary>
    Immunity = 3
}