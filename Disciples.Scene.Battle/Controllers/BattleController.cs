using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Implementation.Base;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.GameObjects;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Models.BattleActions;

namespace Disciples.Scene.Battle.Controllers;

/// <inheritdoc cref="IBattleController" />
internal class BattleController : BaseSupportLoading, IBattleController
{
    /// <summary>
    /// Слой, который перекрывает всех юнитов.
    /// </summary>
    private const int ABOVE_ALL_UNITS_LAYER = 100 * 4;

    private readonly IBattleGameObjectContainer _battleGameObjectContainer;
    private readonly BattleProcessor _battleProcessor;
    private readonly BattleContext _context;
    private readonly BattleAiProcessor _battleAiProcessor;

    /// <summary>
    /// Очередность хода юнитов.
    /// </summary>
    private Queue<Unit> _turnOrder = new();

    /// <summary>
    /// Очередность хода юнитов, которые выбрали ожидание во время своего хода.
    /// </summary>
    private readonly Stack<Unit> _waitingTurnOrder = new ();

    /// <summary>
    /// Создать объект типа <see cref="BattleController" />.
    /// </summary>
    public BattleController(
        IBattleGameObjectContainer battleGameObjectContainer,
        BattleProcessor battleProcessor,
        BattleContext context,
        BattleAiProcessor battleAiProcessor)
    {
        _battleGameObjectContainer = battleGameObjectContainer;
        _battleProcessor = battleProcessor;
        _context = context;
        _battleAiProcessor = battleAiProcessor;
    }

    /// <inheritdoc />
    public override bool IsSharedBetweenScenes => false;

    /// <summary>
    ///  Юнит, который выполняет свой ход.
    /// </summary>
    private BattleUnit CurrentBattleUnit
    {
        get => _context.CurrentBattleUnit;
        set => _context.CurrentBattleUnit = value;
    }

    /// <summary>
    /// Признак того, что юнит атакует второй раз за текущий ход.
    /// </summary>
    /// <remarks>Актуально только для юнитов, которые бьют дважды за ход.</remarks>
    private bool IsSecondAttack
    {
        get => _context.IsSecondAttack;
        set => _context.IsSecondAttack = value;
    }

    /// <summary>
    /// Признак, что ходит юнит, который "ждал" в этом раунде.
    /// </summary>
    private bool IsWaitingUnitTurn
    {
        get => _context.IsWaitingUnitTurn;
        set => _context.IsWaitingUnitTurn = value;
    }

    /// <summary>
    /// Признак, что битва проходит в автоматическом режиме.
    /// </summary>
    private bool IsAutoBattle => _context.IsAutoBattle;

    /// <summary>
    /// Список всех действий на поле боя.
    /// </summary>
    private BattleActionContainer Actions => _context.Actions;

    /// <inheritdoc />
    public void BeforeSceneUpdate()
    {
        // ProcessBeginAction может добавлять новые действия в New,
        // Поэтому вызываем ToList().
        // TODO Подумать над уменьшением выделения памяти.
        foreach (var newBattleAction in Actions.New.ToList())
        {
            ProcessBeginAction(newBattleAction);
        }

        if (IsAutoBattle && Actions.IsNoActions)
            CheckAndProcessIfAiTurn();
    }

    /// <inheritdoc />
    public void AfterSceneUpdate()
    {
        foreach (var completedAction in Actions.Completed)
        {
            ProcessCompletedBattleAction(completedAction);
        }

        // Если последние действия завершились в этом обновлении,
        // Значит атака закончилась и нужно начинать новый ход.
        if (Actions.IsAllActionsCompletedThisUpdate)
        {
            // Перед тем, как отдать ход следующему юниту мы должны подождать немного времени.
            if (!Actions.Completed.OfType<DelayLastBattleAction>().Any())
            {
                Actions.Add(new DelayLastBattleAction());
                return;
            }

            NextTurn();
        }
    }

    /// <inheritdoc />
    protected override void LoadInternal()
    {
        ArrangeUnits();
        StartNewRound();
    }

    /// <inheritdoc />
    protected override void UnloadInternal()
    {
    }

    /// <summary>
    /// Расставить юнитов по позициям.
    /// </summary>
    private void ArrangeUnits()
    {
        var units = new List<BattleUnit>();

        foreach (var attackSquadUnit in _context.AttackingSquad.Units)
            units.Add(_battleGameObjectContainer.AddBattleUnit(attackSquadUnit, true));

        foreach (var defendSquadUnit in _context.DefendingSquad.Units)
            units.Add(_battleGameObjectContainer.AddBattleUnit(defendSquadUnit, false));

        _context.BattleUnits = units;
    }

    // Начать новый раунд.
    private void StartNewRound()
    {
        _turnOrder = _battleProcessor.GetTurnOrder(_context.AttackingSquad, _context.DefendingSquad);

        IsWaitingUnitTurn = false;
        CurrentBattleUnit = _context.GetBattleUnit(_turnOrder.Dequeue());
        CurrentBattleUnit.Unit.Effects.OnUnitTurn();
        CheckAndProcessIfAiTurn();
    }

    // Передать ход следующему юниту.
    private void NextTurn()
    {
        if (_battleProcessor.IsBattleCompleted(_context.AttackingSquad, _context.DefendingSquad))
        {
            // TODO Также должны быть сняты все эффекты с оставшихся юнитов.
            Actions.Add(new BattleCompletedAction());
            return;
        }

        // Была завершена первая атака юнита, который может атаковать дважды.
        // В этом случае ход остаётся у него.
        if (CurrentBattleUnit.Unit.UnitType.IsAttackTwice && !IsSecondAttack)
        {
            IsSecondAttack = true;
            CheckAndProcessIfAiTurn();
            return;
        }

        IsSecondAttack = false;

        // Очередь из основных ходов юнитов.
        do
        {
            if (_turnOrder.TryDequeue(out var nextUnit) && !nextUnit.IsDead)
            {
                CurrentBattleUnit = _context.GetBattleUnit(nextUnit);
                CurrentBattleUnit.Unit.Effects.OnUnitTurn();
                CheckAndProcessIfAiTurn();

                return;
            }
        } while (_turnOrder.Count > 0);


        // Стек из юнитов, которые "ждали".
        if (_waitingTurnOrder.Count > 0)
        {
            IsWaitingUnitTurn = true;

            do
            {
                if (_waitingTurnOrder.TryPop(out var nextUnit) && !nextUnit.IsDead)
                {
                    CurrentBattleUnit = _context.GetBattleUnit(nextUnit);
                    CheckAndProcessIfAiTurn();

                    // TODO Посмотреть как работают эффекты. Возможно, эффект работает только один раз за раунд.
                    // Т.е. если эффект уже отработал - больше не применяется. Но если наложили после ожидания, должен отработать.

                    return;
                }
            } while (_waitingTurnOrder.Count > 0);
        }

        StartNewRound();
    }

    /// <summary>
    /// Обработать ход искусственного интеллекта.
    /// </summary>
    private void CheckAndProcessIfAiTurn()
    {
        if (!CurrentBattleUnit.Unit.Player.IsComputer && !IsAutoBattle)
            return;

        var attackingSquad = CurrentBattleUnit.IsAttacker
            ? _context.AttackingSquad
            : _context.DefendingSquad;
        var defendingSquad = CurrentBattleUnit.IsAttacker
            ? _context.DefendingSquad
            : _context.AttackingSquad;
        var command = _battleAiProcessor.GetAiCommand(
            CurrentBattleUnit.Unit,
            attackingSquad,
            defendingSquad);

        IBattleAction action = command.CommandType switch
        {
            BattleCommandType.Attack => new BeginAttackUnitBattleAction(_context.GetBattleUnit(command.Target!)),
            BattleCommandType.Defend => new DefendBattleAction(),
            BattleCommandType.Wait => new WaitingBattleAction(),
            _ => throw new ArgumentOutOfRangeException()
        };

        Actions.AddDelayed(action);
    }

    /// <summary>
    /// Обработать начало нового действия.
    /// </summary>
    private void ProcessBeginAction(IBattleAction battleAction)
    {
        if (battleAction is BeginAttackUnitBattleAction beginAttackUnitAction)
        {
            ProcessBeginAttackUnitAction(beginAttackUnitAction);
            return;
        }

        if (battleAction is DefendBattleAction)
        {
            ProcessDefendUnitAction();
            return;
        }

        if (battleAction is WaitingBattleAction)
        {
            ProcessWaitingUnitAction();
            return;
        }

        if (battleAction is BeginSecondaryAttackBattleAction secondaryAttackAction)
        {
            ProcessBeginSecondaryAttackAction(secondaryAttackAction);
            return;
        }
    }

    /// <summary>
    /// Обработать начало воздействия на юнита.
    /// </summary>
    private void ProcessBeginAttackUnitAction(BeginAttackUnitBattleAction beginAttackUnitAction)
    {
        CurrentBattleUnit.UnitState = BattleUnitState.Attacking;

        var targetBattleUnit = beginAttackUnitAction.TargetBattleUnit;
        Actions.Add(new MainAttackBattleAction(CurrentBattleUnit, targetBattleUnit));
    }

    /// <summary>
    /// Обработать защиту юнита.
    /// </summary>
    private void ProcessDefendUnitAction()
    {
        IsSecondAttack = true;
        Actions.Add(new UnitBattleAction(CurrentBattleUnit, UnitActionType.Defend));
    }

    /// <summary>
    /// Обработать ожидание юнита.
    /// </summary>
    private void ProcessWaitingUnitAction()
    {
        IsSecondAttack = true;
        _waitingTurnOrder.Push(CurrentBattleUnit.Unit);
        Actions.Add(new UnitBattleAction(CurrentBattleUnit, UnitActionType.Waiting));
    }

    /// <summary>
    /// Обработать завершение дополнительной атаки юнита.
    /// </summary>
    private void ProcessBeginSecondaryAttackAction(BeginSecondaryAttackBattleAction beginSecondaryAttackAction)
    {
        var attacker = beginSecondaryAttackAction.Attacker;
        var target = beginSecondaryAttackAction.Target;
        var power = beginSecondaryAttackAction.Power;
        var attackResult = _battleProcessor.ProcessSecondaryAttack(attacker.Unit, target.Unit, power);

        ProcessAttackResult(attacker, target, attackResult, false);
    }

    /// <summary>
    /// Обработать завершение действия 
    /// </summary>
    private void ProcessCompletedBattleAction(IBattleAction battleAction)
    {
        if (battleAction is MainAttackBattleAction mainAttackAction)
        {
            ProcessCompletedMainAttackAction(mainAttackAction);
            return;
        }

        if (battleAction is AnimationBattleAction animationAction)
        {
            ProcessCompletedBattleUnitAnimation(animationAction);
            return;
        }

        if (battleAction is UnitBattleAction unitBattleAction)
        {
            ProcessCompletedUnitAction(unitBattleAction);
            return;
        }
    }

    /// <summary>
    /// Обработать завершение базовой атаки юнита.
    /// </summary>
    private void ProcessCompletedMainAttackAction(MainAttackBattleAction mainAttackBattleAction)
    {
        var currentUnitAnimation = CurrentBattleUnit.AnimationComponent;
        var currentUnit = CurrentBattleUnit.Unit;
        var targetBattleUnits = currentUnit.UnitType.MainAttack.Reach == UnitAttackReach.All
            ? GetUnitBattleSquad(mainAttackBattleAction.Target)
            : new[] { mainAttackBattleAction.Target };

        // Некоторые вторые атаки, например, выпить жизненную силу обрабатываются особым образом.
        // Мы должны посчитать весь урон, который нанесли первой атакой, а потом сделать целями других юнитов.
        var unitSecondAttack = currentUnit.UnitType.SecondaryAttack;
        bool shouldCalculateDamage = false;
        int damage = 0;
        if (unitSecondAttack?.AttackType == UnitAttackType.DrainOverflow ||
            unitSecondAttack?.AttackType == UnitAttackType.Doppelganger)
        {
            shouldCalculateDamage = true;
        }

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
            // Однако, её последствия будут только после завершения всех анимаций, которые связаны с первой.
            var isSuccessAttack = attackResult.AttackResult is not AttackResult.Miss;
            if (unitSecondAttack != null && isSuccessAttack && !shouldCalculateDamage)
                Actions.AddDelayed(new BeginSecondaryAttackBattleAction(CurrentBattleUnit, targetBattleUnit));
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
            Actions.Add(new AnimationBattleAction(areaAnimation.AnimationComponent));
        }

        // В любом случае дожидаемся завершения анимации атаки.
        Actions.Add(new AnimationBattleAction(currentUnitAnimation));

        // TODO Добавить обработка выпить жизнь.
        // По идее, нужно будет разделить урон на каждого юнита.
        // Но если юнит здоров, то делить нужно между оставшимися.
        if (shouldCalculateDamage)
        {
        }
    }

    /// <summary>
    /// Обработать завершения анимации атаки юнита.
    /// </summary>
    private void ProcessCompletedBattleUnitAnimation(AnimationBattleAction animationAction)
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
            Actions.Add(new AnimationBattleAction(deathAnimation.AnimationComponent));
            Actions.Add(new UnitBattleAction(battleUnit, UnitActionType.Dying));
        }
    }

    /// <summary>
    /// Обработать завершение действия юнита.
    /// </summary>
    private static void ProcessCompletedUnitAction(UnitBattleAction unitAction)
    {
        // Если юнит защитился, то добавляем эффект.
        if (unitAction.UnitActionType == UnitActionType.Defend)
        {
            unitAction.TargetUnit.Unit.Effects.AddBattleEffect(new UnitBattleEffect(UnitBattleEffectType.Defend, 1));
        }

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
    }

    /// <summary>
    /// Обработать результат атаки.
    /// </summary>
    private void ProcessAttackResult(BattleUnit attackerUnit,
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
                    Actions.Add(new UnitBattleAction(targetUnit, UnitActionType.Dodge));

                break;
            }

            case AttackResult.Attack:
            {
                var power = attackResult.Power!.Value;
                var attackClass = attackResult.AttackType!.Value;

                targetUnit.Unit.HitPoints -= power;
                targetUnit.UnitState = BattleUnitState.TakingDamage;

                Actions.Add(new AnimationBattleAction(targetUnit.AnimationComponent));
                Actions.Add(new AttackUnitBattleAction(targetUnit, power, attackClass));

                break;
            }

            case AttackResult.Heal:
            {
                var healPower = attackResult.Power!.Value;
                var attackClass = attackResult.AttackType!.Value;

                targetUnit.Unit.HitPoints += healPower;
                Actions.Add(new AttackUnitBattleAction(targetUnit, healPower, attackClass));

                break;
            }

            case AttackResult.Effect:
            {
                var power = attackResult.Power;
                var roundDuration = attackResult.RoundDuration!.Value;
                var attackClass = attackResult.AttackType!.Value;

                targetUnit.Unit.Effects.AddBattleEffect(
                    new UnitBattleEffect(AttackClassToEffectType(attackClass), roundDuration, power));
                Actions.Add(new EffectUnitBattleAction(targetUnit, attackClass));
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
            Actions.Add(new AnimationBattleAction(targetAnimation.AnimationComponent));
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
}