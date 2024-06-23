using Disciples.Scene.Battle.Controllers.BattleActionControllers.Base;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Processors;
using Disciples.Scene.Battle.Providers;

namespace Disciples.Scene.Battle.Controllers.BattleActionControllers;

/// <summary>
/// Контроллер защиты юнита.
/// </summary>
internal class DefendUnitActionController : BaseBattleActionController
{
    private readonly BattleProcessor _battleProcessor;

    /// <summary>
    /// Создать объект типа <see cref="DefendUnitActionController" />.
    /// </summary>
    public DefendUnitActionController(
        BattleContext context,
        BattleUnitPortraitPanelController unitPortraitPanelController,
        BattleBottomPanelController bottomPanelController,
        BattleSoundController soundController,
        IBattleGameObjectContainer battleGameObjectContainer,
        BattleProcessor battleProcessor,
        IBattleUnitResourceProvider unitResourceProvider
        ) : base(context, unitPortraitPanelController, bottomPanelController, soundController, battleGameObjectContainer, unitResourceProvider)
    {
        _battleProcessor = battleProcessor;
    }

    /// <inheritdoc />
    public override bool ShouldPassTurn { get; protected set; } = true;

    /// <inheritdoc />
    protected override BattleSquadPosition? GetTargetSquadPosition()
    {
        return CurrentBattleUnit.SquadPosition;
    }

    /// <inheritdoc />
    protected override void InitializeInternal()
    {
        var defendProcessor = _battleProcessor.ProcessDefend();
        AddProcessorAction(defendProcessor);
    }
}