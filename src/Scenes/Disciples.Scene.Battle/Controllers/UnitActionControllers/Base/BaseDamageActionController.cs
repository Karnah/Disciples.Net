using Disciples.Engine.Common.Enums.Units;
using Disciples.Resources.Sounds.Models;
using Disciples.Scene.Battle.Controllers.UnitActionControllers.Models;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.GameObjects;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Processors;
using Disciples.Scene.Battle.Processors.UnitActionProcessors;
using Disciples.Scene.Battle.Providers;

namespace Disciples.Scene.Battle.Controllers.UnitActionControllers.Base;

/// <summary>
/// Базовый контроллер для действий, которые могут наносить урон.
/// </summary>
internal abstract class BaseDamageActionController : BaseUnitActionController
{
    private readonly BattleContext _context;
    private readonly BattleUnitPortraitPanelController _unitPortraitPanelController;
    private readonly IBattleGameObjectContainer _battleGameObjectContainer;
    private readonly IBattleUnitResourceProvider _unitResourceProvider;
    private readonly IBattleResourceProvider _battleResourceProvider;
    private readonly BattleProcessor _battleProcessor;

    /// <summary>
    /// Признак, что проигрывается звук получения урона юнитом.
    /// </summary>
    private bool _isDamagedSoundPlaying;

    /// <summary>
    /// Признак, что проигрывается звук смерти.
    /// </summary>
    private bool _isDeathSoundPlaying;

    /// <summary>
    /// Очередь из обработчиков эффектов.
    /// </summary>
    /// <remarks>
    /// Используется в двух случаях:
    /// 1. Для <see cref="BeginUnitTurnController" />, чтобы обработать срабатывание эффекта в начале хода юнита.
    /// 2. При обработке типа атаки <see cref="UnitAttackType.Cure" />, чтобы снимать эффекты с юнитов.
    /// </remarks>
    private readonly Queue<IUnitEffectProcessor> _unitEffectProcessors = new();

    /// <summary>
    /// Признак, что идёт обработка эффекта.
    /// </summary>
    private bool _isUnitEffectProcessing;

    /// <summary>
    /// Создать объект типа <see cref="BaseDamageActionController" />.
    /// </summary>
    protected BaseDamageActionController(
        BattleContext context,
        BattleUnitPortraitPanelController unitPortraitPanelController,
        BattleSoundController soundController,
        IBattleGameObjectContainer battleGameObjectContainer,
        IBattleUnitResourceProvider unitResourceProvider,
        IBattleResourceProvider battleResourceProvider,
        BattleProcessor battleProcessor
        ) : base(context, unitPortraitPanelController, soundController)
    {
        _context = context;
        _unitPortraitPanelController = unitPortraitPanelController;
        _battleGameObjectContainer = battleGameObjectContainer;
        _unitResourceProvider = unitResourceProvider;
        _battleResourceProvider = battleResourceProvider;
        _battleProcessor = battleProcessor;
    }

    /// <inheritdoc />
    public override bool ShouldPassTurn { get; protected set; }

    /// <inheritdoc />
    protected override void OnProcessorActionCompleted(IUnitActionProcessor unitActionProcessor, BattleUnit targetBattleUnit)
    {
        base.OnProcessorActionCompleted(unitActionProcessor, targetBattleUnit);

        if (unitActionProcessor is IUnitEffectProcessor unitEffectProcessor)
            OnEffectProcessorActionCompleted(unitEffectProcessor, targetBattleUnit);
        else if (unitActionProcessor is UnitDeathProcessor unitDeathProcessor)
            OnUnitDeathProcessorCompleted(unitDeathProcessor, targetBattleUnit);
    }

    #region Очередь эффектов

    /// <summary>
    /// Добавить в очередь обработчики эффектов.
    /// </summary>
    protected void EnqueueEffectProcessors(IReadOnlyList<IUnitEffectProcessor> effectProcessors)
    {
        foreach (var unitEffectProcessor in effectProcessors)
            _unitEffectProcessors.Enqueue(unitEffectProcessor);

        if (!_isUnitEffectProcessing)
            ProcessNextBattleEffect();
    }

    /// <summary>
    /// Обработать следующий эффект.
    /// </summary>
    private void ProcessNextBattleEffect()
    {
        while (_unitEffectProcessors.TryDequeue(out var effectProcessor))
        {
            var battleUnit = _context.GetBattleUnit(effectProcessor.TargetUnit);
            if (TryAddEffectProcessorAction(effectProcessor, battleUnit))
            {
                _isUnitEffectProcessing = true;
                return;
            }
        }

        _isUnitEffectProcessing = false;
    }

    /// <summary>
    /// Добавить действие для обработки эффекта.
    /// </summary>
    /// <returns>
    /// <see langword="true" />, если обработчик требует ожидания.
    /// <see langword="false" />, если обработчик выполняется мгновенно.
    /// </returns>
    protected virtual bool TryAddEffectProcessorAction(IUnitEffectProcessor effectProcessor, BattleUnit targetBattleUnit)
    {
        if (effectProcessor is AttackEffectProcessor attackEffectProcessor)
        {
            var attackType = attackEffectProcessor.EffectProcessor.AttackType;
            switch (attackType)
            {
                case UnitAttackType.Paralyze:
                case UnitAttackType.Petrify:
                {
                    AddProcessorAction(attackEffectProcessor);

                    // Если закончились все парализующие эффекты, то юнит снова начинает двигаться.
                    if (!targetBattleUnit.Unit.Effects.IsParalyzed)
                        targetBattleUnit.UnitState = BattleUnitState.Waiting;

                    return true;
                }

                case UnitAttackType.TransformEnemy when attackEffectProcessor.EffectResult.NewDuration.IsCompleted:
                {
                    AddProcessorAction(attackEffectProcessor);
                    AddAttackTypeAnimationAction(targetBattleUnit, attackType);
                    PlayAttackTypeSound(attackType);
                    return true;
                }
            }
        }

        effectProcessor.ProcessBeginAction();
        effectProcessor.ProcessCompletedAction();
        return false;
    }

    /// <inheritdoc />
    protected override BattleUnitPortraitEventData GetProcessorPortraitEventData(IUnitActionProcessor unitActionProcessor)
    {
        if (unitActionProcessor is AttackEffectProcessor attackEffectProcessor)
            return new BattleUnitPortraitEventData(attackEffectProcessor.EffectResult);

        return base.GetProcessorPortraitEventData(unitActionProcessor);
    }

    /// <summary>
    /// Обработать завершение обработки эффекта.
    /// </summary>
    private void OnEffectProcessorActionCompleted(IUnitEffectProcessor unitEffectProcessor, BattleUnit targetBattleUnit)
    {
        if (targetBattleUnit.Unit.HitPoints == 0 && !targetBattleUnit.Unit.IsDead)
        {
            ProcessUnitDeath(targetBattleUnit);
            return;
        }

        if (unitEffectProcessor is AttackEffectProcessor attackEffectProcessor &&
            attackEffectProcessor.EffectResult.NewDuration.IsCompleted)
        {
            switch (attackEffectProcessor.EffectResult.Effect.AttackType)
            {
                case UnitAttackType.TransformEnemy:
                {
                    ProcessTransformUnitBack(targetBattleUnit);
                    break;
                }
            }
        }

        // Продолжаем обрабатывать очередь из эффектов.
        ProcessNextBattleEffect();
    }

    /// <summary>
    /// Обработать превращение юнита.
    /// </summary>
    protected void ProcessTransformUnit(BattleUnit originalBattleUnit, TransformedEnemyUnit transformedUnit)
    {
        originalBattleUnit.IsHidden = true;
        var transformedBattleUnit = _battleGameObjectContainer.AddBattleUnit(transformedUnit, originalBattleUnit.SquadPosition);
        var targetUnitBattleIndex = _context.BattleUnits.IndexOf(originalBattleUnit);
        _context.BattleUnits[targetUnitBattleIndex] = transformedBattleUnit;
        _context.TransformedUnits.Add(originalBattleUnit);

        var targetUnitPortrait = _unitPortraitPanelController.GetUnitPortrait(originalBattleUnit);
        if (targetUnitPortrait != null)
            targetUnitPortrait.Unit = transformedUnit;
    }

    /// <summary>
    /// Обработать возвращение юнита в исходную форму.
    /// </summary>
    private void ProcessTransformUnitBack(BattleUnit transformedBattleUnit)
    {
        var transformedUnit = (TransformedEnemyUnit)transformedBattleUnit.Unit;
        var originalUnit = transformedUnit.OriginalUnit;

        var unitPortrait = _unitPortraitPanelController.GetUnitPortrait(transformedBattleUnit);
        if (unitPortrait != null)
            unitPortrait.Unit = originalUnit;

        var originalBattleUnit = _context.TransformedUnits.First(tu => tu.Unit == originalUnit);
        originalBattleUnit.IsHidden = false;
        _context.TransformedUnits.Remove(originalBattleUnit);

        var targetUnitBattleIndex = _context.BattleUnits.IndexOf(transformedBattleUnit);
        _context.BattleUnits[targetUnitBattleIndex] = originalBattleUnit;
        transformedBattleUnit.Destroy();

        // BUG: Опасная штука, так как эффекты для юнита рассчитываются заранее.
        // Но у всех трансформированных юнитов общий список эффектов.
        if (_context.CurrentBattleUnit == transformedBattleUnit)
            _context.CurrentBattleUnit = originalBattleUnit;
    }

    #endregion

    #region Смерть юнита

    /// <summary>
    /// Обработать смерть юнита.
    /// </summary>
    protected void ProcessUnitDeath(BattleUnit targetBattleUnit)
    {
        var attackProcessorContext = _context.CreateAttackProcessorContext(targetBattleUnit);
        var effectsProcessor = _battleProcessor.GetForceCompleteEffectProcessors(attackProcessorContext);
        var unitDeathProcessor = new UnitDeathProcessor(targetBattleUnit.Unit, effectsProcessor);

        AddProcessorAction(unitDeathProcessor);
        AddUnitDeathAnimationAction(targetBattleUnit);
        PlayUnitDeathSound();
    }

    /// <summary>
    /// Обработать завершение смерти юнита.
    /// </summary>
    private void OnUnitDeathProcessorCompleted(UnitDeathProcessor unitDeathProcessor, BattleUnit targetBattleUnit)
    {
        unitDeathProcessor.ProcessCompletedAction();

        // Превращаем его в кучу костей.
        targetBattleUnit.UnitState = BattleUnitState.Dead;

        // Юнит умер в результате срабатывания эффекта.
        // Всё равно продолжаем обрабатывать очередь, так как в очереди могут быть эффекты на других юнитов.
        // Например, когда умер юнит, который наложил на других "Даровать защиту".
        if (_unitEffectProcessors.Count > 0)
            ProcessNextBattleEffect();
    }

    /// <summary>
    /// Получить анимацию смерти.
    /// </summary>
    private void AddUnitDeathAnimationAction(BattleUnit targetBattleUnit)
    {
        var animationPoint = targetBattleUnit.AnimationComponent.AnimationPoint;
        var deathAnimation = _battleGameObjectContainer.AddAnimation(
            targetBattleUnit.AnimationComponent.BattleUnitAnimation.DeathFrames,
            animationPoint.X,
            animationPoint.Y,
            targetBattleUnit.AnimationComponent.Layer + 2,
            false);
        AddActionDelay(new BattleAnimationDelay(deathAnimation.AnimationComponent));
    }

    /// <summary>
    /// Проиграть звук смерти юнита.
    /// </summary>
    private void PlayUnitDeathSound()
    {
        // Если несколько юнитов умирает сразу, то звук проигрываем только один раз.
        if (_isDeathSoundPlaying)
            return;

        _isDeathSoundPlaying = true;
        PlaySound(_battleResourceProvider.UnitDeathSound);
    }

    #endregion

    #region Прочие анимации и звуки

    /// <summary>
    /// Добавить анимацию типа атаки на юнита.
    /// </summary>
    protected void AddAttackTypeAnimationAction(BattleUnit battleUnit, UnitAttackType effectAttackType)
    {
        var animationFrames = _unitResourceProvider.GetAttackTypeAnimation(effectAttackType, battleUnit.Unit.UnitType.IsSmall);
        if (animationFrames == null)
            return;

        var animationPoint = battleUnit.AnimationComponent.AnimationPoint;
        var animation = _battleGameObjectContainer.AddAnimation(
            animationFrames,
            animationPoint.X,
            animationPoint.Y,
            battleUnit.AnimationComponent.Layer + 2,
            false);
        AddActionDelay(new BattleAnimationDelay(animation.AnimationComponent));
    }

    /// <summary>
    /// Проиграть случай звук получения урона.
    /// </summary>
    protected void PlayRandomDamagedSound(IReadOnlyList<RawSound> sounds)
    {
        // Если несколько юнитов получили урон, то звук будет проигрываться только один раз.
        if (_isDamagedSoundPlaying)
            return;

        _isDamagedSoundPlaying = true;
        PlayRandomSound(sounds);
    }

    protected void PlayAttackTypeSound(UnitAttackType attackType)
    {
        if (_isDamagedSoundPlaying)
            return;

        var attackSound = _battleResourceProvider.GetAttackTypeSound(attackType);
        if (attackSound == null)
            return;

        _isDamagedSoundPlaying = true;
        PlaySound(attackSound);
    }

    #endregion
}