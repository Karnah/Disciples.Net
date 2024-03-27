using Disciples.Engine.Common.Enums.Units;
using Disciples.Scene.Battle.Controllers.BattleActionControllers.Base;
using Disciples.Scene.Battle.Controllers.BattleActionControllers.Models;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.GameObjects;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Processors;
using Disciples.Scene.Battle.Processors.UnitActionProcessors;
using Disciples.Scene.Battle.Providers;

namespace Disciples.Scene.Battle.Controllers.BattleActionControllers;

/// <summary>
/// Контроллер срабатывания эффектов при наступлении хода юнита.
/// </summary>
internal class BeginUnitTurnController : BaseUnitEffectActionController
{
    private readonly BattleProcessor _battleProcessor;

    /// <summary>
    /// Создать объект типа <see cref="BeginUnitTurnController" />.
    /// </summary>
    public BeginUnitTurnController(
        BattleContext context,
        BattleUnitPortraitPanelController unitPortraitPanelController,
        BattleSoundController soundController,
        IBattleGameObjectContainer battleGameObjectContainer,
        IBattleUnitResourceProvider unitResourceProvider,
        IBattleResourceProvider battleResourceProvider,
        BattleProcessor battleProcessor
        ) : base(context, unitPortraitPanelController, soundController, battleGameObjectContainer, unitResourceProvider, battleResourceProvider, battleProcessor)
    {
        _battleProcessor = battleProcessor;
    }

    /// <inheritdoc />
    protected override BattleSquadPosition? GetTargetSquadPosition()
    {
        return CurrentBattleUnit.SquadPosition;
    }

    /// <inheritdoc />
    protected override void InitializeInternal()
    {
        ShouldPassTurn = CurrentBattleUnit.Unit.Effects.IsParalyzed ||
                         CurrentBattleUnit.Unit.Effects.IsRetreating;

        var unitEffectProcessors = _battleProcessor.GetCurrentUnitEffectProcessors();
        EnqueueEffectProcessors(unitEffectProcessors);
    }

    /// <inheritdoc />
    protected override void OnCompleted()
    {
        if (CurrentBattleUnit.Unit.IsDead)
            ShouldPassTurn = true;

        base.OnCompleted();
    }

    /// <inheritdoc />
    protected override bool TryAddEffectProcessorAction(IUnitEffectProcessor effectProcessor, BattleUnit targetBattleUnit)
    {
        // Если юнит умер ранее, нет смысла обрабатывать дальше эффекты.
        // Они уже должны быть сняты смертью юнита.
        if (effectProcessor.TargetUnit.IsDead)
            return false;

        if (effectProcessor is AttackEffectProcessor attackEffectProcessor)
        {
            var attackType = attackEffectProcessor.EffectProcessor.AttackType;
            switch (attackType)
            {
                case UnitAttackType.Poison:
                case UnitAttackType.Frostbite:
                case UnitAttackType.Blister:
                {
                    AddProcessorAction(attackEffectProcessor);
                    AddAttackTypeAnimationAction(targetBattleUnit, attackType);
                    PlayAttackTypeSound(attackType);

                    return true;
                }
            }
        }
        else if (effectProcessor is UnitRetreatedProcessor)
        {
            targetBattleUnit.UnitState = BattleUnitState.Retreated;

            // Добавляем небольшую задержку, чтобы действие не закончилось сразу.
            // Это позволит обработать ShouldPassTurn для контроллера битвы.
            AddActionDelay(new BattleTimerDelay(1));
        }

        return base.TryAddEffectProcessorAction(effectProcessor, targetBattleUnit);
    }
}