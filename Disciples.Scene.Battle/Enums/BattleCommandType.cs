namespace Disciples.Scene.Battle.Enums;

/// <summary>
/// Указание для юнита на поле боя.
/// </summary>
internal enum BattleCommandType
{
    /// <summary>
    /// Атаковать/применить навык на другого юнита.
    /// </summary>
    Attack,

    /// <summary>
    /// Защититься.
    /// </summary>
    Defend,

    /// <summary>
    /// Ожидать.
    /// </summary>
    Wait,

    /// <summary>
    /// Отступить.
    /// </summary>
    Retreat
}