using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Extensions;
using Disciples.Engine.Models;
using Disciples.Resources.Sounds.Models;
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
    private readonly BattleSoundController _soundController;

    private readonly BattleActionContainer _actions = new ();
    private readonly List<IPlayingSound> _playingSounds = new();

    /// <summary>
    /// Признак, что проигрывается звук типа атаки.
    /// </summary>
    private bool _isAttackSoundPlaying;
    /// <summary>
    /// Признак, что проигрывается звук смерти.
    /// </summary>
    private bool _isDeathSoundPlaying;

    /// <summary>
    /// Создать объект типа <see cref="BaseBattleUnitAction" />.
    /// </summary>
    protected BaseBattleUnitAction(
        BattleContext context,
        IBattleGameObjectContainer battleGameObjectContainer,
        BattleUnitPortraitPanelController unitPortraitPanelController,
        IBattleUnitResourceProvider unitResourceProvider,
        BattleSoundController soundController)
    {
        _context = context;
        _battleGameObjectContainer = battleGameObjectContainer;
        _unitPortraitPanelController = unitPortraitPanelController;
        _unitResourceProvider = unitResourceProvider;
        _soundController = soundController;
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
        _unitPortraitPanelController.ProcessActionsBegin(targetSquadPosition);

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

            foreach (var playingSound in _playingSounds)
            {
                playingSound.Stop();
            }

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

        battleUnit.UnitState = battleUnit.Unit.Effects.IsParalyzed
            ? BattleUnitState.Paralyzed
            : BattleUnitState.Waiting;

        // Обрабатываем смерть юнита.
        if (battleUnit.Unit.HitPoints == 0)
            ProcessUnitDeath(battleUnit);
    }

    /// <summary>
    /// Обработать начало действия юнита.
    /// </summary>
    protected virtual void ProcessBeginUnitAction(UnitBattleAction unitAction)
    {
        if (unitAction.ActionType == UnitActionType.Retreating ||
            unitAction.ActionType == UnitActionType.Attacked && unitAction.AttackType == UnitAttackType.Fear)
        {
            unitAction.TargetUnit.Direction = unitAction.TargetUnit.Direction == BattleDirection.Back
                ? BattleDirection.Face
                : BattleDirection.Back;
            unitAction.TargetUnit.Unit.Effects.IsRetreating = true;
        }

        var portrait = _unitPortraitPanelController.GetUnitPortrait(unitAction.TargetUnit);
        portrait?.ProcessBeginUnitPortraitEvent(unitAction.GetUnitPortraitEventData());

        // При накладывании эффекта, сразу переводим в статус парализовано.
        if (unitAction.ActionType == UnitActionType.Attacked &&
            unitAction.AttackType is UnitAttackType.Paralyze or UnitAttackType.Petrify)
        {
            unitAction.TargetUnit.UnitState = BattleUnitState.Paralyzed;
        }
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

            // Если юнит отступал, то возвращаем его положение на место,
            // Чтобы корректно отображались кости.
            unitAction.TargetUnit.Direction = unitAction.TargetUnit.IsAttacker
                ? BattleDirection.Face
                : BattleDirection.Back;
        }

        // На юнита наложен эффект.
        if (unitAction.AttackType?.IsEffect() == true && !unitAction.IsEffectTriggered)
        {
            var targetUnit = unitAction.TargetUnit.Unit;
            targetUnit.Effects.AddBattleEffect(
                new UnitBattleEffect(unitAction.AttackType!.Value, unitAction.AttackSource!.Value, unitAction.EffectDuration!, unitAction.EffectDurationControlUnit!, unitAction.Power));

            // Если у юнита изменилась инициатива, то пересматриваем очерёдность ходов.
            if (unitAction.AttackType == UnitAttackType.ReduceInitiative)
            {
                // Если уменьшилась инициатива, то в очередь его засовываем без учёта случайного разброса.
                // В каких-то особых случаях, это уменьшит вероятность того, что у него инициатива станет в ходу больше, чем была.
                _context.UnitTurnQueue.ReorderUnitTurnOrder(targetUnit, targetUnit.Initiative);
            }
        }

        // Если была защита, то удаляем её из списка.
        // TODO Не знаю, корректно это или нет. Но если есть защита и от типа, и от источника, то снимем сразу оба.
        if (unitAction.ActionType == UnitActionType.Ward)
        {
            unitAction.TargetUnit
                .Unit
                .AttackTypeProtections
                .RemoveAll(atp =>
                    atp.UnitAttackType == unitAction.AttackType &&
                    atp.ProtectionCategory == ProtectionCategory.Ward);
            unitAction.TargetUnit
                .Unit
                .AttackSourceProtections
                .RemoveAll(asp =>
                    asp.UnitAttackSource == unitAction.AttackSource &&
                    asp.ProtectionCategory == ProtectionCategory.Ward);
        }

        if (unitAction.AttackType == UnitAttackType.GiveAdditionalAttack)
            _context.UnitTurnQueue.AddUnitAdditionalAttack(unitAction.TargetUnit.Unit);

        if (unitAction.AttackType == UnitAttackType.Revive)
        {
            var targetUnit = unitAction.TargetUnit;
            targetUnit.UnitState = BattleUnitState.Waiting;
            targetUnit.Unit.IsDead = false;
            targetUnit.Unit.IsRevived = true;
            targetUnit.Unit.HitPoints = targetUnit.Unit.MaxHitPoints / 2;
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
                    AddAction(new UnitBattleAction(targetUnit, attackResult));

                break;
            }

            case AttackResult.Attack:
            {
                var attackType = attackResult.AttackType!.Value;
                switch (attackType)
                {
                    case UnitAttackType.Damage:
                    case UnitAttackType.DrainLife:
                    case UnitAttackType.DrainLifeOverflow:
                    {
                        targetUnit.Unit.HitPoints -= attackResult.Power!.Value;
                        targetUnit.UnitState = BattleUnitState.TakingDamage;

                        AddAction(new AnimationBattleAction(targetUnit.AnimationComponent));

                        // Если будет задето несколько юнитов, то звук удара получит только первый из них.
                        if (!_isAttackSoundPlaying)
                        {
                            PlayRandomSound(targetUnit.SoundComponent.Sounds.DamagedSounds);
                            _isAttackSoundPlaying = true;
                        }

                        break;
                    }

                    case UnitAttackType.Heal:
                    {
                        targetUnit.Unit.HitPoints += attackResult.Power!.Value;
                        break;
                    }

                    // В отличие от заморозки и яда, для вспышки выводится анимация при наложении эффекта.
                    case UnitAttackType.Blister:
                    case UnitAttackType.Revive:
                    {
                        var attackTypeAnimationAction = GetAttackTypeAnimationAction(targetUnit, attackType);
                        if (attackTypeAnimationAction != null)
                            AddAction(attackTypeAnimationAction);
                        break;
                    }
                }

                if (!isMainAttack && !_isAttackSoundPlaying)
                {
                    PlayAttackSound(attackType);
                    _isAttackSoundPlaying = true;
                }

                AddAction(new UnitBattleAction(targetUnit, attackResult));
                break;
            }

            case AttackResult.Ward:
            case AttackResult.Immunity:
                AddAction(new UnitBattleAction(targetUnit, attackResult));
                break;

            case AttackResult.Skip:
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }

        // Если у атакующего есть анимация, применяемая к юниту, то добавляем её на сцену.
        // Это требуется только для основной атаки.
        if (isMainAttack)
        {
            var targetAnimationFrames = targetUnit.IsAttacker
                ? attackerUnit.AnimationComponent.BattleUnitAnimation.TargetAnimation?.AttackerDirectionFrames
                : attackerUnit.AnimationComponent.BattleUnitAnimation.TargetAnimation?.DefenderDirectionFrames;
            if (targetAnimationFrames != null)
            {
                var animationPoint = targetUnit.AnimationComponent.AnimationPoint;
                var targetAnimation = _battleGameObjectContainer.AddAnimation(
                    targetAnimationFrames,
                    animationPoint.X,
                    animationPoint.Y,
                    targetUnit.AnimationComponent.Layer + 2,
                    false);
                AddAction(new AnimationBattleAction(targetAnimation.AnimationComponent));
            }
        }
    }

    /// <summary>
    /// Обработать смерть юнита.
    /// </summary>
    protected void ProcessUnitDeath(BattleUnit battleUnit)
    {
        battleUnit.Unit.IsDead = true;
        battleUnit.Unit.Effects.Clear();

        var animationPoint = battleUnit.AnimationComponent.AnimationPoint;
        var deathAnimation = _battleGameObjectContainer.AddAnimation(
            battleUnit.AnimationComponent.BattleUnitAnimation.DeathFrames,
            animationPoint.X,
            animationPoint.Y,
            battleUnit.AnimationComponent.Layer + 2,
            false);
        AddAction(new AnimationBattleAction(deathAnimation.AnimationComponent));
        AddAction(new UnitBattleAction(battleUnit, UnitActionType.Dying));

        if (!_isDeathSoundPlaying)
        {
            var playingDeathSound = _soundController.PlayUnitDeathSound();
            _playingSounds.Add(playingDeathSound);
            _isDeathSoundPlaying = true;
        }
    }

    /// <summary>
    /// Получить анимацию атаки.
    /// </summary>
    protected AnimationBattleAction? GetAttackTypeAnimationAction(BattleUnit battleUnit, UnitAttackType effectAttackType)
    {
        var animationFrames = _unitResourceProvider.GetAttackTypeAnimation(effectAttackType, battleUnit.Unit.UnitType.IsSmall);
        if (animationFrames == null)
            return null;

        var animationPoint = battleUnit.AnimationComponent.AnimationPoint;
        var animation = _battleGameObjectContainer.AddAnimation(
            animationFrames,
            animationPoint.X,
            animationPoint.Y,
            battleUnit.AnimationComponent.Layer + 2,
            false);
        return new AnimationBattleAction(animation.AnimationComponent);
    }

    /// <summary>
    /// Проиграть звук, соответствующий типу атаки (если такой есть).
    /// </summary>
    protected void PlayAttackSound(UnitAttackType attackType)
    {
        var playingSound = _soundController.PlayAttackSound(attackType);
        if (playingSound == null)
            return;

        _playingSounds.Add(playingSound);
    }

    /// <summary>
    /// Проиграть случайный звук.
    /// </summary>
    protected void PlayRandomSound(IReadOnlyList<RawSound> sounds)
    {
        var playingSound = _soundController.PlayRandomSound(sounds);
        if (playingSound == null)
            return;

        _playingSounds.Add(playingSound);
    }
}