using Disciples.Scene.Battle.Controllers.UnitActionControllers.Base;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Processors.UnitActionProcessors;

namespace Disciples.Scene.Battle.Controllers.UnitActionControllers;

/// <summary>
/// Контроллер ожидания юнита.
/// </summary>
internal class WaitUnitActionController : BaseUnitActionController
{
    private readonly BattleContext _context;

    /// <summary>
    /// Создать объект типа <see cref="DefendUnitActionController" />.
    /// </summary>
    public WaitUnitActionController(
        BattleContext context,
        BattleUnitPortraitPanelController unitPortraitPanelController,
        BattleSoundController soundController
    ) : base(context, unitPortraitPanelController, soundController)
    {
        _context = context;
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
        var unitWaitingProcessor = new UnitWaitingProcessor(CurrentBattleUnit.Unit, _context.UnitTurnQueue);
        AddProcessorAction(unitWaitingProcessor);
    }
}