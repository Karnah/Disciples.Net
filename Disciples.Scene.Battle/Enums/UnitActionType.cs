namespace Disciples.Scene.Battle.Enums;

/// <summary>
/// Действие, которое совершает юнит.
/// </summary>
internal enum UnitActionType
{
    /// <summary>
    /// Попадание атаки в этого юнита.
    /// </summary>
    GetHit,

    /// <summary>
    /// Юнит уклонился от атаки (промах атакующего).
    /// </summary>
    Dodge,

    /// <summary>
    /// Защита.
    /// </summary>
    Defend,

    /// <summary>
    /// Ожидание.
    /// </summary>
    Waiting,

    /// <summary>
    /// Юнит умирает.
    /// </summary>
    Dying,

    /// <summary>
    /// Наложение эффекта.
    /// </summary>
    /// <remarks>
    /// Отравления, усиления, проклятия и т.д.
    /// </remarks>
    UnderEffect
}