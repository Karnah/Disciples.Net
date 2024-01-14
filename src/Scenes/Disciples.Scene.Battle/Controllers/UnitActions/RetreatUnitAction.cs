using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Models.BattleActions;
using Disciples.Scene.Battle.Providers;

namespace Disciples.Scene.Battle.Controllers.UnitActions;

/// <summary>
/// Отступление юнита с поля боя.
/// </summary>
internal class RetreatUnitAction : BaseBattleUnitAction
{
    private readonly BattleUnitPortraitPanelController _unitPortraitPanelController;

    /// <summary>
    /// Создать объект типа <see cref="RetreatUnitAction" />.
    /// </summary>
    public RetreatUnitAction(
        BattleContext context,
        IBattleGameObjectContainer battleGameObjectContainer,
        BattleUnitPortraitPanelController unitPortraitPanelController,
        IBattleUnitResourceProvider unitResourceProvider,
        BattleSoundController soundController
        ) : base(context, battleGameObjectContainer, unitPortraitPanelController, unitResourceProvider, soundController)
    {
        _unitPortraitPanelController = unitPortraitPanelController;
    }

    /// <inheritdoc />
    public override bool ShouldPassTurn { get; protected set; } = true;

    /// <inheritdoc />
    protected override BattleSquadPosition GetTargetSquadPosition()
    {
        // Для отступления отряд не меняется.
        return _unitPortraitPanelController.DisplayingSquad;
    }

    /// <inheritdoc />
    protected override void InitializeInternal()
    {
        AddAction(new UnitBattleAction(CurrentBattleUnit, UnitActionType.Retreating, touchUnitActionDuration: 1));
    }

    /// <inheritdoc />
    protected override void ProcessBeginAction(IBattleAction battleAction)
    {
        switch (battleAction)
        {
            case UnitBattleAction unitBattleAction:
                ProcessBeginUnitAction(unitBattleAction);
                return;
        }
    }

    /// <inheritdoc />
    protected override void ProcessCompletedAction(IBattleAction battleAction)
    {
        switch (battleAction)
        {
            case UnitBattleAction unitBattleAction:
                ProcessCompletedUnitAction(unitBattleAction);
                return;
        }
    }
}