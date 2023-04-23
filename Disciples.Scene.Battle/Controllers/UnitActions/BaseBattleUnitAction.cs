using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.GameObjects;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Models.BattleActions;
using Disciples.Scene.Battle.Providers;

namespace Disciples.Scene.Battle.Controllers.UnitActions;

/// <summary>
/// Класс для базовых действий юнита.
/// </summary>
internal abstract class BaseBattleUnitAction : IBattleUnitAction
{
    private readonly BattleContext _context;
    private readonly IBattleGameObjectContainer _battleGameObjectContainer;
    private readonly BattleUnitPortraitPanelController _unitPortraitPanelController;
    private readonly IBattleUnitResourceProvider _unitResourceProvider;

    private readonly BattleActionContainer _actions = new ();

    /// <summary>
    /// Создать объект типа <see cref="BaseBattleUnitAction" />.
    /// </summary>
    protected BaseBattleUnitAction(
        BattleContext context,
        IBattleGameObjectContainer battleGameObjectContainer,
        BattleUnitPortraitPanelController unitPortraitPanelController,
        IBattleUnitResourceProvider unitResourceProvider)
    {
        _context = context;
        _battleGameObjectContainer = battleGameObjectContainer;
        _unitPortraitPanelController = unitPortraitPanelController;
        _unitResourceProvider = unitResourceProvider;
    }

    /// <inheritdoc />
    public bool IsCompleted { get; private set; }

    /// <inheritdoc />
    public abstract bool ShouldPassTurn { get; protected set; }

    /// <summary>
    /// Признак, что вообще в очереди никаких действий.
    /// </summary>
    protected bool IsNoActions => _actions.IsNoActions;

    /// <summary>
    /// Текущий юнит.
    /// </summary>
    protected BattleUnit CurrentBattleUnit => _context.CurrentBattleUnit;

    /// <inheritdoc />
    public void Initialize()
    {
        var targetSquadPosition = GetTargetSquadPosition();
        _unitPortraitPanelController.DisablePanelSwitch(targetSquadPosition);

        InitializeInternal();

        // Если после инициализации нет действий, то больше ничего делать будет не нужно.
        if (IsNoActions)
            IsCompleted = true;
    }

    /// <inheritdoc />
    public void BeforeSceneUpdate()
    {
        _actions.BeforeSceneUpdate(_context.TicksCount);
    }

    /// <inheritdoc />
    public void AfterSceneUpdate()
    {
        foreach (var completedAction in _actions.Completed)
        {
            ProcessCompletedAction(completedAction);
        }

        _actions.AfterSceneUpdate();

        if (IsNoActions)
        {
            IsCompleted = true;
            OnCompleted();
        }
    }

    /// <summary>
    /// Получить отряд, который должен отображаться на панели.
    /// </summary>
    protected abstract BattleSquadPosition GetTargetSquadPosition();

    /// <summary>
    /// Инициализировать действие.
    /// </summary>
    protected abstract void InitializeInternal();

    /// <summary>
    /// Обработать начало действия.
    /// </summary>
    protected abstract void ProcessBeginAction(IBattleAction battleAction);

    /// <summary>
    /// Обработать завершение действия.
    /// </summary>
    protected abstract void ProcessCompletedAction(IBattleAction battleAction);

    /// <summary>
    /// Добавить действие.
    /// </summary>
    protected virtual void AddAction(IBattleAction battleAction)
    {
        _actions.Add(battleAction);
        ProcessBeginAction(battleAction);
    }

    /// <summary>
    /// Обработать завершения всех действий.
    /// </summary>
    protected virtual void OnCompleted()
    {
    }

    /// <summary>
    /// Обработать завершения анимации юнита.
    /// </summary>
    protected virtual void ProcessCompletedBattleUnitAnimation(AnimationBattleAction animationAction)
    {
        if (animationAction.AnimationComponent.GameObject is not BattleUnit battleUnit)
            return;

        battleUnit.UnitState = BattleUnitState.Waiting;

        // Обрабатываем смерть юнита.
        if (battleUnit.Unit.HitPoints == 0)
            ProcessUnitDeath(battleUnit);
    }

    /// <summary>
    /// Обработать начало действия юнита.
    /// </summary>
    protected virtual void ProcessBeginUnitAction(UnitBattleAction unitAction)
    {
        var portrait = _unitPortraitPanelController.GetUnitPortrait(unitAction.TargetUnit);
        if (portrait == null)
            return;

        portrait.ProcessBeginUnitPortraitEvent(unitAction.GetUnitPortraitEventData());
    }

    /// <summary>
    /// Обработать завершение действия юнита.
    /// </summary>
    protected virtual void ProcessCompletedUnitAction(UnitBattleAction unitAction)
    {
        // Если юнит умер, то превращаем его в кучу костей.
        if (unitAction.ActionType == UnitActionType.Dying)
        {
            unitAction.TargetUnit.UnitState = BattleUnitState.Dead;
        }

        // На юнита наложен эффект.
        if (unitAction is EffectUnitBattleAction effectAction)
        {
            effectAction.TargetUnit.Unit.Effects.AddBattleEffect(
                new UnitBattleEffect(AttackClassToEffectType(effectAction.AttackType!.Value), effectAction.RoundDuration, effectAction.Power));
        }

        _unitPortraitPanelController
            .GetUnitPortrait(unitAction.TargetUnit)
            ?.ProcessCompletedUnitPortraitEvent();
    }

    /// <summary>
    /// Обработать результат атаки.
    /// </summary>
    protected void ProcessAttackResult(BattleUnit attackerUnit,
        BattleUnit targetUnit,
        BattleProcessorAttackResult? attackResult,
        bool isMainAttack)
    {
        // Атака не выполнялась, либо еще не умеем обрабатывать данный тип атаки.
        if (attackResult == null)
            return;

        switch (attackResult.AttackResult)
        {
            case AttackResult.Miss:
            {
                // Если промахнулись дополнительно атакой, то "Промах" выводить не нужно.
                if (isMainAttack)
                    AddAction(new UnitBattleAction(targetUnit, UnitActionType.Dodge));

                break;
            }

            case AttackResult.Attack:
            {
                var power = attackResult.Power!.Value;
                var attackClass = attackResult.AttackType!.Value;

                targetUnit.Unit.HitPoints -= power;
                targetUnit.UnitState = BattleUnitState.TakingDamage;

                AddAction(new AnimationBattleAction(targetUnit.AnimationComponent));
                AddAction(new UnitBattleAction(targetUnit, UnitActionType.GetHit, attackClass, power));

                break;
            }

            case AttackResult.Heal:
            {
                var healPower = attackResult.Power!.Value;
                var attackClass = attackResult.AttackType!.Value;

                targetUnit.Unit.HitPoints += healPower;
                AddAction(new UnitBattleAction(targetUnit, UnitActionType.GetHit, attackClass, healPower));

                break;
            }

            case AttackResult.Effect:
            {
                var power = attackResult.Power;
                var roundDuration = attackResult.RoundDuration!.Value;
                var attackClass = attackResult.AttackType!.Value;

                var effectAnimationAction = ShouldShowEffectAnimation(attackClass)
                    ? GetUnitEffectAnimationAction(targetUnit, attackClass)
                    : null;
                if (effectAnimationAction != null)
                    AddAction(effectAnimationAction);

                AddAction(new EffectUnitBattleAction(targetUnit, attackClass, roundDuration, power, effectAnimationAction));
                break;
            }

            default:
                throw new ArgumentOutOfRangeException();
        }

        // Если у атакующего есть анимация, применяемая к юниту, то добавляем её на сцену.
        // Это требуется только для основной атаки.
        var targetUnitAnimation = attackerUnit.AnimationComponent.BattleUnitAnimation.TargetAnimation;
        if (isMainAttack && targetUnitAnimation?.IsSingle == true)
        {
            var targetAnimationFrames = targetUnit.IsAttacker
                ? targetUnitAnimation.AttackerDirectionFrames
                : targetUnitAnimation.DefenderDirectionFrames;
            var targetAnimation = _battleGameObjectContainer.AddAnimation(
                targetAnimationFrames,
                targetUnit.X,
                targetUnit.Y,
                targetUnit.AnimationComponent.Layer + 2,
                false);
            AddAction(new AnimationBattleAction(targetAnimation.AnimationComponent));
        }
    }

    /// <summary>
    /// Обработать смерть юнита.
    /// </summary>
    protected void ProcessUnitDeath(BattleUnit battleUnit)
    {
        battleUnit.Unit.IsDead = true;
        battleUnit.Unit.Effects.Clear();

        var deathAnimation = _battleGameObjectContainer.AddAnimation(
            battleUnit.AnimationComponent.BattleUnitAnimation.DeathFrames,
            battleUnit.X,
            battleUnit.Y,
            battleUnit.AnimationComponent.Layer + 2,
            false);
        AddAction(new AnimationBattleAction(deathAnimation.AnimationComponent));
        AddAction(new UnitBattleAction(battleUnit, UnitActionType.Dying));
    }

    /// <summary>
    /// Получить анимацию эффекта.
    /// </summary>
    protected AnimationBattleAction? GetUnitEffectAnimationAction(BattleUnit battleUnit, UnitAttackType effectAttackType)
    {
        // TODO Анимации может не быть. Например, для паралича.
        var animationFrames = _unitResourceProvider.GetEffectAnimation(AttackClassToEffectType(effectAttackType), battleUnit.Unit.UnitType.IsSmall);
        var animation = _battleGameObjectContainer.AddAnimation(
            animationFrames,
            battleUnit.X,
            battleUnit.Y,
            battleUnit.AnimationComponent.Layer + 2,
            false);
        return new AnimationBattleAction(animation.AnimationComponent);
    }

    /// <summary>
    /// Получить тип эффекта в зависимости от типа атаки.
    /// </summary>
    private static UnitBattleEffectType AttackClassToEffectType(UnitAttackType attackType)
    {
        return attackType switch
        {
            UnitAttackType.Poison => UnitBattleEffectType.Poison,
            UnitAttackType.Frostbite => UnitBattleEffectType.Frostbite,
            UnitAttackType.Blister => UnitBattleEffectType.Blister,
            _ => throw new ArgumentOutOfRangeException(nameof(attackType), attackType, null)
        };
    }

    /// <summary>
    /// Необходимо ли показывать анимацию при наложении эффекта.
    /// </summary>
    /// <remarks>
    /// Некоторые эффекты (например, яд и обморожении) имеют анимацию срабатывания эффекта.
    /// Но она не используется при наложении эффекта.
    /// </remarks>
    private static bool ShouldShowEffectAnimation(UnitAttackType attackType)
    {
        return attackType == UnitAttackType.Blister;
    }
}