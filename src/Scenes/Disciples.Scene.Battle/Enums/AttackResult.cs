namespace Disciples.Scene.Battle.Enums;

/// <summary>
/// Результат атаки одного юнита на другого.
/// </summary>
internal enum AttackResult
{
    /// <summary>
    /// Промах.
    /// </summary>
    Miss,

    /// <summary>
    /// Успешная атака.
    /// </summary>
    Attack,

    /// <summary>
    /// Разовая защита от атаки
    /// </summary>
    Ward,

    /// <summary>
    /// Иммунитет от атаки.
    /// </summary>
    Immunity,

    /// <summary>
    /// Пропустить первую атаку и перейти сразу ко второй.
    /// </summary>
    Skip
}