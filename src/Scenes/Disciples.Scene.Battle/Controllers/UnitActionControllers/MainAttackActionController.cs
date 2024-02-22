using Disciples.Common.Models;
using Disciples.Engine.Common.Enums;
using Disciples.Scene.Battle.Constants;
using Disciples.Scene.Battle.Controllers.UnitActionControllers.Base;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.GameObjects;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Models.BattleActions;
using Disciples.Scene.Battle.Processors;
using Disciples.Scene.Battle.Processors.UnitActionProcessors;
using Disciples.Scene.Battle.Providers;

namespace Disciples.Scene.Battle.Controllers.UnitActionControllers;

/// <summary>
/// Контроллер для основной атаки юнита.
/// </summary>
internal class MainAttackActionController : BaseAttackActionController
{
    private readonly BattleContext _context;
    private readonly IBattleGameObjectContainer _battleGameObjectContainer;
    private readonly BattleProcessor _battleProcessor;
    private readonly BattleUnitActionFactory _unitActionFactory;

    /// <summary>
    /// Создать объект типа <see cref="MainAttackActionController" />.
    /// </summary>
    public MainAttackActionController(
        BattleContext context,
        BattleUnitPortraitPanelController unitPortraitPanelController,
        BattleSoundController soundController,
        IBattleGameObjectContainer battleGameObjectContainer,
        IBattleUnitResourceProvider unitResourceProvider,
        IBattleResourceProvider battleResourceProvider,
        BattleProcessor battleProcessor,
        BattleUnitActionFactory unitActionFactory,
        BattleUnit targetBattleUnit
        ) : base(context, unitPortraitPanelController, soundController, battleGameObjectContainer, unitResourceProvider, battleResourceProvider, battleProcessor)
    {
        _context = context;
        _battleGameObjectContainer = battleGameObjectContainer;
        _battleProcessor = battleProcessor;
        _unitActionFactory = unitActionFactory;

        TargetBattleUnit = targetBattleUnit;
    }

    /// <summary>
    /// Атакуемый юнит.
    /// </summary>
    public BattleUnit TargetBattleUnit { get; }

    /// <inheritdoc />
    public override bool ShouldPassTurn { get; protected set; }

    /// <inheritdoc />
    protected override BattleSquadPosition GetTargetSquadPosition()
    {
        return TargetBattleUnit.SquadPosition;
    }

    /// <inheritdoc />
    protected override void InitializeInternal()
    {
        // Если это первая атака юнита, который атакует дважды, то ход передавать не нужно.
        // Во всех остальных случаях, будет ход следующего юнита.
        var isFirstAttack = CurrentBattleUnit.Unit.UnitType.IsAttackTwice && !_context.IsSecondAttack;
        ShouldPassTurn = !isFirstAttack;

        // Это нельзя выносить в ProcessBeginAction,
        // Так как индекс анимации рассчитывается в конструкторе MainAttackBattleAction.
        CurrentBattleUnit.UnitState = BattleUnitState.Attacking;

        var animationComponent = CurrentBattleUnit.AnimationComponent;
        var sounds = CurrentBattleUnit.SoundComponent.Sounds;

        // Проигрываем музыку на определённом кадре анимации атаки.
        AddAction(new AnimationBattleAction(animationComponent, sounds.BeginAttackSoundFrameIndex - 1, PlayAttackSound));

        // Первую часть анимации юнит замахивается для удара, сам удар происходит на кадре EndAttackSoundFrameIndex.
        // Именно в этот момент идёт расчет шанса попадания и урона.
        AddAction(new AnimationBattleAction(animationComponent, sounds.EndAttackSoundFrameIndex - 1, ProcessAttack));
    }

    /// <inheritdoc />
    protected override void OnCompleted()
    {
        base.OnCompleted();

        // Завершаем первую атаку юнита, который атакует дважды.
        // Если это уже была вторая атака, то нужно скинуть значение.
        if (CurrentBattleUnit.Unit.UnitType.IsAttackTwice)
            _context.IsSecondAttack = !_context.IsSecondAttack;
    }

    /// <summary>
    /// Проиграть звук атаки.
    /// </summary>
    private void PlayAttackSound()
    {
        PlayRandomSound(CurrentBattleUnit.SoundComponent.Sounds.AttackSounds);
    }

    /// <summary>
    /// Обработать атаку.
    /// </summary>
    private void ProcessAttack()
    {
        var secondaryAttackUnits = new List<BattleUnit>();
        var attackProcessorContext = _context.CreateAttackProcessorContext(TargetBattleUnit);
        var processors = _battleProcessor.ProcessMainAttack(attackProcessorContext);
        foreach (var unitActionProcessor in processors)
        {
            AddProcessorAction(unitActionProcessor);
            secondaryAttackUnits.AddRange(unitActionProcessor.SecondaryAttackUnits.Select(_context.GetBattleUnit));
        }

        // Если у атакующего юнита есть вторая атака и есть хотя бы одно успешное попадание, добавляем обработки второй атаки.
        // Она начнёт выполняться позже, после завершения всех анимаций, связанных с первой.
        if (secondaryAttackUnits.Count > 0)
            _unitActionFactory.BeginSecondaryAttack(secondaryAttackUnits, ShouldPassTurn);

        // В любом случае дожидаемся завершения анимации атаки.
        AddAction(new AnimationBattleAction(CurrentBattleUnit.AnimationComponent, OnAttackerAnimationCompleted));
    }

    /// <inheritdoc />
    protected override void AddSuccessAttackProcessorAction(UnitSuccessAttackProcessor unitSuccessAttackProcessor)
    {
        base.AddSuccessAttackProcessorAction(unitSuccessAttackProcessor);

        var areaAnimationAction = GetAttackAreaAnimationAction(CurrentBattleUnit, TargetBattleUnit);
        if (areaAnimationAction != null)
            AddAction(areaAnimationAction);

        PlayRandomSound(CurrentBattleUnit.SoundComponent.Sounds.HitTargetSounds);
    }

    /// <inheritdoc />
    protected override void ProcessBeginActionAttackResult(BattleUnit targetBattleUnit, CalculatedAttackResult attackResult)
    {
        base.ProcessBeginActionAttackResult(targetBattleUnit, attackResult);

        var targetUnitAnimationAction = GetAttackTargetAnimationAction(CurrentBattleUnit, targetBattleUnit);
        if (targetUnitAnimationAction != null)
            AddAction(targetUnitAnimationAction);
    }

    /// <summary>
    /// Анимация атаки завершена.
    /// </summary>
    private void OnAttackerAnimationCompleted()
    {
        CurrentBattleUnit.UnitState = BattleUnitState.Waiting;
    }

    /// <summary>
    /// Получить анимацию атаки, которая применяется к площади.
    /// </summary>
    private AnimationBattleAction? GetAttackAreaAnimationAction(BattleUnit attackerBattleUnit, BattleUnit targetBattleUnit)
    {
        var targetAnimation = attackerBattleUnit.AnimationComponent.BattleUnitAnimation.TargetAnimation;
        var areaFrames = targetBattleUnit.IsAttacker
            ? targetAnimation?.AttackerAreaFrames
            : targetAnimation?.DefenderAreaFrames;
        if (areaFrames == null)
            return null;

        // Центр анимации будет приходиться на середину между первым и вторым рядом.
        var isTargetAttacker = targetBattleUnit.IsAttacker;
        var squad = isTargetAttacker
            ? _context.AttackingBattleSquad
            : _context.DefendingBattleSquad;
        var backPosition = squad.GetUnitPosition(UnitSquadLinePosition.Back, UnitSquadFlankPosition.Center);
        var frontPosition = squad.GetUnitPosition(UnitSquadLinePosition.Front, UnitSquadFlankPosition.Center);
        var point = new PointD(
            (backPosition.X + frontPosition.X) / 2 + BattleUnit.SmallBattleUnitAnimationOffset.X,
            (backPosition.Y + frontPosition.Y) / 2 + BattleUnit.SmallBattleUnitAnimationOffset.Y);
        var areaAnimation = _battleGameObjectContainer.AddAnimation(
            areaFrames,
            point.X,
            point.Y,
            isTargetAttacker
                ? BattleLayers.ABOVE_ALL_ATTACKER_UNITS_LAYER
                : BattleLayers.ABOVE_ALL_DEFENDER_UNITS_LAYER,
            false);
        return new AnimationBattleAction(areaAnimation.AnimationComponent);
    }

    /// <summary>
    /// Получить анимацию атаки, которая применяется к юниту-цели.
    /// </summary>
    private AnimationBattleAction? GetAttackTargetAnimationAction(BattleUnit attackerBattleUnit, BattleUnit targetBattleUnit)
    {
        var attackerTargetAnimation = attackerBattleUnit.AnimationComponent.BattleUnitAnimation.TargetAnimation;
        var targetAnimationFrames = targetBattleUnit.IsAttacker
            ? attackerTargetAnimation?.AttackerUnitFrames
            : attackerTargetAnimation?.DefenderUnitFrames;
        if (targetAnimationFrames == null)
            return null;

        var animationPoint = targetBattleUnit.AnimationComponent.AnimationPoint;
        var targetAnimation = _battleGameObjectContainer.AddAnimation(
            targetAnimationFrames,
            animationPoint.X,
            animationPoint.Y,
            targetBattleUnit.AnimationComponent.Layer + 2,
            false);
        return new AnimationBattleAction(targetAnimation.AnimationComponent);
    }
}