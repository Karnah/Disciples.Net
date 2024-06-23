using Disciples.Engine.Common.Enums.Units;
using Disciples.Scene.Battle.Controllers.BattleActionControllers.Models;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.GameObjects;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Processors;
using Disciples.Scene.Battle.Processors.UnitActionProcessors;
using Disciples.Scene.Battle.Providers;

namespace Disciples.Scene.Battle.Controllers.BattleActionControllers.Base;

/// <summary>
/// Базовый контроллер для атаки.
/// </summary>
internal abstract class BaseAttackActionController : BaseUnitEffectActionController
{
    private readonly BattleContext _context;
    private readonly BattleUnitPortraitPanelController _unitPortraitPanelController;
    private readonly IBattleGameObjectContainer _battleGameObjectContainer;
    private readonly IBattleUnitResourceProvider _unitResourceProvider;
    private readonly BattleProcessor _battleProcessor;

    /// <summary>
    /// Создать объект типа <see cref="BaseAttackActionController" />.
    /// </summary>
    protected BaseAttackActionController(
        BattleContext context,
        BattleUnitPortraitPanelController unitPortraitPanelController,
        BattleSoundController soundController,
        IBattleGameObjectContainer battleGameObjectContainer,
        IBattleUnitResourceProvider unitResourceProvider,
        IBattleResourceProvider battleResourceProvider,
        BattleProcessor battleProcessor,
        BattleBottomPanelController bottomPanelController
        ) : base(context, unitPortraitPanelController, soundController, battleGameObjectContainer, unitResourceProvider, battleResourceProvider, battleProcessor, bottomPanelController)
    {
        _context = context;
        _unitPortraitPanelController = unitPortraitPanelController;
        _battleGameObjectContainer = battleGameObjectContainer;
        _unitResourceProvider = unitResourceProvider;
        _battleProcessor = battleProcessor;
    }

    /// <inheritdoc />
    public override bool ShouldPassTurn { get; protected set; }

    /// <summary>
    /// Добавить обработчик действия.
    /// </summary>
    protected override void AddProcessorAction(IUnitActionProcessor unitActionProcessor)
    {
        if (unitActionProcessor is UnitSuccessAttackProcessor unitAttackProcessor)
        {
            AddSuccessAttackProcessorAction(unitAttackProcessor);
            return;
        }

        base.AddProcessorAction(unitActionProcessor);
    }

    /// <summary>
    /// Обработать начало атаки.
    /// </summary>
    protected virtual void AddSuccessAttackProcessorAction(UnitSuccessAttackProcessor unitSuccessAttackProcessor)
    {
        // Обработчик исцеления сразу снимает все эффекты.
        // Но для UI так не подойдёт, так как необходимо обрабатывать с задержками отдельные эффекты (например, превращение).
        // Поэтому пропускаем его использование.
        if (unitSuccessAttackProcessor.AttackTypeProcessor.AttackType != UnitAttackType.Cure)
            unitSuccessAttackProcessor.ProcessBeginAction();

        foreach (var attackResult in unitSuccessAttackProcessor.AttackResults)
        {
            // Если идёт вызов юнита, то нужно сразу создать для него игровой объект.
            // Но видимым он станет только после завершения атаки.
            if (attackResult.AttackType == UnitAttackType.Summon)
            {
                var battleUnit = AddBattleUnit(attackResult.Context.TargetUnit, CurrentBattleUnit.SquadPosition);
                battleUnit.IsHidden = true;
            }

            var targetBattleUnit = _context.GetBattleUnit(attackResult.Context.TargetUnit);
            ProcessBeginSuccessAttack(targetBattleUnit, attackResult);
        }

        // Добавляем анимации исцеления вампиризмом.
        foreach (var drainLifeHealUnit in unitSuccessAttackProcessor.DrainLifeHealUnits)
            AddDrainLifeHealAnimationAction(_context.GetBattleUnit(drainLifeHealUnit));

        AddActionDelay(new BattleTimerDelay(COMMON_ACTION_DELAY,
            () => OnSuccessAttackProcessorActionCompleted(unitSuccessAttackProcessor)));
    }

    /// <summary>
    /// Обработать завершение атаки.
    /// </summary>
    private void OnSuccessAttackProcessorActionCompleted(UnitSuccessAttackProcessor unitSuccessAttackProcessor)
    {
        unitSuccessAttackProcessor.ProcessCompletedAction();

        foreach (var attackResult in unitSuccessAttackProcessor.AttackResults)
        {
            var targetBattleUnit = _context.GetBattleUnit(attackResult.Context.TargetUnit);
            ProcessCompleteSuccessAttack(targetBattleUnit, attackResult);
        }
    }

    /// <summary>
    /// Обработать начало атаки на конкретного юнита.
    /// </summary>
    protected virtual void ProcessBeginSuccessAttack(BattleUnit targetBattleUnit, CalculatedAttackResult attackResult)
    {
        _unitPortraitPanelController.DisplayMessage(targetBattleUnit,
            new BattleUnitPortraitEventData(attackResult));

        var attackType = attackResult.AttackType;
        switch (attackType)
        {
            case UnitAttackType.Damage:
            case UnitAttackType.DrainLife:
            case UnitAttackType.DrainLifeOverflow:
            {
                targetBattleUnit.UnitState = BattleUnitState.TakingDamage;
                AddActionDelay(new BattleAnimationDelay(targetBattleUnit.AnimationComponent,
                    () => OnUnitDamagedAnimationCompleted(targetBattleUnit)));
                PlayRandomDamagedSound(targetBattleUnit.SoundComponent.Sounds.DamagedSounds);
                break;
            }

            case UnitAttackType.Paralyze:
            case UnitAttackType.Petrify:
            {
                targetBattleUnit.UnitState = BattleUnitState.Paralyzed;
                break;
            }

            case UnitAttackType.Revive:
            case UnitAttackType.ReduceLevel:
            case UnitAttackType.Doppelganger:
            case UnitAttackType.TransformSelf:
            case UnitAttackType.TransformEnemy:
            // В отличие от заморозки и яда, для вспышки выводится анимация при наложении эффекта.
            case UnitAttackType.Blister:
            {
                AddAttackTypeAnimationAction(targetBattleUnit, attackType);
                break;
            }
        }

        PlayAttackTypeSound(attackResult.AttackType);
    }

    /// <summary>
    /// Обработать завершение атаки на конкретного юнита.
    /// </summary>
    private void ProcessCompleteSuccessAttack(BattleUnit targetBattleUnit, CalculatedAttackResult attackResult)
    {
        _unitPortraitPanelController.CloseMessage(targetBattleUnit);

        switch (attackResult.AttackType)
        {
            case UnitAttackType.Revive:
            {
                targetBattleUnit.UnitState = BattleUnitState.Waiting;
                break;
            }

            case UnitAttackType.Cure:
            {
                var effectProcessors =
                    _battleProcessor.GetForceCompleteEffectProcessors(targetBattleUnit.Unit, attackResult.CuredEffects);
                EnqueueEffectProcessors(effectProcessors);
                break;
            }

            case UnitAttackType.Summon:
            {
                targetBattleUnit.IsHidden = false;
                break;
            }

            case UnitAttackType.ReduceLevel:
            case UnitAttackType.Doppelganger:
            case UnitAttackType.TransformSelf:
            case UnitAttackType.TransformEnemy:
            {
                ProcessTransformUnit(targetBattleUnit, attackResult.TransformedUnit!);
                break;
            }
        }
    }

    /// <summary>
    /// Обработать завершение анимации получения юнитом урона.
    /// </summary>
    private void OnUnitDamagedAnimationCompleted(BattleUnit targetBattleUnit)
    {
        targetBattleUnit.UnitState = targetBattleUnit.Unit.Effects.IsParalyzed
            ? BattleUnitState.Paralyzed
            : BattleUnitState.Waiting;

        // Обрабатываем смерть юнита.
        if (targetBattleUnit.Unit.HitPoints == 0)
            ProcessUnitDeath(targetBattleUnit);
    }

    #region Анимации и звуки

    /// <summary>
    /// Добавить анимацию для исцеления юнита вампиризмом.
    /// </summary>
    private void AddDrainLifeHealAnimationAction(BattleUnit targetBattleUnit)
    {
        var animationPoint = targetBattleUnit.AnimationComponent.AnimationPoint;
        var drainLifeHealAnimation = _battleGameObjectContainer.AddAnimation(
            _unitResourceProvider.DrainLifeHealAnimationFrames,
            animationPoint.X,
            animationPoint.Y,
            targetBattleUnit.AnimationComponent.Layer + 2,
            false);
        AddActionDelay(new BattleAnimationDelay(drainLifeHealAnimation.AnimationComponent));
    }

    #endregion
}