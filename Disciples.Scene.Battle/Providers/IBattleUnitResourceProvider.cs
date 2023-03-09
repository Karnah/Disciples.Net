using Disciples.Engine;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.Models;

namespace Disciples.Scene.Battle.Providers;

/// <summary>
/// Поставщик ресурсов для юнитов на поле боя.
/// </summary>
internal interface IBattleUnitResourceProvider : ISupportLoading
{
    /// <summary>
    /// Получить изображение юнита.
    /// </summary>
    /// <param name="unitType">Тип юнита.</param>
    IBitmap GetUnitFace(UnitType unitType);

    /// <summary>
    /// Получить изображение юнита для битвы (скруглённое изображение).
    /// </summary>
    /// <param name="unitType">Тип юнита.</param>
    IBitmap GetUnitBattleFace(UnitType unitType);

    /// <summary>
    /// Получить большой портрет юнита (выводится в информации о юните).
    /// </summary>
    /// <param name="unitType">Тип юнита.</param>
    IBitmap GetUnitPortrait(UnitType unitType);

    /// <summary>
    /// Получить набор анимаций юнита.
    /// </summary>
    /// <param name="unitType">Тип юнита.</param>
    /// <param name="direction">Направление положения юнита.</param>
    BattleUnitAnimation GetBattleUnitAnimation(UnitType unitType, BattleDirection direction);
}