using Disciples.Scene.Battle.Controllers.BattleActionControllers.Base;
using Disciples.Scene.Battle.Controllers.BattleActionControllers.Models;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Providers;

namespace Disciples.Scene.Battle.Controllers.BattleActionControllers;

/// <summary>
/// Контроллер начала битвы.
/// </summary>
/// <remarks>
/// Необходимо, чтобы корректно отрабатывала инициализация всех классов в начале сцены.
/// </remarks>
internal class BeginBattleActionController : BaseBattleActionController
{
    /// <summary>
    /// Создать объект типа <see cref="BeginBattleActionController" />.
    /// </summary>
    public BeginBattleActionController(
        BattleContext context,
        BattleUnitPortraitPanelController unitPortraitPanelController,
        BattleBottomPanelController bottomPanelController,
        IBattleGameObjectContainer battleGameObjectContainer,
        IBattleUnitResourceProvider unitResourceProvider
        ) : base(context, unitPortraitPanelController, bottomPanelController, battleGameObjectContainer, unitResourceProvider)
    {
    }

    /// <inheritdoc />
    public override bool ShouldPassTurn { get; protected set; }

    /// <inheritdoc />
    protected override void InitializeInternal()
    {
        AddActionDelay(new BattleTimerDelay(1));
    }
}
