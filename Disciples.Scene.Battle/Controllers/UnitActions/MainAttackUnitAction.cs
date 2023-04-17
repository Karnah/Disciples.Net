using Disciples.Engine.Common.Enums.Units;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.GameObjects;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Models.BattleActions;

namespace Disciples.Scene.Battle.Controllers.UnitActions;

/// <summary>
/// Основная атаку одного юнита на другого.
/// </summary>
internal class MainAttackUnitAction : BaseBattleUnitAction
{
    /// <summary>
    /// Слой, который перекрывает всех юнитов.
    /// </summary>
    private const int ABOVE_ALL_UNITS_LAYER = 100 * 4;

    private readonly BattleContext _context;
    private readonly BattleProcessor _battleProcessor;
    private readonly IBattleGameObjectContainer _battleGameObjectContainer;
    private readonly BattleUnitActionController _unitActionController;

    /// <summary>
    /// Создать объект типа <see cref="MainAttackUnitAction" />.
    /// </summary>
    public MainAttackUnitAction(BattleContext context,
        BattleProcessor battleProcessor,
        IBattleGameObjectContainer battleGameObjectContainer,
        BattleUnitPortraitPanelController unitPortraitPanelController,
        BattleUnit targetBattleUnit,
        BattleUnitActionController unitActionController
        ) : base(context, battleGameObjectContainer, unitPortraitPanelController)
    {
        _context = context;
        _battleProcessor = battleProcessor;
        _battleGameObjectContainer = battleGameObjectContainer;
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
    /// Обработать завершение базовой атаки юнита.
    /// </summary>
    private void ProcessCompletedMainAttackAction(MainAttackBattleAction mainAttackAction)
    {
        var currentUnitAnimation = CurrentBattleUnit.AnimationComponent;
        var currentUnit = CurrentBattleUnit.Unit;
        var targetBattleUnits = currentUnit.UnitType.MainAttack.Reach == UnitAttackReach.All
            ? GetUnitBattleSquad(mainAttackAction.Target)
            : new[] { mainAttackAction.Target };

        // Некоторые вторые атаки, например, выпить жизненную силу обрабатываются особым образом.
        // Мы должны посчитать весь урон, который нанесли первой атакой, а потом сделать целями других юнитов.
        var unitSecondAttack = currentUnit.UnitType.SecondaryAttack;
        bool shouldCalculateDamage = false;
        int damage = 0;

        if (unitSecondAttack?.AttackType is UnitAttackType.DrainOverflow or UnitAttackType.Doppelganger)
        {
            shouldCalculateDamage = true;
        }

        var secondaryAttackUnits = new List<BattleUnit>();

        foreach (var targetBattleUnit in targetBattleUnits)
        {
            var attackResult = _battleProcessor.ProcessMainAttack(currentUnit, targetBattleUnit.Unit);

            // Атака не выполнялась, либо еще не умеем обрабатывать данный тип атаки.
            if (attackResult == null)
                continue;

            ProcessAttackResult(CurrentBattleUnit, targetBattleUnit, attackResult, true);

            if (attackResult.AttackResult == AttackResult.Attack)
                damage += attackResult.Power!.Value;

            // Сразу добавляем действие второй атаки, если первая была успешная.
            var isSuccessAttack = attackResult.AttackResult is not AttackResult.Miss;
            if (unitSecondAttack != null && isSuccessAttack && !shouldCalculateDamage)
                secondaryAttackUnits.Add(targetBattleUnit);
        }

        // Если есть анимация, применяемая на площадь, то добавляем её на сцену.
        if (currentUnitAnimation.BattleUnitAnimation.TargetAnimation?.IsSingle == false)
        {
            // Центр анимации будет приходиться на середину между первым и вторым рядом.
            var isTargetAttacker = targetBattleUnits.First().IsAttacker;
            var (x, y) = BattleUnit.GetSceneUnitPosition(isTargetAttacker, 0.5, 1);

            var targetAnimationFrames = isTargetAttacker
                ? currentUnitAnimation.BattleUnitAnimation.TargetAnimation.AttackerDirectionFrames
                : currentUnitAnimation.BattleUnitAnimation.TargetAnimation.DefenderDirectionFrames;
            var areaAnimation = _battleGameObjectContainer.AddAnimation(
                targetAnimationFrames,
                x,
                y,
                ABOVE_ALL_UNITS_LAYER,
                false);
            AddAction(new AnimationBattleAction(areaAnimation.AnimationComponent));
        }

        // В любом случае дожидаемся завершения анимации атаки.
        AddAction(new AnimationBattleAction(currentUnitAnimation));

        // Если у атакующего юнита есть вторая атака и есть хотя бы одно успешное попадание, добавляем обработки второй атаки.
        // Она начнёт выполняться позже, после завершения всех анимаций, связанных с первой.
        if (secondaryAttackUnits.Count > 0)
            _unitActionController.BeginSecondaryAttack(CurrentBattleUnit, secondaryAttackUnits, damage, ShouldPassTurn);

        // TODO Добавить обработка выпить жизнь.
        // По идее, нужно будет разделить урон на каждого юнита.
        // Но если юнит здоров, то делить нужно между оставшимися.
        if (shouldCalculateDamage)
        {
        }
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