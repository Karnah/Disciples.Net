using System;
using System.Collections.Generic;
using System.Linq;

using Disciples.Common.Helpers;
using Disciples.Engine.Battle;
using Disciples.Engine.Battle.Controllers;
using Disciples.Engine.Battle.Enums;
using Disciples.Engine.Battle.GameObjects;
using Disciples.Engine.Battle.Models;
using Disciples.Engine.Battle.Models.BattleActions;
using Disciples.Engine.Battle.Providers;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Extensions;
using Disciples.Engine.Implementation.Base;

namespace Disciples.Engine.Implementation.Battle.Controllers
{
    /// <inheritdoc cref="IBattleController" />
    public class BattleController : BaseSupportLoading<BattleSquadsData>, IBattleController
    {
        /// <summary>
        /// Разброс инициативы при вычислении очередности.
        /// </summary>
        private const int INITIATIVE_RANGE = 5;
        /// <summary>
        /// Разброс атаки при ударе.
        /// </summary>
        private const int ATTACK_RANGE = 5;
        /// <summary>
        /// Слой, который перекрывает всех юнитов.
        /// </summary>
        private const int ABOVE_ALL_UNITS_LAYER = 100 * 4;
        /// <summary>
        /// Задержка после завершения всех действий, прежде чем ход перейдёт к следующему юниту.
        /// </summary>
        private const long ACTION_DELAY = 250;


        private readonly IBattleSceneController _battleSceneController;
        private readonly IBattleActionProvider _battleActionProvider;

        /// <summary>
        /// Список всех юнитов.
        /// </summary>
        private List<BattleUnit> _units;
        /// <summary>
        /// Очередность хода юнитов.
        /// </summary>
        private Queue<BattleUnit> _turnOrder;

        /// <inheritdoc />
        public BattleController(IBattleSceneController battleSceneController, IBattleActionProvider battleActionProvider)
        {
            _battleSceneController = battleSceneController;
            _battleActionProvider = battleActionProvider;
        }


        /// <inheritdoc />
        public BattleUnit CurrentUnitObject { get; private set; }

        /// <inheritdoc />
        public bool IsSecondAttack { get; private set; }

        /// <inheritdoc />
        public IReadOnlyList<BattleUnit> Units => _units;

        /// <inheritdoc />
        public override bool OneTimeLoading => false;


        /// <inheritdoc />
        public event EventHandler<UnitActionBeginEventArgs> UnitActionBegin;

        /// <inheritdoc />
        public event EventHandler UnitActionEnded;

        /// <inheritdoc />
        public event EventHandler BattleEnded;


        /// <inheritdoc />
        public BattleUnit GetUnitObject(Unit unit)
        {
            return Units.First(u => u.Unit == unit);
        }

        /// <inheritdoc />
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
            if (targetUnitGameObject.Unit.IsDead && currentUnit.UnitType.FirstAttack.Reach != Reach.All)
                return false;

            // Лекарь по одиночной цели без второй атаки может лечить только тех,
            // у кого меньше максимального значения здоровья.
            if (currentUnit.Player == targetUnit.Player &&
                currentUnit.UnitType.FirstAttack.AttackClass == AttackClass.Heal &&
                currentUnit.UnitType.FirstAttack.Reach == Reach.Any &&
                currentUnit.UnitType.SecondAttack == null &&
                targetUnit.HitPoints == targetUnit.UnitType.HitPoints) {
                return false;
            }

            // Если юнит может атаковать только ближайшего, то проверяем препятствия.
            if (currentUnit.UnitType.FirstAttack.Reach == Reach.Adjacent) {
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

        /// <inheritdoc />
        public bool AttackUnit(BattleUnit targetUnitGameObject)
        {
            if (CanAttack(targetUnitGameObject) == false)
                return false;

            CurrentUnitObject.Action = BattleAction.Attacking;
            _battleActionProvider.RegisterBattleAction(new FirstAttackBattleAction(CurrentUnitObject, targetUnitGameObject));

            UnitActionBegin?.Invoke(this, new UnitActionBeginEventArgs(UnitActionType.Attack));

            return true;
        }

        /// <inheritdoc />
        public void Defend()
        {
            UnitActionBegin?.Invoke(this, new UnitActionBeginEventArgs(UnitActionType.Defend));

            IsSecondAttack = true;
            _battleActionProvider.RegisterBattleAction(new TouchUnitBattleAction(CurrentUnitObject, TouchUnitActionType.Defend));
        }

        /// <inheritdoc />
        public void Wait()
        {
            UnitActionBegin?.Invoke(this, new UnitActionBeginEventArgs(UnitActionType.Wait));

            IsSecondAttack = true;
            _battleActionProvider.RegisterBattleAction(new TouchUnitBattleAction(CurrentUnitObject, TouchUnitActionType.Waiting));

            // todo Перебросить в конец очереди.
        }


        /// <inheritdoc />
        protected override void LoadInternal(BattleSquadsData data)
        {
            ArrangeUnits(data.AttackSquad, data.DefendSquad);
            StartNewRound();

            _battleActionProvider.BattleActionBegin += OnBattleActionBegin;
            _battleActionProvider.BattleActionEnd += OnBattleActionEnd;
        }

        /// <inheritdoc />
        protected override void UnloadInternal()
        {
            _battleActionProvider.BattleActionBegin -= OnBattleActionBegin;
            _battleActionProvider.BattleActionEnd -= OnBattleActionEnd;

            // Уничтожаем объекты юнитов.
            foreach (var battleUnit in Units)
            {
                battleUnit.Destroy();
            }
        }


        /// <summary>
        /// Расставить юнитов по позициям.
        /// </summary>
        private void ArrangeUnits(Squad attackSquad, Squad defendSquad)
        {
            _units = new List<BattleUnit>();

            foreach (var attackSquadUnit in attackSquad.Units) {
                _units.Add(_battleSceneController.AddBattleUnit(attackSquadUnit, true));
            }

            foreach (var defendSquadUnit in defendSquad.Units) {
                _units.Add(_battleSceneController.AddBattleUnit(defendSquadUnit, false));
            }
        }


        /// <summary>
        /// Обработать начала действия на поле боя.
        /// </summary>
        private void OnBattleActionBegin(object sender, BattleActionEventArgs e)
        {

        }

        /// <summary>
        /// Обработать завершение действия на поле боя.
        /// </summary>
        private void OnBattleActionEnd(object sender, BattleActionEventArgs e)
        {
            // Обрабатываем событие того, что анимация атакующего до того момента, когда можем выводить результаты действия.
            if (e.BattleAction is FirstAttackBattleAction firstAttackAction) {
                OnFirstAttackAction(firstAttackAction.Target);
                return;
            }

            // Событие того, что все действия, связанные с первой атакой завершились, и время обрабатывать вторую атаку.
            if (e.BattleAction is SecondAttackBattleAction secondAttackAction) {
                ProcessAttack(secondAttackAction.Attacker, secondAttackAction.Target, false, secondAttackAction.Power);
                return;
            }

            // Обрабатываем действия.
            if (e.BattleAction is TouchUnitBattleAction touchUnitAction) {
                // Если юнит защитился, то добавляем эффект.
                if (touchUnitAction.TouchUnitActionType == TouchUnitActionType.Defend) {
                    touchUnitAction.TargetUnit.Unit.Effects.AddBattleEffect(new UnitBattleEffect(UnitBattleEffectType.Defend, 1));
                }

                // Если юнит умер, то превращаем его в кучу костей.
                if (touchUnitAction.TouchUnitActionType == TouchUnitActionType.Death) {
                    touchUnitAction.TargetUnit.Action = BattleAction.Dead;
                }
            }

            // Обрабатываем наложение эффекта.
            if (e.BattleAction is EffectUnitBattleAction effectUnitAction) {
                switch (effectUnitAction.AttackClass) {
                    case AttackClass.Poison:
                        break;
                    case AttackClass.Frostbite:
                        effectUnitAction.TargetUnit.Unit.Effects.AddBattleEffect(
                            new UnitBattleEffect(UnitBattleEffectType.Frostbite, effectUnitAction.RoundDuration, effectUnitAction.Power));
                        break;
                    case AttackClass.Blister:
                        effectUnitAction.TargetUnit.Unit.Effects.AddBattleEffect(
                            new UnitBattleEffect(UnitBattleEffectType.Blister, effectUnitAction.RoundDuration, effectUnitAction.Power));
                        break;
                }
            }

            // Обрабатываем завершение анимации юнита.
            if (e.BattleAction is AnimationBattleAction animationAction) {
                if (animationAction.AnimationComponent.GameObject is BattleUnit battleUnit) {
                    battleUnit.Action = BattleAction.Waiting;

                    // Обрабатываем смерть юнита.
                    if (battleUnit.Unit.HitPoints == 0) {
                        battleUnit.Unit.IsDead = true;

                        var deathAnimation = _battleSceneController.AddAnimation(
                            battleUnit.AnimationComponent.BattleUnitAnimation.DeathFrames,
                            battleUnit.X,
                            battleUnit.Y,
                            battleUnit.AnimationComponent.Layer + 2,
                            false);
                        _battleActionProvider.RegisterBattleAction(new AnimationBattleAction(deathAnimation.AnimationComponent));
                        _battleActionProvider.RegisterBattleAction(new TouchUnitBattleAction(battleUnit, TouchUnitActionType.Death));
                    }
                }
            }

            // Обрабатываем завершение всех действий.
            if (_battleActionProvider.IsInterfaceActive) {
                // Перед завершением делаем небольшую задержку.
                if (!(e.BattleAction is DelayAction)) {
                    _battleActionProvider.RegisterBattleAction(new DelayAction(ACTION_DELAY));
                    return;
                }

                // Проверяем, если битва завершилась.
                if (IsBattleEnd()) {
                    BattleEnded?.Invoke(this, EventArgs.Empty);
                    return;
                }

                // Если юнит может атаковать дважды, и сейчас атаковал в первый раз, то не передаём ход дальше.
                if (CurrentUnitObject.Unit.UnitType.AttackTwice && !IsSecondAttack) {
                    IsSecondAttack = true;
                }
                else {
                    IsSecondAttack = false;
                    NextTurn();
                }

                UnitActionEnded?.Invoke(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Обработать событие того, что необходимо рассчитать урон, который нанёс юнит.
        /// </summary>
        private void OnFirstAttackAction(BattleUnit targetUnitGameObject)
        {
            var curUnitAnimation = CurrentUnitObject.AnimationComponent;
            var curUnit = CurrentUnitObject.Unit;
            var targetUnits = CurrentUnitObject.Unit.UnitType.FirstAttack.Reach == Reach.All
                ? GetUnitSquad(targetUnitGameObject)
                : new[] { targetUnitGameObject };

            // Некоторые вторые атаки, например, выпить жизненную силу обрабатываются особым образом.
            // Мы должны посчитать весь урон, который нанесли первой атакой, а потом сделать целями других юнитов.
            var unitSecondAttack = curUnit.UnitType.SecondAttack;
            bool calculateDamage = false;
            int damage = 0;
            if (unitSecondAttack?.AttackClass == AttackClass.DrainOverflow ||
                unitSecondAttack?.AttackClass == AttackClass.Doppelganger)
                calculateDamage = true;

            foreach (var targetUnit in targetUnits) {
                // Проверяем меткость юнита.
                var chanceOfFirstAttack = RandomGenerator.Next(0, 100);
                if (chanceOfFirstAttack > curUnit.FirstAttackAccuracy) {
                    _battleActionProvider.RegisterBattleAction(new TouchUnitBattleAction(targetUnit, TouchUnitActionType.Miss));
                    continue;
                }

                var power = ProcessAttack(CurrentUnitObject, targetUnit, true);
                damage += power;

                // Если есть анимация, применяемая к юниту, то добавляем её на сцену.
                if (curUnitAnimation.BattleUnitAnimation.TargetAnimation?.IsSingle == true) {
                    var targetAnimation = _battleSceneController.AddAnimation(
                        curUnitAnimation.BattleUnitAnimation.TargetAnimation.Frames,
                        targetUnit.X,
                        targetUnit.Y,
                        targetUnit.AnimationComponent.Layer + 2,
                        false);
                    _battleActionProvider.RegisterBattleAction(
                        new AnimationBattleAction(targetAnimation.AnimationComponent));
                }

                // Сразу обрабатываем вторую атаку.
                // Однако, её последствия будут только после завершения всех анимаций, которые связаны с первой.
                // Кроме того, если был промах, то он никак не будет отмечен.
                var chanceOfSecondAttack = RandomGenerator.Next(0, 100);
                if (unitSecondAttack != null && !calculateDamage && chanceOfSecondAttack <= curUnit.SecondAttackAccuracy) {
                    _battleActionProvider.RegisterBattleAction(new SecondAttackBattleAction(CurrentUnitObject, targetUnit));
                }
            }

            // Если есть анимация, применяемая на площадь, то добавляем её на сцену.
            if (curUnitAnimation.BattleUnitAnimation.TargetAnimation?.IsSingle == false) {
                // Центр анимации будет приходиться на середину между первым и вторым рядом.
                var isTargetAttacker = targetUnits.First().IsAttacker;
                var (x, y) = BattleUnit.GetSceneUnitPosition(isTargetAttacker, 0.5, 1);

                var areaAnimation = _battleSceneController.AddAnimation(
                    curUnitAnimation.BattleUnitAnimation.TargetAnimation.Frames,
                    x,
                    y,
                    ABOVE_ALL_UNITS_LAYER,
                    false);
                _battleActionProvider.RegisterBattleAction(new AnimationBattleAction(areaAnimation.AnimationComponent));
            }

            // В любом случае дожидаемся завершения анимации атаки.
            _battleActionProvider.RegisterBattleAction(new AnimationBattleAction(curUnitAnimation));

            // TODO Добавить обработка выпить жизнь.
            // По идее, нужно будет разделить урон на каждого юнита.
            // Но если юнит здоров, то делить нужно между оставшимися.
            if (calculateDamage) {
            }
        }

        /// <summary>
        /// Обработать действие одного юнита на другого.
        /// </summary>
        /// <param name="attackingUnitObject">Юнит, который воздействует.</param>
        /// <param name="targetUnitObject">Юнит, на которого воздействует.</param>
        /// <param name="isFirstAttack">Используется базовая атака или вторичная.</param>
        /// <param name="externalPower">Уже рассчитанная сила.</param>
        private int ProcessAttack(BattleUnit attackingUnitObject, BattleUnit targetUnitObject, bool isFirstAttack, int? externalPower = null)
        {
            var attackingUnit = attackingUnitObject.Unit;
            var targetUnit = targetUnitObject.Unit;

            var power = externalPower ??
                        (isFirstAttack
                            ? attackingUnit.FirstAttackPower
                            : attackingUnit.SecondAttackPower.Value);
            var attack = isFirstAttack
                ? attackingUnit.UnitType.FirstAttack
                : attackingUnit.UnitType.SecondAttack;

            // todo Сразу обработать иммунитет + сопротивления. Также вернуть результат.
            // Вторая атака не будет действовать, если первая упёрлась в иммунитет.

            switch (attack.AttackClass) {
                case AttackClass.Damage:
                    // todo Максимальное значение атаки - 250/300/400.
                    var attackPower = power + RandomGenerator.Next(ATTACK_RANGE);

                    // Уменьшаем входящий урон в зависимости от защиты.
                    attackPower = (int) (attackPower * (1 - targetUnit.Armor / 100.0));

                    // Если юнит защитился, то урон уменьшается в два раза.
                    if (targetUnit.Effects.ExistsBattleEffect(UnitBattleEffectType.Defend)) {
                        attackPower /= 2;
                    }

                    // Мы не можем нанести урон больше, чем осталось очков здоровья.
                    attackPower = Math.Min(attackPower, targetUnit.HitPoints);
                    targetUnit.HitPoints -= attackPower;

                    targetUnitObject.Action = BattleAction.TakingDamage;
                    _battleActionProvider.RegisterBattleAction(new AnimationBattleAction(targetUnitObject.AnimationComponent));
                    _battleActionProvider.RegisterBattleAction(new AttackUnitBattleAction(targetUnitObject, attackPower, attack.AttackClass));

                    break;

                case AttackClass.Drain:
                    break;

                case AttackClass.Paralyze:
                    break;

                case AttackClass.Heal:
                    var healPower = Math.Min(power, targetUnit.MaxHitPoints - targetUnit.HitPoints);
                    if (healPower != 0) {
                        targetUnit.HitPoints += healPower;
                        _battleActionProvider.RegisterBattleAction(new AttackUnitBattleAction(targetUnitObject, healPower, attack.AttackClass));
                    }

                    break;

                case AttackClass.Fear:
                    break;

                case AttackClass.BoostDamage:
                    break;

                case AttackClass.Petrify:
                    break;

                case AttackClass.LowerDamage:
                    break;

                case AttackClass.LowerInitiative:
                    break;

                case AttackClass.Poison:
                case AttackClass.Frostbite:
                case AttackClass.Blister:
                    targetUnit.Effects.AddBattleEffect(new UnitBattleEffect(AttackClassToEffectType(attack.AttackClass), 2, power));
                    _battleActionProvider.RegisterBattleAction(new EffectUnitBattleAction(targetUnitObject, attack.AttackClass));
                    break;

                case AttackClass.Revive:
                    break;

                case AttackClass.DrainOverflow:
                    break;

                case AttackClass.Cure:
                    break;

                case AttackClass.Summon:
                    break;

                case AttackClass.DrainLevel:
                    break;

                case AttackClass.GiveAttack:
                    break;

                case AttackClass.Doppelganger:
                    break;

                case AttackClass.TransformSelf:
                    break;

                case AttackClass.TransformOther:
                    break;

                case AttackClass.BestowWards:
                    break;

                case AttackClass.Shatter:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return power;
        }


        /// <summary>
        /// Начать новый раунд.
        /// </summary>
        private void StartNewRound()
        {
            _turnOrder = new Queue<BattleUnit>(
                _units
                    .Where(u => u.Unit.IsDead == false)
                    .OrderByDescending(u => u.Unit.Initiative + RandomGenerator.Next(0, INITIATIVE_RANGE)));

            CurrentUnitObject = _turnOrder.Dequeue();
            CurrentUnitObject.Unit.Effects.OnUnitTurn();
        }

        /// <summary>
        /// Передать ход следующему юниту.
        /// </summary>
        private void NextTurn()
        {
            do
            {
                if (_turnOrder.TryDequeue(out var nextUnit) && nextUnit.Unit.IsDead == false)
                {
                    CurrentUnitObject = nextUnit;
                    CurrentUnitObject.Unit.Effects.OnUnitTurn();

                    return;
                }
            } while (_turnOrder.Any());

            StartNewRound();
        }

        /// <summary>
        /// Проверить, был ли уничтожен один из отрядов.
        /// </summary>
        private bool IsBattleEnd()
        {
            return Units
                       .Where(u => u.Unit.IsDead == false)
                       .Select(u => u.Unit.Player)
                       .Distinct()
                       .Count() < 2;
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

        /// <summary>
        /// Получить тип эффекта в зависимости от типа атаки.
        /// </summary>
        private static UnitBattleEffectType AttackClassToEffectType(AttackClass attackClass)
        {
            switch (attackClass) {
                case AttackClass.Poison:
                    return UnitBattleEffectType.Poison;
                case AttackClass.Frostbite:
                    return UnitBattleEffectType.Frostbite;
                case AttackClass.Blister:
                    return UnitBattleEffectType.Blister;
                default:
                    throw new ArgumentOutOfRangeException(nameof(attackClass), attackClass, null);
            }
        }
    }
}