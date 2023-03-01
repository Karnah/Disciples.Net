using Disciples.Engine;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Extensions;
using Disciples.Engine.Implementation.Base;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.GameObjects;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Models.BattleActions;

namespace Disciples.Scene.Battle.Controllers
{
    /// <inheritdoc cref="IBattleController" />
    public class BattleController : BaseSupportLoading, IBattleController
    {
        /// <summary>
        /// Разброс инициативы при вычислении очередности.
        /// </summary>
        private const int INITIATIVE_RANGE = 5;
        /// <summary>
        /// Слой, который перекрывает всех юнитов.
        /// </summary>
        private const int ABOVE_ALL_UNITS_LAYER = 100 * 4;

        private readonly IBattleSceneController _battleSceneController;
        private readonly BattleProcessor _battleProcessor;

        private Squad _attackSquad;
        private Squad _defendSquad;

        private List<BattleUnit> _units;

        /// <summary>
        /// Очередность хода юнитов.
        /// </summary>
        private Queue<BattleUnit> _turnOrder;

        /// <summary>
        /// Создать объект типа <see cref="BattleController" />.
        /// </summary>
        public BattleController(IBattleSceneController battleSceneController, BattleProcessor battleProcessor)
        {
            _battleSceneController = battleSceneController;
            _battleProcessor = battleProcessor;
        }

        /// <inheritdoc />
        public override bool IsSharedBetweenScenes => false;

        public BattleUnit CurrentUnitObject { get; private set; }

        /// <inheritdoc />
        public bool IsSecondAttack { get; private set; }

        /// <inheritdoc />
        public IReadOnlyList<BattleUnit> Units => _units;

        /// <inheritdoc />
        public void InitializeParameters(BattleSquadsData parameters)
        {
            _attackSquad = parameters.AttackSquad;
            _defendSquad = parameters.DefendSquad;
        }

        /// <inheritdoc />
        public void BeforeSceneUpdate(BattleUpdateContext context)
        {
            // ProcessBeginAction может добавлять новые действия в NewActions,
            // Поэтому вызываем ToList().
            // TODO Подумать над уменьшением выделения памяти.
            foreach (var newBattleAction in context.NewActions.ToList())
            {
                ProcessBeginAction(context, newBattleAction);
            }
        }

        /// <inheritdoc />
        public void AfterSceneUpdate(BattleUpdateContext context)
        {
            foreach (var completedAction in context.CompletedActions)
            {
                ProcessCompletedBattleAction(context, completedAction);
            }

            // Если последние действия завершились в этом обновлении,
            // Значит атака закончилась и нужно начинать новый ход.
            if (context.IsAllActionsCompletedThisUpdate)
            {
                // Перед тем, как отдать ход следующему юниту мы должны подождать немного времени.
                if (!context.CompletedActions.OfType<DelayLastBattleAction>().Any())
                {
                    context.AddNewAction(new DelayLastBattleAction());
                    return;
                }

                NextTurn(context);
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
            // Уничтожаем объекты юнитов.
            foreach (var battleUnit in Units)
            {
                battleUnit.Destroy();
            }
        }

        /// <summary>
        /// Расставить юнитов по позициям.
        /// </summary>
        private void ArrangeUnits()
        {
            _units = new List<BattleUnit>();

            foreach (var attackSquadUnit in _attackSquad.Units)
                _units.Add(_battleSceneController.AddBattleUnit(attackSquadUnit, true));

            foreach (var defendSquadUnit in _defendSquad.Units)
                _units.Add(_battleSceneController.AddBattleUnit(defendSquadUnit, false));
        }

        // Начать новый раунд.
        private void StartNewRound()
        {
            _turnOrder = new Queue<BattleUnit>(
                _units
                    .Where(u => u.Unit.IsDead == false)
                    .OrderByDescending(u => u.Unit.Initiative + RandomGenerator.Next(0, INITIATIVE_RANGE)));

            CurrentUnitObject = _turnOrder.Dequeue();
            CurrentUnitObject.Unit.Effects.OnUnitTurn();
        }

        // Передать ход следующему юниту.
        private void NextTurn(BattleUpdateContext context)
        {
            if (IsBattleCompleted())
            {
                // TODO Также должны быть сняты все эффекты с оставшихся юнитов.
                context.AddNewAction(new BattleCompletedAction());
                return;
            }

            // Была завершена первая атака юнита, который может атаковать дважды.
            // В этом случае ход остаётся у него.
            if (CurrentUnitObject.Unit.UnitType.IsAttackTwice && !IsSecondAttack)
            {
                IsSecondAttack = true;
                return;
            }

            IsSecondAttack = false;

            do
            {
                if (_turnOrder.TryDequeue(out var nextUnit) && nextUnit.Unit.IsDead == false)
                {
                    CurrentUnitObject = nextUnit;
                    CurrentUnitObject.Unit.Effects.OnUnitTurn();

                    return;
                }
            } while (_turnOrder.Count > 0);

            StartNewRound();
        }

        /// <summary>
        /// Проверить битва завершилась победой одной из сторон.
        /// </summary>
        private bool IsBattleCompleted()
        {
            return Units
                       .Where(u => u.Unit.IsDead == false)
                       .Select(u => u.Unit.Player)
                       .Distinct()
                       .Count() < 2;
        }


        /// <inheritdoc />
        public BattleUnit GetUnitObject(Unit unit)
        {
            return Units.First(u => u.Unit == unit);
        }

        #region AttackMethods

        public bool CanAttack(BattleUnit targetUnitGameObject)
        {
            var currentUnit = CurrentUnitObject.Unit;
            var targetUnit = targetUnitGameObject.Unit;

            // Лекарь не может атаковать врага, а воин не может атаковать союзника.
            if (currentUnit.Player == targetUnit.Player && currentUnit.HasAllyAbility() == false ||
                currentUnit.Player != targetUnit.Player && currentUnit.HasEnemyAbility() == false) {
                return false;
            }

            // todo Патриарх может воскресить юнита, так что эта проверка не совсем корректна.
            // Если юнит бьёт по площади, то разрешаем кликнуть на мертвого юнита.
            if (targetUnitGameObject.Unit.IsDead && currentUnit.UnitType.MainAttack.Reach != Reach.All)
                return false;

            // Лекарь по одиночной цели без второй атаки может лечить только тех,
            // у кого меньше максимального значения здоровья.
            if (currentUnit.Player == targetUnit.Player &&
                currentUnit.UnitType.MainAttack.AttackClass == AttackClass.Heal &&
                currentUnit.UnitType.MainAttack.Reach == Reach.Any &&
                currentUnit.UnitType.SecondaryAttack == null &&
                targetUnit.HitPoints == targetUnit.UnitType.HitPoints) {
                return false;
            }

            // Если юнит может атаковать только ближайшего, то проверяем препятствия.
            if (currentUnit.UnitType.MainAttack.Reach == Reach.Adjacent) {
                // Если атакующий юнит находится сзади и есть линия союзников впереди.
                var currentSquad = GetUnitSquad(CurrentUnitObject);
                if (currentUnit.SquadLinePosition == 0 && IsFirstLineEmpty(currentSquad) == false)
                    return false;

                // Если враг находится сзади, то проверяем, что нет первой вражеской линии.
                var targetSquad = GetUnitSquad(targetUnitGameObject);
                if (targetUnit.SquadLinePosition == 0 && IsFirstLineEmpty(targetSquad) == false)
                    return false;

                // Проверка, может ли юнит дотянуться до врага.
                if (CanAttackOnFlank(
                        currentUnit.SquadFlankPosition,
                        targetUnit.SquadFlankPosition,
                        targetUnit.SquadLinePosition,
                        targetSquad) == false)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Получить весь отряд указанного юнита.
        /// </summary>
        private IReadOnlyCollection<BattleUnit> GetUnitSquad(BattleUnit battleUnit)
        {
            return Units.Where(u => u.Unit.Player == battleUnit.Unit.Player && u.Unit.IsDead == false).ToList();
        }

        /// <summary>
        /// Проверить, свободна ли первая линия в отряде.
        /// </summary>
        private static bool IsFirstLineEmpty(IReadOnlyCollection<BattleUnit> squad)
        {
            return squad.Any(u => u.Unit.SquadLinePosition == 1 &&
                                  u.Unit.IsDead == false) == false;
        }

        /// <summary>
        /// Проверить, можно ли атаковать цель в зависимости от расположения на фланге.
        /// </summary>
        private static bool CanAttackOnFlank(int currentUnitFlankPosition, int targetUnitFlankPosition, int targetUnitLinePosition, IReadOnlyCollection<BattleUnit> targetSquad)
        {
            // Если юниты находятся по разные стороны флагов и занят вражеский центр или соседняя с атакующим клетка, то атаковать нельзя.
            if (Math.Abs(currentUnitFlankPosition - targetUnitFlankPosition) > 1 &&
                (IsPlaceEmpty(targetSquad, targetUnitLinePosition, 1) == false || IsPlaceEmpty(targetSquad, targetUnitLinePosition, currentUnitFlankPosition) == false))
                return false;

            return true;
        }

        /// <summary>
        /// Проверить, свободна ли клетка на арене.
        /// </summary>
        private static bool IsPlaceEmpty(IReadOnlyCollection<BattleUnit> squad, int line, int flank)
        {
            return squad.Any(u => u.Unit.SquadLinePosition == line &&
                                  u.Unit.SquadFlankPosition == flank &&
                                  u.Unit.IsDead == false) == false;
        }

        #endregion

        /// <summary>
        /// Обработать начало нового действия.
        /// </summary>
        private void ProcessBeginAction(BattleUpdateContext context, IBattleAction battleAction)
        {
            if (battleAction is BeginAttackUnitBattleAction beginAttackUnitAction)
            {
                ProcessBeginAttackUnitAction(context, beginAttackUnitAction);
                return;
            }

            if (battleAction is DefendBattleAction)
            {
                ProcessDefendUnitAction(context);
                return;
            }

            if (battleAction is WaitingBattleAction)
            {
                ProcessWaitingUnitAction(context);
                return;
            }

            if (battleAction is BeginSecondaryAttackBattleAction secondaryAttackAction)
            {
                ProcessBeginSecondaryAttackAction(context, secondaryAttackAction);
                return;
            }
        }

        /// <summary>
        /// Обработать начало воздействия на юнита.
        /// </summary>
        private void ProcessBeginAttackUnitAction(BattleUpdateContext context, BeginAttackUnitBattleAction beginAttackUnitAction)
        {
            // Нельзя было атаковать данного юнита.
            if (!CanAttack(beginAttackUnitAction.TargetUnit))
                return;

            CurrentUnitObject.Action = BattleAction.Attacking;

            var targetUnitGameObject = beginAttackUnitAction.TargetUnit;
            context.AddNewAction(new MainAttackBattleAction(CurrentUnitObject, targetUnitGameObject));
        }

        /// <summary>
        /// Обработать защиту юнита.
        /// </summary>
        private void ProcessDefendUnitAction(BattleUpdateContext context)
        {
            IsSecondAttack = true;
            context.AddNewAction(new UnitBattleAction(CurrentUnitObject, UnitActionType.Defend));
        }

        /// <summary>
        /// Обработать ожидание юнита.
        /// </summary>
        private void ProcessWaitingUnitAction(BattleUpdateContext context)
        {
            IsSecondAttack = true;
            context.AddNewAction(new UnitBattleAction(CurrentUnitObject, UnitActionType.Waiting));
        }

        /// <summary>
        /// Обработать завершение дополнительной атаки юнита.
        /// </summary>
        private void ProcessBeginSecondaryAttackAction(BattleUpdateContext context, BeginSecondaryAttackBattleAction beginSecondaryAttackAction)
        {
            var attacker = beginSecondaryAttackAction.Attacker;
            var target = beginSecondaryAttackAction.Target;
            var power = beginSecondaryAttackAction.Power;

            var attackResult = _battleProcessor.ProcessSecondaryAttack(attacker.Unit, target.Unit, power);
            ProcessAttackResult(context, attacker, target, attackResult);
        }

        /// <summary>
        /// Обработать завершение действия 
        /// </summary>
        private void ProcessCompletedBattleAction(BattleUpdateContext context, IBattleAction battleAction)
        {
            if (battleAction is MainAttackBattleAction mainAttackAction)
            {
                ProcessCompletedMainAttackAction(context, mainAttackAction);
                return;
            }

            if (battleAction is AnimationBattleAction animationAction)
            {
                ProcessCompletedBattleUnitAnimation(context, animationAction);
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
        private void ProcessCompletedMainAttackAction(
            BattleUpdateContext context,
            MainAttackBattleAction mainAttackBattleAction)
        {
            var targetUnitGameObject = mainAttackBattleAction.Target;
            var curUnitAnimation = CurrentUnitObject.AnimationComponent;
            var curUnit = CurrentUnitObject.Unit;
            var targetUnits = CurrentUnitObject.Unit.UnitType.MainAttack.Reach == Reach.All
                ? GetUnitSquad(targetUnitGameObject)
                : new[] { targetUnitGameObject };

            // Некоторые вторые атаки, например, выпить жизненную силу обрабатываются особым образом.
            // Мы должны посчитать весь урон, который нанесли первой атакой, а потом сделать целями других юнитов.
            var unitSecondAttack = curUnit.UnitType.SecondaryAttack;
            bool shouldCalculateDamage = false;
            int damage = 0;
            if (unitSecondAttack?.AttackClass == AttackClass.DrainOverflow ||
                unitSecondAttack?.AttackClass == AttackClass.Doppelganger)
            {
                shouldCalculateDamage = true;
            }

            foreach (var targetUnit in targetUnits)
            {
                var attackResult = _battleProcessor.ProcessMainAttack(curUnit, targetUnit.Unit);

                // Атака не выполнялась, либо еще не умеем обрабатывать данный тип атаки.
                if (attackResult == null)
                    continue;

                ProcessAttackResult(context, CurrentUnitObject, targetUnit, attackResult);

                if (attackResult.AttackResult == AttackResult.Attack)
                    damage += attackResult.Power!.Value;

                // Сразу обрабатываем вторую атаку.
                // Однако, её последствия будут только после завершения всех анимаций, которые связаны с первой.
                if (unitSecondAttack != null && !shouldCalculateDamage)
                    context.AddDelayedAction(new BeginSecondaryAttackBattleAction(CurrentUnitObject, targetUnit));
            }

            // Если есть анимация, применяемая на площадь, то добавляем её на сцену.
            if (curUnitAnimation.BattleUnitAnimation.TargetAnimation?.IsSingle == false)
            {
                // Центр анимации будет приходиться на середину между первым и вторым рядом.
                var isTargetAttacker = targetUnits.First().IsAttacker;
                var (x, y) = BattleUnit.GetSceneUnitPosition(isTargetAttacker, 0.5, 1);

                var areaAnimation = _battleSceneController.AddAnimation(
                    curUnitAnimation.BattleUnitAnimation.TargetAnimation.Frames,
                    x,
                    y,
                    ABOVE_ALL_UNITS_LAYER,
                    false);
                context.AddNewAction(new AnimationBattleAction(areaAnimation.AnimationComponent));
            }

            // В любом случае дожидаемся завершения анимации атаки.
            context.AddNewAction(new AnimationBattleAction(curUnitAnimation));

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
        private void ProcessCompletedBattleUnitAnimation(BattleUpdateContext context, AnimationBattleAction animationAction)
        {
            if (animationAction.AnimationComponent.GameObject is not BattleUnit battleUnit)
                return;

            battleUnit.Action = BattleAction.Waiting;

            // Обрабатываем смерть юнита.
            if (battleUnit.Unit.HitPoints == 0)
            {
                battleUnit.Unit.IsDead = true;

                var deathAnimation = _battleSceneController.AddAnimation(
                    battleUnit.AnimationComponent.BattleUnitAnimation.DeathFrames,
                    battleUnit.X,
                    battleUnit.Y,
                    battleUnit.AnimationComponent.Layer + 2,
                    false);
                context.AddNewAction(new AnimationBattleAction(deathAnimation.AnimationComponent));
                context.AddNewAction(new UnitBattleAction(battleUnit, UnitActionType.Dying));
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
                unitAction.TargetUnit.Action = BattleAction.Dead;
            }

            // На юнита наложен эффект.
            if (unitAction.UnitActionType == UnitActionType.UnderEffect)
            {
                var effectAction = (EffectUnitBattleAction)unitAction;
                effectAction.TargetUnit.Unit.Effects.AddBattleEffect(
                    new UnitBattleEffect(AttackClassToEffectType(effectAction.AttackClass), effectAction.RoundDuration, effectAction.Power));
            }
        }

        /// <summary>
        /// Обработать результат атаки.
        /// </summary>
        private void ProcessAttackResult(
            BattleUpdateContext context,
            BattleUnit attackerUnit,
            BattleUnit targetUnit,
            BattleProcessorAttackResult? attackResult)
        {
            // Атака не выполнялась, либо еще не умеем обрабатывать данный тип атаки.
            if (attackResult == null)
                return;

            switch (attackResult.AttackResult)
            {
                case AttackResult.Miss:
                {
                    context.AddNewAction(new UnitBattleAction(targetUnit, UnitActionType.Dodge));
                    break;
                }

                case AttackResult.Attack:
                {
                    var power = attackResult.Power!.Value;
                    var attackClass = attackResult.AttackClass!.Value;

                    targetUnit.Unit.HitPoints -= power;
                    targetUnit.Action = BattleAction.TakingDamage;

                    context.AddNewAction(new AnimationBattleAction(targetUnit.AnimationComponent));
                    context.AddNewAction(new AttackUnitBattleAction(targetUnit, power, attackClass));

                    break;
                }

                case AttackResult.Heal:
                {
                    var healPower = attackResult.Power!.Value;
                    var attackClass = attackResult.AttackClass!.Value;

                    targetUnit.Unit.HitPoints += healPower;
                    context.AddNewAction(new AttackUnitBattleAction(targetUnit, healPower, attackClass));

                    break;
                }

                case AttackResult.Effect:
                {
                    var power = attackResult.Power;
                    var roundDuration = attackResult.RoundDuration!.Value;
                    var attackClass = attackResult.AttackClass!.Value;

                    targetUnit.Unit.Effects.AddBattleEffect(
                        new UnitBattleEffect(AttackClassToEffectType(attackClass), roundDuration, power));
                    context.AddNewAction(new EffectUnitBattleAction(targetUnit, attackClass));
                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Если у атакующего есть анимация, применяемая к юниту, то добавляем её на сцену.
            var targetUnitAnimation = attackerUnit.AnimationComponent.BattleUnitAnimation.TargetAnimation;
            if (targetUnitAnimation?.IsSingle == true)
            {
                var targetAnimation = _battleSceneController.AddAnimation(
                    targetUnitAnimation.Frames,
                    targetUnit.X,
                    targetUnit.Y,
                    targetUnit.AnimationComponent.Layer + 2,
                    false);
                context.AddNewAction(new AnimationBattleAction(targetAnimation.AnimationComponent));
            }
        }

        /// <summary>
        /// Получить тип эффекта в зависимости от типа атаки.
        /// </summary>
        private static UnitBattleEffectType AttackClassToEffectType(AttackClass attackClass)
        {
            return attackClass switch
            {
                AttackClass.Poison => UnitBattleEffectType.Poison,
                AttackClass.Frostbite => UnitBattleEffectType.Frostbite,
                AttackClass.Blister => UnitBattleEffectType.Blister,
                _ => throw new ArgumentOutOfRangeException(nameof(attackClass), attackClass, null)
            };
        }
    }
}