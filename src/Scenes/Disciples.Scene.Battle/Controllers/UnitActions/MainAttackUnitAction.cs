using Disciples.Common.Models;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Extensions;
using Disciples.Scene.Battle.Constants;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.GameObjects;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Models.BattleActions;
using Disciples.Scene.Battle.Providers;

namespace Disciples.Scene.Battle.Controllers.UnitActions;

/// <summary>
/// Основная атаку одного юнита на другого.
/// </summary>
internal class MainAttackUnitAction : BaseBattleUnitAction
{
    private readonly BattleContext _context;
    private readonly BattleProcessor _battleProcessor;
    private readonly IBattleGameObjectContainer _battleGameObjectContainer;
    private readonly IBattleUnitResourceProvider _battleUnitResourceProvider;
    private readonly BattleUnitActionController _unitActionController;

    /// <summary>
    /// Создать объект типа <see cref="MainAttackUnitAction" />.
    /// </summary>
    public MainAttackUnitAction(BattleContext context,
        BattleProcessor battleProcessor,
        IBattleGameObjectContainer battleGameObjectContainer,
        BattleUnitPortraitPanelController unitPortraitPanelController,
        IBattleUnitResourceProvider unitResourceProvider,
        BattleUnit targetBattleUnit,
        BattleSoundController soundController,
        BattleUnitActionController unitActionController
        ) : base(context, battleGameObjectContainer, unitPortraitPanelController, unitResourceProvider, soundController)
    {
        _context = context;
        _battleProcessor = battleProcessor;
        _battleGameObjectContainer = battleGameObjectContainer;
        _battleUnitResourceProvider = unitResourceProvider;
        _unitActionController = unitActionController;

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

        AddAction(new MainAttackBattleAction(CurrentBattleUnit, TargetBattleUnit));
        AddAction(new MainAttackSoundBattleAction(CurrentBattleUnit));
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
            case MainAttackSoundBattleAction mainAttackSoundAction:
                ProcessCompletedMainAttackSoundAction(mainAttackSoundAction);
                return;

            case MainAttackBattleAction mainAttackAction:
                ProcessCompletedMainAttackAction(mainAttackAction);
                return;

            case AnimationBattleAction animationAction:
                ProcessCompletedBattleUnitAnimation(animationAction);
                return;

            case UnitBattleAction unitBattleAction:
                ProcessCompletedUnitAction(unitBattleAction);
                return;
        }
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
    /// Обработать завершения задержки для проигрывания звука атаки юнита.
    /// </summary>
    private void ProcessCompletedMainAttackSoundAction(MainAttackSoundBattleAction mainAttackSoundAction)
    {
        PlayRandomSound(mainAttackSoundAction.AttackSounds);
    }

    /// <summary>
    /// Обработать завершение базовой атаки юнита.
    /// </summary>
    private void ProcessCompletedMainAttackAction(MainAttackBattleAction mainAttackAction)
    {
        var currentUnitAnimation = CurrentBattleUnit.AnimationComponent;
        var currentUnit = CurrentBattleUnit.Unit;
        var targetBattleUnits = currentUnit.UnitType.MainAttack.Reach == UnitAttackReach.All
            ? GetUnitBattleSquad(mainAttackAction.Target)
            : new[] { mainAttackAction.Target };

        var secondaryAttackUnits = new List<BattleUnit>();
        var hasSuccessAttack = false;
        var totalDamage = 0;

        foreach (var targetBattleUnit in targetBattleUnits)
        {
            var attackResult = _battleProcessor.ProcessMainAttack(currentUnit, targetBattleUnit.Unit);
            if (attackResult != null)
            {
                ProcessAttackResult(CurrentBattleUnit, targetBattleUnit, attackResult, true);

                if (attackResult.AttackResult == AttackResult.Attack && attackResult.AttackType!.Value.IsDirectDamage())
                    totalDamage += attackResult.Power!.Value;
            }

            var isSuccessAttack = attackResult?.AttackResult is AttackResult.Attack or AttackResult.Skip;
            hasSuccessAttack |= isSuccessAttack;

            // Сразу добавляем действие второй атаки, если первая была успешная.
            if (currentUnit.UnitType.SecondaryAttack != null && isSuccessAttack)
                secondaryAttackUnits.Add(targetBattleUnit);
        }

        // Лечение от эффекта вампиризма проявляется сразу.
        if (currentUnit.UnitType.MainAttack.AttackType is UnitAttackType.DrainLife or UnitAttackType.DrainLifeOverflow)
        {
            var squad = CurrentBattleUnit.IsAttacker
                ? _context.AttackingSquad
                : _context.DefendingSquad;
            var drainLifeHealUnits = _battleProcessor.ProcessDrainLifeHeal(currentUnit, squad, totalDamage);
            foreach (var drainLifeHealUnit in drainLifeHealUnits)
            {
                drainLifeHealUnit.TargetUnit.HitPoints += drainLifeHealUnit.HealPower;

                var targetBattleUnit = _context.GetBattleUnit(drainLifeHealUnit.TargetUnit);
                var animationPoint = targetBattleUnit.AnimationComponent.AnimationPoint;
                var drainLifeHealAnimation = _battleGameObjectContainer.AddAnimation(
                    _battleUnitResourceProvider.DrainLifeHealAnimationFrames,
                    animationPoint.X,
                    animationPoint.Y,
                    targetBattleUnit.AnimationComponent.Layer + 2,
                    false);
                AddAction(new AnimationBattleAction(drainLifeHealAnimation.AnimationComponent));
            }
        }

        // Если есть анимация, применяемая на площадь, то добавляем её на сцену.
        if (currentUnitAnimation.BattleUnitAnimation.TargetAnimation?.AreaFrames != null)
        {
            // Центр анимации будет приходиться на середину между первым и вторым рядом.
            var isTargetAttacker = targetBattleUnits.First().IsAttacker;
            var squad = isTargetAttacker
                ? _context.AttackingBattleSquad
                : _context.DefendingBattleSquad;
            var backPosition = squad.GetUnitPosition(UnitSquadLinePosition.Back, UnitSquadFlankPosition.Center);
            var frontPosition = squad.GetUnitPosition(UnitSquadLinePosition.Front, UnitSquadFlankPosition.Center);
            var point = new PointD(
                            (backPosition.X + frontPosition.X) / 2 + BattleUnit.SmallBattleUnitAnimationOffset.X,
                            (backPosition.Y + frontPosition.Y) / 2 + BattleUnit.SmallBattleUnitAnimationOffset.Y);
            var areaAnimation = _battleGameObjectContainer.AddAnimation(
                currentUnitAnimation.BattleUnitAnimation.TargetAnimation.AreaFrames,
                point.X,
                point.Y,
                isTargetAttacker
                    ? BattleLayers.ABOVE_ALL_ATTACKER_UNITS_LAYER
                    : BattleLayers.ABOVE_ALL_DEFENDER_UNITS_LAYER,
                false);
            AddAction(new AnimationBattleAction(areaAnimation.AnimationComponent));
        }

        // В любом случае дожидаемся завершения анимации атаки.
        AddAction(new AnimationBattleAction(currentUnitAnimation));

        // Если было хотя бы одно успешное попадание, то добавляем звук удара.
        if (hasSuccessAttack)
            PlayRandomSound(CurrentBattleUnit.SoundComponent.Sounds.HitTargetSounds);

        // Если у атакующего юнита есть вторая атака и есть хотя бы одно успешное попадание, добавляем обработки второй атаки.
        // Она начнёт выполняться позже, после завершения всех анимаций, связанных с первой.
        if (secondaryAttackUnits.Count > 0)
            _unitActionController.BeginSecondaryAttack(CurrentBattleUnit, secondaryAttackUnits, ShouldPassTurn);
    }

    /// <summary>
    /// Получить отряд указанного юнита.
    /// </summary>
    private IReadOnlyList<BattleUnit> GetUnitBattleSquad(BattleUnit battleUnit)
    {
        var squad = battleUnit.IsAttacker
            ? _context.AttackingSquad
            : _context.DefendingSquad;
        return squad
            .Units
            .Select(u => _context.GetBattleUnit(u))
            .ToList();
    }
}