using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.GameObjects;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Models.BattleActions;
using Disciples.Scene.Battle.Providers;

namespace Disciples.Scene.Battle.Controllers.UnitActions;

/// <summary>
/// Вторая атака юнита.
/// </summary>
internal class SecondaryAttackUnitAction : BaseBattleUnitAction
{
    private readonly BattleProcessor _battleProcessor;

    private readonly BattleUnit _attackerBattleUnit;
    private readonly IReadOnlyList<BattleUnit> _targetBattleUnits;
    private readonly int? _power;

    /// <summary>
    /// Создать объект типа <see cref="SecondaryAttackUnitAction" />.
    /// </summary>
    public SecondaryAttackUnitAction(
        BattleContext context,
        IBattleGameObjectContainer battleGameObjectContainer,
        BattleUnitPortraitPanelController unitPortraitPanelController,
        IBattleUnitResourceProvider unitResourceProvider,
        BattleSoundController battleSoundController,
        BattleProcessor battleProcessor,
        BattleUnit attackerBattleUnit,
        IReadOnlyList<BattleUnit> targetBattleUnits,
        int? power,
        bool shouldPassTurn
        ) : base(context, battleGameObjectContainer, unitPortraitPanelController, unitResourceProvider, battleSoundController)
    {
        _battleProcessor = battleProcessor;
        _attackerBattleUnit = attackerBattleUnit;
        _targetBattleUnits = targetBattleUnits;
        _power = power;

        ShouldPassTurn = shouldPassTurn;
    }

    /// <inheritdoc />
    public sealed override bool ShouldPassTurn { get; protected set; }

    /// <inheritdoc />
    protected override BattleSquadPosition GetTargetSquadPosition()
    {
        return _targetBattleUnits[0].SquadPosition;
    }

    /// <inheritdoc />
    protected override void InitializeInternal()
    {
        foreach (var targetBattleUnit in _targetBattleUnits)
        {
            var attackResult = _battleProcessor.ProcessSecondaryAttack(_attackerBattleUnit.Unit, targetBattleUnit.Unit, _power);
            ProcessAttackResult(_attackerBattleUnit, targetBattleUnit, attackResult, false);
        }
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
            case AnimationBattleAction animationAction:
                ProcessCompletedBattleUnitAnimation(animationAction);
                return;

            case UnitBattleAction unitBattleAction:
                ProcessCompletedUnitAction(unitBattleAction);
                return;
        }
    }
}