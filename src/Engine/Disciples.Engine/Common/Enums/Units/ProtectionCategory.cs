namespace Disciples.Engine.Common.Enums.Units;

/// <summary>
/// Категория защиты от атаки.
/// </summary>
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