using System.Drawing;

namespace Disciples.Scene.Battle.Constants;

/// <summary>
/// Цвета, используемые в битве.
/// </summary>
internal static class BattleColors
{
    /// <summary>
    /// Цвет урона.
    /// </summary>
    public static Color Damage { get; } = Color.FromArgb(128, 255, 0, 0);

    /// <summary>
    /// Цвет лечения.
    /// </summary>
    public static Color Heal { get; } = Color.FromArgb(128, 0, 0, 255);

    /// <summary>
    /// Цвет промаха.
    /// </summary>
    public static Color Miss { get; } = Color.FromArgb(128, 255, 255, 0);

    /// <summary>
    /// Цвет паралича.
    /// </summary>
    public static Color Paralyze { get; } = Color.FromArgb(64, 255, 255, 255);

    /// <summary>
    /// Цвет отравления.
    /// </summary>
    public static Color Poison { get; } = Color.FromArgb(128, 33, 33, 0);

    /// <summary>
    /// Цвет заморозки.
    /// </summary>
    public static Color Frostbite { get; } = Color.FromArgb(128, 0, 0, 255);

    /// <summary>
    /// Цвет ожога.
    /// </summary>
    public static Color Blister { get; } = Color.FromArgb(128, 255, 102, 0);

    /// <summary>
    /// Цвет усиления.
    /// </summary>
    public static Color Boost { get; } = Color.FromArgb(128, 0, 128, 0);

    /// <summary>
    /// Цвет разбивания брони.
    /// </summary>
    public static Color ReduceArmor { get; } = Color.FromArgb(128, 48, 213, 200);

    /// <summary>
    /// Цвет перевоплощения.
    /// </summary>
    public static Color Transform { get; } = Color.FromArgb(255, 0, 0, 0);

    /// <summary>
    /// Цвет понижения уровня.
    /// </summary>
    public static Color ReduceLevel { get; } = Color.FromArgb(128, 255, 0, 255);

    /// <summary>
    /// Прозрачный цвет.
    /// </summary>
    public static Color Transparent { get; } = Color.FromArgb(0, 0, 0, 0);

    /// <summary>
    /// Цвет начисления опыта.
    /// </summary>
    public static Color Experience { get; } = Color.FromArgb(128, 255, 255, 0);
}