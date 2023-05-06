using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Models.BattleActions;
using Disciples.Scene.Battle.Providers;

namespace Disciples.Scene.Battle.Controllers.UnitActions;

/// <summary>
/// Начало хода юнита, обработка всех действующих эффектов.
/// </summary>
internal class TurnUnitAction : BaseBattleUnitAction
{
    private readonly BattleContext _battleContext;
    private readonly BattleProcessor _battleProcessor;

    private int? _currentUnitEffectIndex;
    private IReadOnlyList<UnitBattleEffect> _unitEffects = null!;

    /// <summary>
    /// Создать объект типа <see cref="TurnUnitAction" />.
    /// </summary>
    public TurnUnitAction(BattleContext context,
        IBattleGameObjectContainer battleGameObjectContainer,
        BattleUnitPortraitPanelController unitPortraitPanelController,
        IBattleUnitResourceProvider unitResourceProvider,
        BattleSoundController battleSoundController,
        BattleProcessor battleProcessor
        ) : base(context, battleGameObjectContainer, unitPortraitPanelController, unitResourceProvider, battleSoundController)
    {
        _battleContext = context;
        _battleProcessor = battleProcessor;
    }

    /// <inheritdoc />
    public override bool ShouldPassTurn { get; protected set; }

    /// <inheritdoc />
    protected override BattleSquadPosition GetTargetSquadPosition()
    {
        return CurrentBattleUnit.SquadPosition;
    }

    /// <inheritdoc />
    protected override void InitializeInternal()
    {
        _unitEffects = CurrentBattleUnit.Unit.Effects.GetBattleEffects();

        var hasBattleEffects = ProcessNextBattleEffect();

        // Сбрасываем признак защиты.
        if (!hasBattleEffects)
            CurrentBattleUnit.Unit.Effects.IsDefended = false;
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
                break;
        }
    }

    /// <inheritdoc />
    protected override void ProcessCompletedUnitAction(UnitBattleAction unitAction)
    {
        if (unitAction is UnitTriggeredEffectAction unitTriggeredEffectAction)
        {
            if (unitTriggeredEffectAction.AttackType is UnitAttackType.Poison
                or UnitAttackType.Frostbite
                or UnitAttackType.Blister)
            {
                CurrentBattleUnit.Unit.HitPoints -= unitTriggeredEffectAction.Power!.Value;

                if (CurrentBattleUnit.Unit.HitPoints == 0)
                {
                    ProcessUnitDeath(CurrentBattleUnit);
                    ShouldPassTurn = true;
                }
            }
        }

        base.ProcessCompletedUnitAction(unitAction);

        // Если завершилось последнее действие, то обрабатываем статус и запускаем следующий эффект.
        if (IsNoActions)
        {
            if (ProcessNextBattleEffect())
                return;

            CurrentBattleUnit.Unit.Effects.IsDefended = false;

            // Если все эффекты были обработаны, до ждём немного и завершаем действие.
            AddAction(new DelayBattleAction());
        }
    }

    /// <summary>
    /// Обработать эффект.
    /// </summary>
    private bool ProcessNextBattleEffect()
    {
        while (true)
        {
            if (_currentUnitEffectIndex == null)
                _currentUnitEffectIndex = 0;
            else
                _currentUnitEffectIndex += 1;

            if (_unitEffects.Count <= _currentUnitEffectIndex)
                return false;

            var unitEffect = _unitEffects[_currentUnitEffectIndex.Value];
            var attackResult = _battleProcessor.ProcessEffect(CurrentBattleUnit.Unit, unitEffect, _battleContext.Round);
            if (attackResult == null)
                continue;

            // Если эффект закончился, то удаляем его.
            if (unitEffect.Duration.IsCompleted)
                CurrentBattleUnit.Unit.Effects.Remove(unitEffect.AttackType);

            switch (unitEffect.AttackType)
            {
                case UnitAttackType.Paralyze:
                case UnitAttackType.Petrify:
                    // Если закончились все парализующие эффекты, то юнит снова начинает двигаться.
                    if (!CurrentBattleUnit.Unit.Effects.IsParalyzed)
                        CurrentBattleUnit.UnitState = BattleUnitState.Waiting;

                    // При любом эффекте паралича, юнит пропускает ход.
                    ShouldPassTurn = true;

                    AddAction(new UnitTriggeredEffectAction(CurrentBattleUnit, unitEffect.AttackType, unitEffect.Power, unitEffect.Duration));
                    PlayAttackSound(unitEffect.AttackType);
                    break;

                case UnitAttackType.Poison:
                case UnitAttackType.Frostbite:
                case UnitAttackType.Blister:
                    var effectAnimationAction = GetUnitEffectAnimationAction(CurrentBattleUnit, attackResult.AttackType!.Value);
                    if (effectAnimationAction != null)
                        AddAction(effectAnimationAction);

                    AddAction(new UnitTriggeredEffectAction(CurrentBattleUnit, attackResult.AttackType!.Value, attackResult.Power!.Value, unitEffect.Duration, animationBattleAction: effectAnimationAction));
                    PlayAttackSound(attackResult.AttackType.Value);
                    break;

                default:
                    continue;
            }

            return true;
        }
    }
}