using System;

namespace Disciples.Engine.Common.Enums;

/// <summary>
/// Положение юнита в отряде.
/// </summary>
[Flags]
public enum UnitSquadPosition
{
    /// <summary>
    /// Пустая позиция.
    /// </summary>
    None = 0,

    /// <summary>
    /// Задняя линия.
    /// </summary>
    Back = 1,

    /// <summary>
    /// Передняя линия.
    /// </summary>
    Front = 2,

    /// <summary>
    /// Низ.
    /// </summary>
    Bottom = 32,

    /// <summary>
    /// Центр.
    /// </summary>
    Center = 64,

    /// <summary>
    /// Верх.
    /// </summary>
    Top = 128,

    /// <summary>
    /// Низ задней линии.
    /// </summary>
    BackBottom = Back | Bottom,

    /// <summary>
    /// Центр задней линии.
    /// </summary>
    BackCenter = Back | Center,

    /// <summary>
    /// Верх задней линии.
    /// </summary>
    BackTop = Back | Top,

    /// <summary>
    /// Низ передней линии.
    /// </summary>
    FrontBottom = Front | Bottom,

    /// <summary>
    /// Центр передней линии.
    /// </summary>
    FrontCenter = Front | Center,

    /// <summary>
    /// Верх передней линии.
    /// </summary>
    FrontTop = Front | Top,

    /// <summary>
    /// Большой юнит (занимает как переднюю, так и заднюю линии).
    /// </summary>
    Big = Back | Front,

    /// <summary>
    /// Большой юнит внизу.
    /// </summary>
    BigBottom = Big | Bottom,

    /// <summary>
    /// Большой юнит в центре.
    /// </summary>
    BigCenter = Big | Center,

    /// <summary>
    /// Большой юнит наверху.
    /// </summary>
    BigTop = Big | Top,

    /// <summary>
    /// Горизонтальная линия в отряде.
    /// </summary>
    HorizontalLine = Back | Front,

    /// <summary>
    /// Вертикальная линия в отряде.
    /// </summary>
    VerticalLine = Top | Center | Bottom
}
