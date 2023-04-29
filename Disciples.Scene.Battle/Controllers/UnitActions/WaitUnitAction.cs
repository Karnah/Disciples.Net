using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Models.BattleActions;
using Disciples.Scene.Battle.Providers;

namespace Disciples.Scene.Battle.Controllers.UnitActions;

/// <summary>
/// Ожидание юнита.
/// </summary>
internal class WaitUnitAction : BaseBattleUnitAction
{
    private readonly BattleContext _battleContext;

    /// <summary>
    /// Создать объект типа <see cref="WaitUnitAction" />.
    /// </summary>
    public WaitUnitAction(BattleContext context,
        IBattleGameObjectContainer battleGameObjectContainer,
        BattleUnitPortraitPanelController unitPortraitPanelController,
        IBattleUnitResourceProvider unitResourceProvider,
        BattleSoundController battleSoundController
        ) : base(context, battleGameObjectContainer, unitPortraitPanelController, unitResourceProvider, battleSoundController)
    {
        _battleContext = context;
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
        AddAction(new UnitBattleAction(CurrentBattleUnit, UnitActionType.Waiting));
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

    /// <inheritdoc />
    protected override void ProcessCompletedUnitAction(UnitBattleAction unitAction)
    {
        base.ProcessCompletedUnitAction(unitAction);

        _battleContext.UnitTurnQueue.UnitWait(CurrentBattleUnit.Unit);

        AddAction(new DelayBattleAction());
    }
}