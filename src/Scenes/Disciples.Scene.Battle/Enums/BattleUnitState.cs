namespace Disciples.Scene.Battle.Enums;

/// <summary>
/// Состояние юнита на поле боя.
/// </summary>
internal enum BattleUnitState
{
    /// <summary>
    /// Юнит ожидает.
    /// </summary>
    Waiting,

    /// <summary>
    /// Юнит атакует.
    /// </summary>
    Attacking,

    /// <summary>
    /// Юнит получает удар.
    /// </summary>
    TakingDamage,

    /// <summary>
    /// Юнит парализован.
    /// </summary>
    Paralyzed,

    /// <summary>
    /// Юнит убит.
    /// </summary>
    Dead
}