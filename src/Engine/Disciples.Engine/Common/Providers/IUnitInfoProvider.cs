using System.Collections.Generic;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Models;

namespace Disciples.Engine.Common.Providers;

/// <summary>
/// Поставщик информации о типах юнитов.
/// </summary>
public interface IUnitInfoProvider : ISupportLoading
{
    /// <summary>
    /// Получить информацию о типе юнита по идентификатору.
    /// </summary>
    /// <param name="unitTypeId">Идентификатор типа юнита.</param>
    UnitType GetUnitType(string unitTypeId);

    /// <summary>
    /// Получить типы юнитов для повышения из текущего.
    /// </summary>
    IReadOnlyList<UnitType> GetUpgradeUnitsTypes(string unitTypeId);

    /// <summary>
    /// Получить изображение юнита.
    /// </summary>
    /// <param name="unitTypeId">Идентификатор типа юнита.</param>
    IBitmap GetUnitFace(string unitTypeId);

    /// <summary>
    /// Получить изображение юнита для битвы (скруглённое изображение).
    /// </summary>
    /// <param name="unitTypeId">Идентификатор типа юнита.</param>
    IBitmap GetUnitBattleFace(string unitTypeId);

    /// <summary>
    /// Получить большой портрет юнита (выводится в информации о юните).
    /// </summary>
    /// <param name="unitTypeId">Идентификатор типа юнита.</param>
    IBitmap GetUnitPortrait(string unitTypeId);
}