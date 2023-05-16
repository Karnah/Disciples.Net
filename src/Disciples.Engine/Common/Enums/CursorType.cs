namespace Disciples.Engine.Common.Enums;

/// <summary>
/// Тип курсора.
/// </summary>
public enum CursorType
{
    /// <summary>
    /// Обычный курсор.
    /// </summary>
    Default = 1,

    /// <summary>
    /// Курсор отсутствует.
    /// </summary>
    None = 2,

    /// <summary>
    /// Курсор атаки цели.
    /// </summary>
    Enemy = 3,

    /// <summary>
    /// Курсор выбора союзника.
    /// </summary>
    Ally = 4
}