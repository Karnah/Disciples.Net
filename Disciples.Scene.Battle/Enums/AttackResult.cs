namespace Disciples.Scene.Battle.Enums;

/// <summary>
/// Результат атаки одного юнита на другого.
/// </summary>
public enum AttackResult
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
    Effect
}