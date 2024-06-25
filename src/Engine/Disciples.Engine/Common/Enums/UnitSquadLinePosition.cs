using System;

namespace Disciples.Engine.Common.Enums;

/// <summary>
/// Позиция юнита в отряде по горизонтали.
/// </summary>
[Flags]
public enum UnitSquadLinePosition
{
    /// <summary>
    /// Отсутствует позиция.
    /// </summary>
    /// <remarks>
    /// Используется для проверок пересечения.
    /// </remarks>
    None = 0,

    /// <summary>
    /// Задняя линия.
    /// </summary>
    Back = 1,

    /// <summary>
    /// Первая линия.
    /// </summary>
    Front = 2,

    /// <summary>
    /// Обе линии.
    /// </summary>
    /// <remarks>
    /// Для больших юнитов.
    /// </remarks>
    Both = Back | Front
}