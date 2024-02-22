using Disciples.Scene.Battle.Controllers.UnitActionControllers.Base;
using Disciples.Scene.Battle.Controllers.UnitActionControllers.Models;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Processors.UnitActionProcessors;

namespace Disciples.Scene.Battle.Controllers.UnitActionControllers;

/// <summary>
/// Контроллер отступления юнита.
/// </summary>
internal class RetreatingActionController : BaseUnitActionController
{
    private readonly BattleUnitPortraitPanelController _unitPortraitPanelController;
    private readonly UnitRetreatingProcessor _unitRetreatingProcessor;

    /// <summary>
    /// Создать объект типа <see cref="RetreatingActionController" />.
    /// </summary>
    public RetreatingActionController(
        BattleContext context,
        BattleUnitPortraitPanelController unitPortraitPanelController,
        BattleSoundController soundController
        ) : base(context, unitPortraitPanelController, soundController)
    {
        _unitPortraitPanelController = unitPortraitPanelController;
        _unitRetreatingProcessor = new UnitRetreatingProcessor(CurrentBattleUnit.Unit);
    }

    /// <inheritdoc />
    public override bool ShouldPassTurn { get; protected set; } = true;

    /// <inheritdoc />
    protected override BattleSquadPosition GetTargetSquadPosition()
    {
        return _unitPortraitPanelController.DisplayingSquad;
    }

    /// <inheritdoc />
    protected override void InitializeInternal()
    {
        _unitRetreatingProcessor.ProcessBeginAction();
        _unitRetreatingProcessor.ProcessCompletedAction();

        // Добавляем небольшую задержку, чтобы действие не закончилось сразу.
        // Это позволит обработать ShouldPassTurn для контроллера битвы.
        AddActionDelay(new BattleTimerDelay(1));
    }
}