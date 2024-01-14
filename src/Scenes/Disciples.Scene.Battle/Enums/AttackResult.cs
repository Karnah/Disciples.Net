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
    /// Исцеление юнита.
    /// </summary>
    Heal,

    /// <summary>
    /// Наложение эффекта.
    /// </summary>
    /// <remarks>
    /// Отравления, усиления, проклятия и т.д.
    /// </remarks>
    Effect,

    /// <summary>
    /// Разовая защита от атаки
    /// </summary>
    Ward,

    /// <summary>
    /// Иммунитет от атаки.
    /// </summary>
    Immunity,

    /// <summary>
    /// Юнит испугался и собирается сбежать.
    /// </summary>
    Fear
}