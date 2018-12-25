using System;
using System.Collections.Generic;
using System.Linq;

using Disciples.Common.Helpers;
using Disciples.Engine.Battle.Contollers;
using Disciples.Engine.Battle.Enums;
using Disciples.Engine.Battle.GameObjects;
using Disciples.Engine.Battle.Models;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Extensions;
using Disciples.Engine.Models;

namespace Disciples.Engine.Implementation.Controllers
{
    /// <inheritdoc />
    public class BattleAttackController : IBattleAttackController
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


        private readonly IGame _game;
        private readonly IVisualSceneController _visualSceneController;

        private List<BattleUnit> _units;
        private List<AnimationObject> _targetAnimations;
        private IReadOnlyCollection<BattleUnit> _targetUnits;


        /// <summary>
        /// Очередность хода юнитов.
        /// </summary>
        private Queue<BattleUnit> _turnOrder;
        /// <summary>
        /// Задержка, которая необходима, чтобы завершились все эффекты после удара/действия.
        /// </summary>
        private long? _delayTime;

        public BattleAttackController(IGame game,IVisualSceneController visualSceneController, Squad attackSquad, Squad defendSquad)
        {
            _game = game;
            _visualSceneController = visualSceneController;

            _targetAnimations = new List<AnimationObject>();

            ArrangeUnits(attackSquad, defendSquad);
            StartNewRound();

            _game.SceneEndUpdating += OnUpdate;
        }


        private void OnUpdate(object sender, SceneUpdatingEventArgs args)
        {
            if (BattleState == BattleState.WaitingAction || BattleState == BattleState.BattleEnd)
                return;

            // Обрабатываем моментальные эффекты.
            foreach (var battleUnit in Units) {
                var unitEffects = battleUnit.Unit.Effects;
                unitEffects.OnTick(args.TicksCount);

                if (unitEffects.CurrentMomentalEffect == null)
                    continue;

                // Обрабатываем начало эффекта.
                if (unitEffects.MomentalEffectBegin) {
                    OnUnitMomentalEffectBegin(battleUnit.Unit);
                    continue;
                }

                // Обрабатываем завершение эффекта.
                if (unitEffects.MomentalEffectEnded) {
                    OnUnitMomentalEffectEnded(battleUnit.Unit);
                    continue;
                }
            }

            if (BattleState == BattleState.Delay) {
                // Если остался хотя бы один действующий эффект, то не завершаем действие.
                if (Units.Any(u => u.Unit.Effects.CurrentMomentalEffect != null))
                    return;

                // Сразу после завершения всех действий, мы не передаём ход дальше, а ждём немного времени.
                if (_delayTime == null) {
                    _delayTime = ACTION_DELAY;
                    return;
                }

                // Продолжаем ожидание.
                if (_delayTime > 0) {
                    _delayTime -= args.TicksCount;
                    return;
                }

                _delayTime = null;

                // Если юнит может атаковать дважды, и сейчас атаковал в первый раз, то не передаём ход дальше.
                if (CurrentUnitObject.Unit.UnitType.AttackTwice && IsSecondAttack == false) {
                    IsSecondAttack = true;
                }
                else {
                    IsSecondAttack = false;
                    NextTurn();
                }

                BattleState = BattleState.WaitingAction;
                UnitActionEnded?.Invoke(this, EventArgs.Empty);

                return;
            }

            if (BattleState == BattleState.BeforeTouch) {
                BeforeTouch();
                return;
            }

            if (BattleState == BattleState.AfterTouch) {
                AfterTouch();
                return;
            }
        }

        /// <summary>
        /// Проверить, что анимация текущего юнита дошла да крайней точки и юнит готов вернуться в начальное положение.
        /// Если это произошло, то пересчитываем урон и добавляем на сцену анимации атаки.
        /// </summary>
        private void BeforeTouch()
        {
            var curUnitAnimation = CurrentUnitObject.BattleUnitAnimationComponent;
            var curUnit = CurrentUnitObject.Unit;

            // todo Нужно научиться определять на каком фрейме происходит удар.
            if (curUnitAnimation.FramesCount - curUnitAnimation.FrameIndex > 12)
                return;

            foreach (var targetUnit in _targetUnits) {
                var unit = targetUnit.Unit;

                // Проверяем меткость юнита.
                var chanceAttack = RandomGenerator.Next(0, 100);
                if (chanceAttack > curUnit.UnitType.FirstAttack.Accuracy) {
                    unit.Effects.AddMomentalEffect(new UnitMomentalEffect(UnitMomentalEffectType.Miss));
                    continue;
                }

                ProcessAttack(CurrentUnitObject, targetUnit, true);

                // Если есть анимация, применяемая к юниту, то добавляем её на сцену.
                if (curUnitAnimation.BattleUnitAnimation.TargetAnimation?.IsSingle == true) {
                    var targetAnimation = _visualSceneController.AddAnimation(
                        curUnitAnimation.BattleUnitAnimation.TargetAnimation.Frames,
                        targetUnit.X,
                        targetUnit.Y,
                        targetUnit.BattleUnitAnimationComponent.Layer + 2,
                        false);
                    _targetAnimations.Add(targetAnimation);
                }
            }

            // Если есть анимация, применяемая на площадь, то добавляем её на сцену.
            if (curUnitAnimation.BattleUnitAnimation.TargetAnimation?.IsSingle == false) {
                // Центр анимации будет приходиться на середину между первым и вторым рядом.
                var isTargetAttacker = _targetUnits.First().IsAttacker;
                var (x, y) = BattleUnit.GetSceneUnitPosition(isTargetAttacker, 0.5, 1);

                var areaAnimation = _visualSceneController.AddAnimation(
                    curUnitAnimation.BattleUnitAnimation.TargetAnimation.Frames,
                    x,
                    y,
                    ABOVE_ALL_UNITS_LAYER,
                    false);
                _targetAnimations.Add(areaAnimation);
            }

            BattleState = BattleState.AfterTouch;
        }

        /// <summary>
        /// Удалить со сцены завершившиеся анимации.
        /// Завершить атаку, если все анимации завершились.
        /// </summary>
        private void AfterTouch()
        {
            var curUnitAnimation = CurrentUnitObject.BattleUnitAnimationComponent;

            // Переводим юнита в ожидание если его анимация закончилась.
            if (CurrentUnitObject.Action == BattleAction.Attacking &&
                curUnitAnimation.FrameIndex == curUnitAnimation.FramesCount - 1) {
                CurrentUnitObject.Action = BattleAction.Waiting;
            }

            // Обрабатываем конец анимации получения повреждений для юнитов.
            foreach (var targetUnitObject in _targetUnits) {
                var tarUnit = targetUnitObject.Unit;
                var tarUnitAnim = targetUnitObject.BattleUnitAnimationComponent;

                if (targetUnitObject.Action == BattleAction.TakingDamage &&
                    tarUnitAnim.FrameIndex == tarUnitAnim.FramesCount - 1) {

                    targetUnitObject.Action = BattleAction.Waiting;

                    // В начале просто добавляем анимацию смерти юнита.
                    // Только спустя несколько тиков он превратится в кучку костей.
                    if (tarUnit.HitPoints == 0) {
                        tarUnit.IsDead = true;

                        var deathAnimation = _visualSceneController.AddAnimation(
                            targetUnitObject.BattleUnitAnimationComponent.BattleUnitAnimation.DeathFrames,
                            targetUnitObject.X,
                            targetUnitObject.Y,
                            targetUnitObject.BattleUnitAnimationComponent.Layer + 2,
                            false);

                        _targetAnimations.Add(deathAnimation);
                    }

                    continue;
                }

                // Отображаем мертвого юнита как кости на сцене.
                if (tarUnit.IsDead &&
                    targetUnitObject.Action == BattleAction.Waiting &&
                    tarUnitAnim.FrameIndex == tarUnitAnim.FramesCount / 2) {
                    targetUnitObject.Action = BattleAction.Dead;
                }
            }


            // Если активных действий нет, заканчиваем.
            var isCurrentUnitWaiting = CurrentUnitObject.Action == BattleAction.Waiting;
            var isAllAnimationsEnded = _targetAnimations.All(ta => ta.IsDestroyed);
            var isAllMomentalEffectsEnded = Units.All(u => u.Unit.Effects.CurrentMomentalEffect == null);
            if (isCurrentUnitWaiting && isAllAnimationsEnded && isAllMomentalEffectsEnded) {
                _targetAnimations = new List<AnimationObject>();

                if (IsBattleEnd()) {
                    // todo Добавить что-то более подходящее
                    Console.WriteLine("Battle end");

                    BattleState = BattleState.BattleEnd;
                    BattleEnded?.Invoke(this, EventArgs.Empty);

                    return;
                }

                BattleState = BattleState.Delay;
            }
        }

        /// <summary>
        /// Обработать действие одного юнита на другого.
        /// </summary>
        /// <param name="attackingUnitObject">Юнит, который воздействует.</param>
        /// <param name="targetUnitObject">Юнит, на которого воздействует.</param>
        /// <param name="isFirstAttack">Используется базовая атака или вторичная.</param>
        private static void ProcessAttack(BattleUnit attackingUnitObject, BattleUnit targetUnitObject, bool isFirstAttack)
        {
            var attackingUnit = attackingUnitObject.Unit;
            var targetUnit = targetUnitObject.Unit;

            var power = isFirstAttack
                ? attackingUnit.FirstAttackPower
                : attackingUnit.SecondAttackPower.Value;
            var attack = isFirstAttack
                ? attackingUnit.UnitType.FirstAttack
                : attackingUnit.UnitType.SecondAttack;

            // todo Сразу обработать иммунитет + сопротивления. Также вернуть результат.
            // Вторая атака не будет действовать, если первая упёрлась в иммунитет.

            switch (attack.AttackClass) {
                case AttackClass.Damage:
                    targetUnitObject.Action = BattleAction.TakingDamage;

                    // todo Максимальное значение атаки - 250/300/400.
                    var attackPower = power + RandomGenerator.Next(ATTACK_RANGE);

                    // Уменьшаем входящий урон в зависимости от защиты.
                    attackPower = (int)(attackPower * (1 - targetUnit.Armor / 100.0));

                    // Если юнит защитился, то урон уменьшается в два раза.
                    if (targetUnit.Effects.ExistsBattleEffect(UnitBattleEffectType.Defend)) {
                        attackPower /= 2;
                    }

                    // Мы не можем нанести урон больше, чем осталось очков здоровья.
                    attackPower = Math.Min(attackPower, targetUnit.HitPoints);

                    targetUnit.Effects.AddMomentalEffect(new UnitMomentalEffect(UnitMomentalEffectType.Damaged, attackPower));
                    break;

                case AttackClass.Drain:
                    break;

                case AttackClass.Paralyze:
                    break;

                case AttackClass.Heal:
                    targetUnit.Effects.AddMomentalEffect(new UnitMomentalEffect(UnitMomentalEffectType.Healed, power));
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
                    break;

                case AttackClass.Frostbite:
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

                case AttackClass.Blister:
                    break;

                case AttackClass.BestowWards:
                    break;

                case AttackClass.Shatter:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Обработать начало моментального эффекта.
        /// </summary>
        /// <param name="unit">Юнит у которого начался моментальный эффект.</param>
        private void OnUnitMomentalEffectBegin(Unit unit)
        {
            var currentMomentalEffect = unit.Effects.CurrentMomentalEffect;
            switch (currentMomentalEffect.EffectType) {
                case UnitMomentalEffectType.Damaged:
                    unit.ChangeHitPoints(- currentMomentalEffect.Power.Value);
                    break;
                case UnitMomentalEffectType.Healed:
                    unit.ChangeHitPoints(currentMomentalEffect.Power.Value);
                    break;
            }
        }

        /// <summary>
        /// Обработать завершение моментального эффекта.
        /// </summary>
        /// <param name="unit">Юнит у которого завершился моментальный эффект.</param>
        private void OnUnitMomentalEffectEnded(Unit unit)
        {
            var currentMomentalEffect = unit.Effects.CurrentMomentalEffect;
            switch (currentMomentalEffect.EffectType)
            {
                case UnitMomentalEffectType.Defended:
                    // При защите юнита, который бьёт дважды, необходимо сразу передать ход следующему.
                    // Поэтому считаем, что была совершена вторая атака, чтобы не делать дополнительных проверок где-либо еще.
                    IsSecondAttack = true;

                    unit.Effects.AddBattleEffect(new UnitBattleEffect(UnitBattleEffectType.Defend, 1));
                    break;
                case UnitMomentalEffectType.Waiting:
                    // При ожидании юнита, который бьёт дважды, необходимо сразу передать ход следующему.
                    // Поэтому считаем, что была совершена вторая атака, чтобы не делать дополнительных проверок где-либо еще.
                    IsSecondAttack = true;

                    // todo Перебросить в конец очереди.
                    break;
            }
        }

        /// <inheritdoc />
        public BattleState BattleState { get; private set; } = BattleState.WaitingAction;

        public BattleUnit CurrentUnitObject { get; private set; }

        /// <inheritdoc />
        public bool IsSecondAttack { get; private set; }

        /// <inheritdoc />
        public IReadOnlyList<BattleUnit> Units => _units;


        /// <inheritdoc />
        public event EventHandler<UnitActionBeginEventArgs> UnitActionBegin;

        /// <inheritdoc />
        public event EventHandler UnitActionEnded;

        /// <inheritdoc />
        public event EventHandler BattleEnded;


        /// <summary>
        /// Расставить юнитов по позициям.
        /// </summary>
        private void ArrangeUnits(Squad attackSquad, Squad defendSquad)
        {
            _game.ClearScene();

            _units = new List<BattleUnit>();

            foreach (var attackSquadUnit in attackSquad.Units) {
                _units.Add(_visualSceneController.AddBattleUnit(attackSquadUnit, true));
            }

            foreach (var defendSquadUnit in defendSquad.Units) {
                _units.Add(_visualSceneController.AddBattleUnit(defendSquadUnit, false));
            }
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
        private void NextTurn()
        {
            do {
                if (_turnOrder.TryDequeue(out var nextUnit) && nextUnit.Unit.IsDead == false) {
                    CurrentUnitObject = nextUnit;
                    CurrentUnitObject.Unit.Effects.OnUnitTurn();

                    return;
                }
            } while (_turnOrder.Any());

            StartNewRound();
        }

        // Проверить, если ли враги.
        private bool IsBattleEnd()
        {
            return Units
                       .Where(u => u.Unit.IsDead == false)
                       .Select(u => u.Unit.Player)
                       .Distinct()
                       .Count() < 2;
        }


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

        public bool AttackUnit(BattleUnit targetUnitGameObject)
        {
            if (CanAttack(targetUnitGameObject) == false)
                return false;

            UnitActionBegin?.Invoke(this, new UnitActionBeginEventArgs(UnitActionType.Attack));

            CurrentUnitObject.Action = BattleAction.Attacking;

            _targetUnits = CurrentUnitObject.Unit.UnitType.FirstAttack.Reach == Reach.All
                ? GetUnitSquad(targetUnitGameObject)
                : new [] { targetUnitGameObject };

            BattleState = BattleState.BeforeTouch;

            return true;
        }

        public void Defend()
        {
            UnitActionBegin?.Invoke(this, new UnitActionBeginEventArgs(UnitActionType.Defend));

            CurrentUnitObject.Unit.Effects.AddMomentalEffect(new UnitMomentalEffect(UnitMomentalEffectType.Defended));

            BattleState = BattleState.Delay;
        }

        public void Wait()
        {
            UnitActionBegin?.Invoke(this, new UnitActionBeginEventArgs(UnitActionType.Wait));

            CurrentUnitObject.Unit.Effects.AddMomentalEffect(new UnitMomentalEffect(UnitMomentalEffectType.Waiting));
            // todo Перебросить в конец очереди.
            BattleState = BattleState.Delay;
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
    }
}