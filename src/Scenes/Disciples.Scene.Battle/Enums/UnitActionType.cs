namespace Disciples.Scene.Battle.Enums;

/// <summary>
/// Действие, которое совершает юнит.
/// </summary>
internal enum UnitActionType
{
    /// <summary>
    /// Попадание атаки в этого юнита.
    /// </summary>
    Damaged,

    /// <summary>
    /// Юнит вылечен.
    /// </summary>
    Healed,

    /// <summary>
    /// Юнит уклонился от атаки (промах атакующего).
    /// </summary>
    Miss,

    /// <summary>
    /// Защита.
    /// </summary>
    Defend,

    /// <summary>
    /// Ожидание.
    /// </summary>
    Waiting,

    /// <summary>
    /// Юнит готовится к побегу.
    /// </summary>
    Retreating,

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
    UnderEffect,

    /// <summary>
    /// Срабатывание эффекта.
    /// </summary>
    /// <remarks>
    /// Отравления, усиления, проклятия и т.д.
    /// </remarks>
    TriggeredEffect,

    /// <summary>
    /// Разовая защита от атаки
    /// </summary>
    Ward,

    /// <summary>
    /// Иммунитет от атаки.
    /// </summary>
    Immunity,

    /// <summary>
    /// Юнит получил дополнительную атаку.
    /// </summary>
    GiveAdditionalAttack
}