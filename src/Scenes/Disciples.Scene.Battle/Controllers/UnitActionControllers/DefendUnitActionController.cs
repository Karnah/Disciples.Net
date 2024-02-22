using Disciples.Scene.Battle.Controllers.UnitActionControllers.Base;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Processors.UnitActionProcessors;

namespace Disciples.Scene.Battle.Controllers.UnitActionControllers;

/// <summary>
/// Контроллер защиты юнита.
/// </summary>
internal class DefendUnitActionController : BaseUnitActionController
{
    /// <summary>
    /// Создать объект типа <see cref="DefendUnitActionController" />.
    /// </summary>
    public DefendUnitActionController(
        BattleContext context,
        BattleUnitPortraitPanelController unitPortraitPanelController,
        BattleSoundController soundController
        ) : base(context, unitPortraitPanelController, soundController)
    {
    }

    /// <inheritdoc />
    public override bool ShouldPassTurn { get; protected set; } = true;

    /// <inheritdoc />
    protected override BattleSquadPosition GetTargetSquadPosition()
    {
        return CurrentBattleUnit.SquadPosition;
    }

    /// <inheritdoc />
    protected override void InitializeInternal()
    {
        var defendProcessor = new DefendProcessor(CurrentBattleUnit.Unit);
        AddProcessorAction(defendProcessor);
    }
}