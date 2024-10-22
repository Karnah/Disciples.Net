﻿using Disciples.Engine.Common.Enums.Units;
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
    private readonly BattleContext _context;
    private readonly BattleSoundController _soundController;
    private readonly IBattleGameObjectContainer _battleGameObjectContainer;
    private readonly BattleProcessor _battleProcessor;
    private readonly IBattleInterfaceController _battleInterfaceController;

    /// <summary>
    /// Создать объект типа <see cref="BeginUnitTurnController" />.
    /// </summary>
    public BeginUnitTurnController(
        BattleContext context,
        BattleUnitPortraitPanelController unitPortraitPanelController,
        BattleSoundController soundController,
        IBattleGameObjectContainer battleGameObjectContainer,
        IBattleUnitResourceProvider unitResourceProvider,
        BattleProcessor battleProcessor,
        BattleBottomPanelController bottomPanelController,
        IBattleInterfaceController battleInterfaceController
        ) : base(context, unitPortraitPanelController, soundController, battleGameObjectContainer, unitResourceProvider, battleProcessor, bottomPanelController)
    {
        _context = context;
        _soundController = soundController;
        _battleGameObjectContainer = battleGameObjectContainer;
        _battleProcessor = battleProcessor;
        _battleInterfaceController = battleInterfaceController;
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
        {
            ShouldPassTurn = true;
        }
        // Добавляем плейсхолдеры для возможности вызова юнита.
        else if (CurrentBattleUnit.Unit.MainAttack.AttackType == UnitAttackType.Summon)
        {
            var squadPosition = CurrentBattleUnit.SquadPosition;
            foreach (var summonPosition in _battleProcessor.GetSummonPositions())
            {
                var bounds = _battleInterfaceController.GetBattleUnitPosition(squadPosition, summonPosition);
                var summonPlaceholder = _battleGameObjectContainer.AddSummonPlaceholder(squadPosition, summonPosition, bounds);
                _context.SummonPlaceholders.Add(summonPlaceholder);

                // Если плейсхолдер перекрывает юнита, то запрещаем выделять его.
                // Все события будет обрабатывать плейсхолдер.
                var hiddenBattleUnit = _context
                    .GetBattleUnits(squadPosition, summonPosition)
                    .FirstOrDefault();
                if (hiddenBattleUnit != null)
                    hiddenBattleUnit.IsSelectionEnabled = false;
            }
        }

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
                    _soundController.PlayAttackTypeSound(attackType);

                    return true;
                }
            }
        }
        else if (effectProcessor is UnitRetreatedProcessor unitRetreatedProcessor)
        {
            targetBattleUnit.UnitState = BattleUnitState.Retreated;

            if (_context.CurrentBattleUnit.Unit is SummonedUnit)
            {
                AddUnitUnsummonAnimationAction(targetBattleUnit);
                RemoveBattleUnit(_context.CurrentBattleUnit);
                _soundController.PlayUnitUsummonSound();
            }

            // Если сбегающий юнит вызывал других юнитов, то они будут уничтожены.
            EnqueueEffectProcessors(unitRetreatedProcessor.UnsummonProcessors);

            // Добавляем небольшую задержку, чтобы действие не закончилось сразу.
            // Это позволит обработать ShouldPassTurn для контроллера битвы.
            AddActionDelay(new BattleTimerDelay(1));
        }

        return base.TryAddEffectProcessorAction(effectProcessor, targetBattleUnit);
    }
}