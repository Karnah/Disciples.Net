using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.GameObjects;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Models.BattleActions;

namespace Disciples.Scene.Battle.Controllers.UnitActions;

/// <summary>
/// Класс для базовых действий юнита.
/// </summary>
internal abstract class BaseBattleUnitAction : IBattleUnitAction
{
    private readonly BattleContext _context;
    private readonly IBattleGameObjectContainer _battleGameObjectContainer;
    private readonly BattleUnitPortraitPanelController _unitPortraitPanelController;

    private readonly BattleActionContainer _actions = new ();

    /// <summary>
    /// Создать объект типа <see cref="BaseBattleUnitAction" />.
    /// </summary>
    protected BaseBattleUnitAction(
        BattleContext context,
        IBattleGameObjectContainer battleGameObjectContainer,
        BattleUnitPortraitPanelController unitPortraitPanelController)
    {
        _context = context;
        _battleGameObjectContainer = battleGameObjectContainer;
        _unitPortraitPanelController = unitPortraitPanelController;
    }

    /// <inheritdoc />
    public bool IsCompleted { get; private set; }

    /// <inheritdoc />
    public abstract bool ShouldPassTurn { get; protected set; }

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
        if (_actions.IsNoActions)
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

        if (_actions.IsNoActions)
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
    }

    /// <summary>
    /// Обработать начало действия юнита.
    /// </summary>
    protected virtual void ProcessBeginUnitAction(UnitBattleAction unitAction)
    {
        var portrait = _unitPortraitPanelController.GetUnitPortrait(unitAction.TargetUnit);
        if (portrait == null)
            return;

        portrait.ProcessBeginUnitAction(unitAction);
    }

    /// <summary>
    /// Обработать завершение действия юнита.
    /// </summary>
    protected virtual void ProcessCompletedUnitAction(UnitBattleAction unitAction)
    {
        // Если юнит умер, то превращаем его в кучу костей.
        if (unitAction.UnitActionType == UnitActionType.Dying)
        {
            unitAction.TargetUnit.UnitState = BattleUnitState.Dead;
        }

        // На юнита наложен эффект.
        if (unitAction.UnitActionType == UnitActionType.UnderEffect)
        {
            var effectAction = (EffectUnitBattleAction)unitAction;
            effectAction.TargetUnit.Unit.Effects.AddBattleEffect(
                new UnitBattleEffect(AttackClassToEffectType(effectAction.AttackType), effectAction.RoundDuration, effectAction.Power));
        }

        _unitPortraitPanelController
            .GetUnitPortrait(unitAction.TargetUnit)
            ?.ProcessCompletedUnitAction();
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
                AddAction(new GetHitUnitBattleAction(targetUnit, power, attackClass));

                break;
            }

            case AttackResult.Heal:
            {
                var healPower = attackResult.Power!.Value;
                var attackClass = attackResult.AttackType!.Value;

                targetUnit.Unit.HitPoints += healPower;
                AddAction(new GetHitUnitBattleAction(targetUnit, healPower, attackClass));

                break;
            }

            case AttackResult.Effect:
            {
                var power = attackResult.Power;
                var roundDuration = attackResult.RoundDuration!.Value;
                var attackClass = attackResult.AttackType!.Value;

                targetUnit.Unit.Effects.AddBattleEffect(
                    new UnitBattleEffect(AttackClassToEffectType(attackClass), roundDuration, power));
                AddAction(new EffectUnitBattleAction(targetUnit, attackClass));
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
    /// Получить тип эффекта в зависимости от типа атаки.
    /// </summary>
    protected static UnitBattleEffectType AttackClassToEffectType(UnitAttackType attackType)
    {
        return attackType switch
        {
            UnitAttackType.Poison => UnitBattleEffectType.Poison,
            UnitAttackType.Frostbite => UnitBattleEffectType.Frostbite,
            UnitAttackType.Blister => UnitBattleEffectType.Blister,
            _ => throw new ArgumentOutOfRangeException(nameof(attackType), attackType, null)
        };
    }
}