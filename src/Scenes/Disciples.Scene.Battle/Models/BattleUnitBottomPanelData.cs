using Disciples.Engine.Common.GameObjects;
using Disciples.Scene.Battle.GameObjects;

namespace Disciples.Scene.Battle.Models;

/// <summary>
/// Объекты, связанные с нижней панелью юнита.
/// </summary>
internal class BattleUnitBottomPanelData
{
    /// <summary>
    /// Юнит, который отображается на панели.
    /// </summary>
    public BattleUnit? BattleUnit { get; set; }

    /// <summary>
    /// Портрет юнита.
    /// </summary>
    public ImageObject Portrait { get; init; } = null!;

    /// <summary>
    /// Информация о юните.
    /// </summary>
    public TextBlockObject Info { get; init; } = null!;

    /// <summary>
    /// Изображение панели для предметов лидера-юнита.
    /// </summary>
    public ImageObject LeaderItemsPanel { get; init; } = null!;
}