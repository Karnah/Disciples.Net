using Disciples.Common.Models;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Models;

namespace Disciples.Scene.Battle.Models;

/// <summary>
/// Данные отряда.
/// </summary>
internal class BattleSquadData
{
    /// <summary>
    /// Плейсхолдеры для расположения юнита.
    /// </summary>
    public IReadOnlyDictionary<int, SceneElement> UnitPlaceholders { get; set; } = null!;

    /// <summary>
    /// Получить позицию юнита.
    /// </summary>
    public RectangleD GetUnitPosition(UnitSquadLinePosition linePosition, UnitSquadFlankPosition flankPosition)
    {
        var unitPlaceholderId = 6 - (int)linePosition - (int)flankPosition * 2;
        return UnitPlaceholders[unitPlaceholderId].Position;
    }
}