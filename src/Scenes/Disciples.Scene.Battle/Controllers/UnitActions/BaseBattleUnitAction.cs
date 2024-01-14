using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
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
    private bool _isAttackSoundPlaying = false;
    /// <summary>
    /// Признак, что проигрывается звук смерти.
    /// </summary>
    private bool _isDeathSoundPlaying = false;

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
        var portrait = _unitPortraitPanelController.GetUnitPortrait(unitAction.TargetUnit);
        portrait?.ProcessBeginUnitPortraitEvent(unitAction.GetUnitPortraitEventData());

        // При накладывании эффекта, сразу переводим в статус парализовано.
        if (unitAction.ActionType == UnitActionType.UnderEffect &&
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
        if (unitAction is EffectUnitBattleAction effectAction)
        {
            effectAction.TargetUnit.Unit.Effects.AddBattleEffect(
                new UnitBattleEffect(effectAction.AttackType!.Value, effectAction.AttackSource!.Value, effectAction.Duration, effectAction.Power));
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
                    AddAction(new UnitBattleAction(targetUnit, UnitActionType.Miss));

                break;
            }

            case AttackResult.Attack:
            {
                var power = attackResult.Power!.Value;
                var attackClass = attackResult.AttackType!.Value;

                targetUnit.Unit.HitPoints -= power;
                targetUnit.UnitState = BattleUnitState.TakingDamage;

                AddAction(new AnimationBattleAction(targetUnit.AnimationComponent));
                AddAction(new UnitBattleAction(targetUnit, UnitActionType.Damaged, attackClass, power));

                // Если будет задето несколько юнитов, то звук удара получит только первый из них.
                if (!_isAttackSoundPlaying)
                {
                    PlayRandomSound(targetUnit.SoundComponent.Sounds.DamagedSounds);
                    _isAttackSoundPlaying = true;
                }

                break;
            }

            case AttackResult.Heal:
            {
                var healPower = attackResult.Power!.Value;
                var attackClass = attackResult.AttackType!.Value;

                targetUnit.Unit.HitPoints += healPower;
                AddAction(new UnitBattleAction(targetUnit, UnitActionType.Healed, attackClass, healPower));

                if (!isMainAttack && !_isAttackSoundPlaying)
                {
                    PlayAttackSound(attackClass);
                    _isAttackSoundPlaying = true;
                }

                break;
            }

            case AttackResult.Effect:
            {
                var power = attackResult.Power;
                var effectDuration = attackResult.EffectDuration!;
                var attackClass = attackResult.AttackType!.Value;
                var attackSource = attackResult.AttackSource!.Value;

                var effectAnimationAction = ShouldShowEffectAnimation(attackClass)
                    ? GetUnitEffectAnimationAction(targetUnit, attackClass)
                    : null;
                if (effectAnimationAction != null)
                    AddAction(effectAnimationAction);

                AddAction(new EffectUnitBattleAction(targetUnit, attackClass, attackSource, effectDuration, power, effectAnimationAction));

                if (!isMainAttack && !_isAttackSoundPlaying)
                {
                    PlayAttackSound(attackClass);
                    _isAttackSoundPlaying = true;
                }

                break;
            }

            case AttackResult.Ward:
                AddAction(new UnitBattleAction(targetUnit, UnitActionType.Ward, attackResult.AttackType!.Value, attackSource: attackResult.AttackSource!.Value));
                break;

            case AttackResult.Immunity:
                AddAction(new UnitBattleAction(targetUnit, UnitActionType.Immunity));
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
    /// Получить анимацию эффекта.
    /// </summary>
    protected AnimationBattleAction? GetUnitEffectAnimationAction(BattleUnit battleUnit, UnitAttackType effectAttackType)
    {
        // TODO Анимации может не быть. Например, для паралича.
        var animationFrames = _unitResourceProvider.GetEffectAnimation(effectAttackType, battleUnit.Unit.UnitType.IsSmall);
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