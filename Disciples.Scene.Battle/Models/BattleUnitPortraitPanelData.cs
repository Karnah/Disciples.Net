using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.SceneObjects;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.GameObjects;

namespace Disciples.Scene.Battle.Models;

/// <summary>
/// Данные панели с юнитами.
/// </summary>
internal class BattleUnitPortraitPanelData
{
    /// <summary>
    /// Изображение панели.
    /// </summary>
    public IImageSceneObject? PanelImage { get; set; }

    /// <summary>
    /// Отряд, который в данный момент отображается на правой панели.
    /// </summary>
    public BattleSquadPosition? PanelSquadDirection { get; set; }

    /// <summary>
    /// Список юнитов.
    /// </summary>
    public IReadOnlyCollection<BattleUnit> BattleUnits { get; set; } = Array.Empty<BattleUnit>();

    /// <summary>
    /// Список портретов.
    /// </summary>
    public IReadOnlyList<UnitPortraitObject> UnitPortraits { get; set; } = Array.Empty<UnitPortraitObject>();

    /// <summary>
    /// Анимации-рамки, которые отрисовываются на панели с юнитами.
    /// </summary>
    public IReadOnlyList<AnimationObject> BorderAnimations { get; set; } = Array.Empty<AnimationObject>();
}