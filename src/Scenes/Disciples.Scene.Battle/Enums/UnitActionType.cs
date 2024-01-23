namespace Disciples.Scene.Battle.Enums;

/// <summary>
/// Действие, которое совершает юнит.
/// </summary>
internal enum UnitActionType
{
    /// <summary>
    /// Попадание атаки в этого юнита.
    /// </summary>
    /// <remarks>
    /// Положительные эффекты, тоже считаются атаками.
    /// </remarks>
    Attacked,

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
    /// Разовая защита от атаки
    /// </summary>
    Ward,

    /// <summary>
    /// Иммунитет от атаки.
    /// </summary>
    Immunity,
}