using Disciples.Common.Models;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Constants;
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
/// Контроллер для основной атаки юнита.
/// </summary>
internal class MainAttackActionController : BaseAttackActionController
{
    private readonly BattleContext _context;
    private readonly BattleSoundController _soundController;
    private readonly IBattleGameObjectContainer _battleGameObjectContainer;
    private readonly BattleProcessor _battleProcessor;
    private readonly BattleActionFactory _actionFactory;

    private MainAttackResult _mainAttackResult = null!;
    private bool _isAnimationAndSoundDisabled;

    /// <summary>
    /// Создать объект типа <see cref="MainAttackActionController" />.
    /// </summary>
    public MainAttackActionController(
        BattleContext context,
        BattleUnitPortraitPanelController unitPortraitPanelController,
        BattleSoundController soundController,
        IBattleGameObjectContainer battleGameObjectContainer,
        IBattleUnitResourceProvider unitResourceProvider,
        BattleProcessor battleProcessor,
        BattleBottomPanelController bottomPanelController,
        BattleActionFactory actionFactory,
        BattleSquadPosition targetSquadPosition,
        UnitSquadPosition targetUnitPosition
        ) : base(context, unitPortraitPanelController, soundController, battleGameObjectContainer, unitResourceProvider, battleProcessor, bottomPanelController)
    {
        _context = context;
        _soundController = soundController;
        _battleGameObjectContainer = battleGameObjectContainer;
        _battleProcessor = battleProcessor;
        _actionFactory = actionFactory;

        TargetSquadPosition = targetSquadPosition;
        TargetUnitPosition = targetUnitPosition;
    }

    /// <summary>
    /// Целевой отряд.
    /// </summary>
    public BattleSquadPosition TargetSquadPosition { get; }

    /// <summary>
    /// Цель атаки.
    /// </summary>
    /// <remarks>
    /// Практически для всех юнитов - это атакуемый юнит.
    /// Но для призывателя это может быть пустой клеткой.
    /// </remarks>
    public UnitSquadPosition TargetUnitPosition { get; }

    /// <inheritdoc />
    public override bool ShouldPassTurn { get; protected set; }

    /// <inheritdoc />
    protected override BattleSquadPosition? GetTargetSquadPosition()
    {
        return TargetSquadPosition;
    }

    /// <inheritdoc />
    protected override void InitializeInternal()
    {
        // Если это первая атака юнита, который атакует дважды, то ход передавать не нужно.
        // Во всех остальных случаях, будет ход следующего юнита.
        var isFirstAttack = CurrentBattleUnit.Unit.UnitType.IsAttackTwice && !_context.IsSecondAttack;
        ShouldPassTurn = !isFirstAttack;

        var targetSquad = TargetSquadPosition == BattleSquadPosition.Attacker
            ? _battleProcessor.AttackingSquad
            : _battleProcessor.DefendingSquad;
        _mainAttackResult = _battleProcessor.ProcessMainAttack(targetSquad, TargetUnitPosition);

        // Если юнит имеют альтернативную атаку, то анимация атаки/звук относятся именно к ней.
        // Основная атака в таком случае использует стандартную для данного типа атаки анимацию.
        // Юниты с альтернативной атакой - "Доппельгангер" и "Повелитель волков",
        // Своей основной атакой превращают себя в другого юнита.
        _isAnimationAndSoundDisabled = CurrentBattleUnit.Unit.AlternativeAttack != null &&
                                       !_mainAttackResult.IsAlternativeAttackUsed;
        if (_isAnimationAndSoundDisabled)
        {
            ProcessAttack();
            return;
        }

        CurrentBattleUnit.UnitState = BattleUnitState.Attacking;

        var animationComponent = CurrentBattleUnit.AnimationComponent;
        var sounds = CurrentBattleUnit.SoundComponent.Sounds;

        // Проигрываем музыку на определённом кадре анимации атаки.
        AddActionDelay(new BattleAnimationDelay(animationComponent, sounds.BeginAttackSoundFrameIndex - 1,
            PlayAttackSound));

        // Первую часть анимации юнит замахивается для удара, сам удар происходит на кадре EndAttackSoundFrameIndex.
        // Именно в этот момент идёт расчет шанса попадания и урона.
        AddActionDelay(new BattleAnimationDelay(animationComponent, sounds.EndAttackSoundFrameIndex - 1,
            ProcessAttack));
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
        _soundController.PlayRandomAttackSound(CurrentBattleUnit.SoundComponent.Sounds.AttackSounds);
    }

    /// <summary>
    /// Обработать атаку.
    /// </summary>
    private void ProcessAttack()
    {
        foreach (var attackProcessor in _mainAttackResult.AttackProcessors)
            AddProcessorAction(attackProcessor);

        // Если у атакующего юнита есть вторая атака и есть хотя бы одно успешное попадание, добавляем обработки второй атаки.
        // Она начнёт выполняться позже, после завершения всех анимаций, связанных с первой.
        if (_mainAttackResult.SecondaryAttackUnits.Count > 0)
        {
            var secondaryAttackBattleUnits = _mainAttackResult
                .SecondaryAttackUnits
                .Select(_context.GetBattleUnit)
                .ToArray();
            _actionFactory.BeginSecondaryAttack(secondaryAttackBattleUnits, ShouldPassTurn);
        }

        // Дожидаемся завершения анимации атаки, если она была.
        if (!_isAnimationAndSoundDisabled)
            AddActionDelay(new BattleAnimationDelay(CurrentBattleUnit.AnimationComponent, OnAttackerAnimationCompleted));
    }

    /// <inheritdoc />
    protected override void AddSuccessAttackProcessorAction(UnitSuccessAttackProcessor unitSuccessAttackProcessor)
    {
        base.AddSuccessAttackProcessorAction(unitSuccessAttackProcessor);

        if (_isAnimationAndSoundDisabled)
            return;

        AddAttackAreaAnimationAction(CurrentBattleUnit, TargetSquadPosition);
        _soundController.PlayRandomHitSound(CurrentBattleUnit.SoundComponent.Sounds.HitTargetSounds);
    }

    /// <inheritdoc />
    protected override void ProcessBeginSuccessAttack(BattleUnit targetBattleUnit, CalculatedAttackResult attackResult)
    {
        base.ProcessBeginSuccessAttack(targetBattleUnit, attackResult);

        if (_isAnimationAndSoundDisabled)
            return;

        AddAttackTargetAnimationAction(CurrentBattleUnit, targetBattleUnit);
    }

    /// <summary>
    /// Анимация атаки завершена.
    /// </summary>
    private void OnAttackerAnimationCompleted()
    {
        CurrentBattleUnit.UnitState = BattleUnitState.Waiting;
    }

    /// <summary>
    /// Добавить анимацию атаки, которая применяется к площади.
    /// </summary>
    private void AddAttackAreaAnimationAction(BattleUnit attackerBattleUnit, BattleSquadPosition targetSquadPosition)
    {
        var targetAnimation = attackerBattleUnit.AnimationComponent.BattleUnitAnimation.TargetAnimation;
        var areaFrames = targetSquadPosition == BattleSquadPosition.Attacker
            ? targetAnimation?.AttackerAreaFrames
            : targetAnimation?.DefenderAreaFrames;
        if (areaFrames == null)
            return;

        // Центр анимации будет приходиться на середину между первым и вторым рядом.
        var isTargetAttacker = targetSquadPosition == BattleSquadPosition.Attacker;
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
        var animationDelay = new BattleAnimationDelay(areaAnimation.AnimationComponent);
        AddActionDelay(animationDelay);
    }

    /// <summary>
    /// Добавить анимацию атаки, которая применяется к юниту-цели.
    /// </summary>
    private void AddAttackTargetAnimationAction(BattleUnit attackerBattleUnit, BattleUnit targetBattleUnit)
    {
        var attackerTargetAnimation = attackerBattleUnit.AnimationComponent.BattleUnitAnimation.TargetAnimation;
        var targetAnimationFrames = targetBattleUnit.IsAttacker
            ? attackerTargetAnimation?.AttackerUnitFrames
            : attackerTargetAnimation?.DefenderUnitFrames;
        if (targetAnimationFrames == null)
            return;

        var animationPoint = targetBattleUnit.AnimationComponent.AnimationPoint;
        var targetAnimation = _battleGameObjectContainer.AddAnimation(
            targetAnimationFrames,
            animationPoint.X,
            animationPoint.Y,
            targetBattleUnit.AnimationComponent.Layer + 2,
            false);
        var animationDelay = new BattleAnimationDelay(targetAnimation.AnimationComponent);
        AddActionDelay(animationDelay);
    }
}