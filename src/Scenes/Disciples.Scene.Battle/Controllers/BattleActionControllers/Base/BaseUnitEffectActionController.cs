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
/// Базовый контроллер для действий, которые могут накладывать/снимать эффекты или наносить урон.
/// </summary>
internal abstract class BaseUnitEffectActionController : BaseBattleActionController
{
    private readonly BattleContext _context;
    private readonly BattleUnitPortraitPanelController _unitPortraitPanelController;
    private readonly BattleSoundController _soundController;
    private readonly IBattleGameObjectContainer _battleGameObjectContainer;
    private readonly IBattleUnitResourceProvider _unitResourceProvider;
    private readonly BattleProcessor _battleProcessor;

    /// <summary>
    /// Очередь из обработчиков эффектов.
    /// </summary>
    /// <remarks>
    /// Используется в трёх случаях:
    /// 1. Для <see cref="BeginUnitTurnController" />, чтобы обработать срабатывание эффекта в начале хода юнита.
    /// 2. При обработке типа атаки <see cref="UnitAttackType.Cure" />, чтобы снимать эффекты с юнитов.
    /// 3. При завершении битвы <see cref="BeforeCompleteBattleActionController" />, чтобы снять все оставшиеся эффекты.
    /// </remarks>
    private readonly Queue<IUnitEffectProcessor> _unitEffectProcessors = new();

    /// <summary>
    /// Признак, что идёт обработка эффекта.
    /// </summary>
    private bool _isUnitEffectProcessing;

    /// <summary>
    /// Создать объект типа <see cref="BaseUnitEffectActionController" />.
    /// </summary>
    protected BaseUnitEffectActionController(
        BattleContext context,
        BattleUnitPortraitPanelController unitPortraitPanelController,
        BattleSoundController soundController,
        IBattleGameObjectContainer battleGameObjectContainer,
        IBattleUnitResourceProvider unitResourceProvider,
        BattleProcessor battleProcessor,
        BattleBottomPanelController bottomPanelController
        ) : base(context, unitPortraitPanelController, bottomPanelController, battleGameObjectContainer, unitResourceProvider)
    {
        _context = context;
        _unitPortraitPanelController = unitPortraitPanelController;
        _soundController = soundController;
        _battleGameObjectContainer = battleGameObjectContainer;
        _unitResourceProvider = unitResourceProvider;
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

                case UnitAttackType.Summon when attackEffectProcessor.EffectResult.NewDuration.IsCompleted:
                {
                    AddProcessorAction(attackEffectProcessor);
                    AddUnitUnsummonAnimationAction(targetBattleUnit);
                    _soundController.PlayUnitUsummonSound();
                    return true;
                }

                case UnitAttackType.ReduceLevel when attackEffectProcessor.EffectResult.NewDuration.IsCompleted:
                case UnitAttackType.Doppelganger when attackEffectProcessor.EffectResult.NewDuration.IsCompleted:
                case UnitAttackType.TransformSelf when attackEffectProcessor.EffectResult.NewDuration.IsCompleted:
                case UnitAttackType.TransformEnemy when attackEffectProcessor.EffectResult.NewDuration.IsCompleted:
                {
                    AddProcessorAction(attackEffectProcessor);
                    AddAttackTypeAnimationAction(targetBattleUnit, attackType);
                    _soundController.PlayAttackTypeSound(attackType);
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
                case UnitAttackType.Summon:
                {
                    RemoveBattleUnit(targetBattleUnit);
                    break;
                }

                case UnitAttackType.ReduceLevel:
                case UnitAttackType.Doppelganger:
                case UnitAttackType.TransformSelf:
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
    protected void ProcessTransformUnit(BattleUnit originalBattleUnit, ITransformedUnit transformedUnit)
    {
        ReplaceUnit(originalBattleUnit, transformedUnit.Unit);
    }

    /// <summary>
    /// Обработать возвращение юнита в исходную форму.
    /// </summary>
    private void ProcessTransformUnitBack(BattleUnit transformedBattleUnit)
    {
        var transformedUnit = (ITransformedUnit)transformedBattleUnit.Unit;
        ReplaceUnit(transformedBattleUnit, transformedUnit.OriginalUnit);
    }

    #endregion

    #region Смерть юнита

    /// <summary>
    /// Обработать смерть юнита.
    /// </summary>
    protected void ProcessUnitDeath(BattleUnit targetBattleUnit)
    {
        var unitDeathProcessor = _battleProcessor.ProcessDeath(targetBattleUnit.Unit);

        AddProcessorAction(unitDeathProcessor);
        AddUnitDeathAnimationAction(targetBattleUnit);
        _soundController.PlayUnitDeathSound();

        // Если юнит был превращён, то сразу возвращаем портрет оригинального юнита.
        // При этом BattleUnit будет сброшен только в конце.
        if (targetBattleUnit.Unit is ITransformedUnit transformedUnit)
        {
            var unitPortrait = _unitPortraitPanelController.GetUnitPortrait(targetBattleUnit);
            unitPortrait?.ChangeUnit(transformedUnit.OriginalUnit);
        }
    }

    /// <summary>
    /// Обработать завершение смерти юнита.
    /// </summary>
    private void OnUnitDeathProcessorCompleted(UnitDeathProcessor unitDeathProcessor, BattleUnit targetBattleUnit)
    {
        unitDeathProcessor.ProcessCompletedAction();

        // Превращаем его в кучу костей.
        targetBattleUnit.UnitState = BattleUnitState.Dead;

        // Если юнит был превращён, то возвращаем ему исходную форму.
        if (targetBattleUnit.Unit is ITransformedUnit)
            ProcessTransformUnitBack(targetBattleUnit);

        // Если юнит призывал других, то после его смерти они будут уничтожены.
        EnqueueEffectProcessors(unitDeathProcessor.UnsummonProcessors);

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
        AddActionDelay(new BattleAnimationDelay(deathAnimation.AnimationComponent, () => OnUnitDeathAnimationCompleted(targetBattleUnit)));
    }

    /// <summary>
    /// Обработать завершение анимации смерти юнита.
    /// </summary>
    private void OnUnitDeathAnimationCompleted(BattleUnit targetBattleUnit)
    {
        // Призванный юнит просто удаляется.
        if (targetBattleUnit.Unit is SummonedUnit)
        {
            RemoveBattleUnit(targetBattleUnit);
            _soundController.PlayUnitUsummonSound();
        }
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

    #endregion
}