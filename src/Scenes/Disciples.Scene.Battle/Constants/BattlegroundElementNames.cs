namespace Disciples.Scene.Battle.Constants;

/// <summary>
/// Имена элементов на поле боя.
/// </summary>
internal static class BattlegroundElementNames
{
    /// <summary>
    /// Паттерн для поиска плейсхолдеров для позиции атакующего юнита на поле боя.
    /// </summary>
    /// <remarks>
    /// Пример: IMG_AUNIT1.
    /// </remarks>
    public const string ATTACK_UNIT_PATTERN_PLACEHOLDER = "IMG_AUNIT";

    /// <summary>
    /// Паттерн для поиска плейсхолдеров для позиции защищающегося юнита на поле боя.
    /// </summary>
    /// <remarks>
    /// Пример: IMG_DUNIT1.
    /// </remarks>
    public const string DEFEND_UNIT_PATTERN_PLACEHOLDER = "IMG_DUNIT";
}