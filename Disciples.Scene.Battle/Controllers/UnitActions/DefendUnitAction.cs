using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Models.BattleActions;

namespace Disciples.Scene.Battle.Controllers.UnitActions;

/// <summary>
/// Защита юнита.
/// </summary>
internal class DefendUnitAction : BaseBattleUnitAction
{
    /// <summary>
    /// Создать объект типа <see cref="DefendUnitAction" />.
    /// </summary>
    public DefendUnitAction(
        BattleContext context,
        IBattleGameObjectContainer battleGameObjectContainer,
        BattleUnitPortraitPanelController unitPortraitPanelController
        ) : base(context, battleGameObjectContainer, unitPortraitPanelController)
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
        AddAction(new UnitBattleAction(CurrentBattleUnit, UnitActionType.Defend));
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

        CurrentBattleUnit.Unit.Effects.AddBattleEffect(new UnitBattleEffect(UnitBattleEffectType.Defend, 1));
        AddAction(new DelayBattleAction());
    }
}