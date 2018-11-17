using System;
using System.Collections.Generic;
using System.Linq;

using Engine.Battle.Contollers;
using Engine.Battle.Enums;
using Engine.Battle.GameObjects;
using Engine.Battle.Models;
using Engine.Common.Controllers;
using Engine.Common.Enums.Units;
using Engine.Common.GameObjects;
using Engine.Common.Models;
using Engine.Extensions;
using Engine.Models;

namespace Engine.Implementation.Controllers
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
        /// Задержка после некоторых действия игрока, прежде чем ход перейдёт к следующему юниту.
        /// </summary>
        private const long ACTION_DELAY = 1500;


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
        /// Некоторые юниты могут атаковать дважды за ход.
        /// </summary>
        private bool _isSecondAttack = false;
        /// <summary>
        /// 
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

            if (BattleState == BattleState.Delay) {
                _delayTime -= args.TicksCount;
                if (_delayTime <= 0) {
                    _delayTime = null;
                    NextTurn();
                    BattleState = BattleState.WaitingAction;
                    UnitActionEnded?.Invoke(this, EventArgs.Empty);
                }

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

            // todo нужно научиться определять на каком фрейме происходит удар.
            if (curUnitAnimation.FramesCount - curUnitAnimation.FrameIndex > 12)
                return;

            var isAttacking = curUnit.HasEnemyAbility();
            foreach (var targetUnit in _targetUnits) {
                var unit = targetUnit.Unit;

                // Проверяем меткость юнита.
                var chanceAttack = RandomGenerator.Next(0, 100);
                if (chanceAttack > curUnit.UnitType.FirstAttack.Accuracy) {
                    unit.Effects.AddMomentalEffect(new UnitMomentalEffect(UnitMomentalEffectType.Miss));
                    continue;
                }

                // Если юнит атакует врага, а не лечит союзника, то у цели вызываем анимацию получения повреждений.
                if (isAttacking) {
                    targetUnit.Action = BattleAction.TakingDamage;

                    var damage = curUnit.UnitType.FirstAttack.DamagePower + RandomGenerator.Next(ATTACK_RANGE);
                    damage = (int) (damage * (1 - unit.UnitType.Armor / 100.0));

                    // Если юнит защитился, то урон уменьшается в два раза.
                    if (unit.Effects.ExistsBattleEffect(UnitBattleEffectType.Defend)) {
                        damage /= 2;
                    }

                    unit.ChangeHitPoints(-damage);
                    unit.Effects.AddMomentalEffect(new UnitMomentalEffect(UnitMomentalEffectType.Damaged));
                }
                else {
                    // todo пока только целители
                    var heal = curUnit.UnitType.FirstAttack.HealPower;
                    unit.ChangeHitPoints(heal);
                    unit.Effects.AddMomentalEffect(new UnitMomentalEffect(UnitMomentalEffectType.Healed));
                }

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
            if (CurrentUnitObject.Action == BattleAction.Waiting && _targetAnimations.All(ta => ta.IsDestroyed)) {
                _targetAnimations = new List<AnimationObject>();

                if (IsBattleEnd()) {
                    // todo Добавить что-то более подходящее
                    Console.WriteLine("Battle end");

                    BattleState = BattleState.BattleEnd;
                    BattleEnded?.Invoke(this, EventArgs.Empty);

                    return;
                }

                // Если юнит может атаковать дважды, и сейчас атаковал в первый раз, то не передаём ход дальше.
                if (CurrentUnitObject.Unit.UnitType.AttackTwice && _isSecondAttack == false) {
                    _isSecondAttack = true;
                }
                else {
                    _isSecondAttack = false;
                    NextTurn();
                }

                BattleState = BattleState.WaitingAction;
                UnitActionEnded?.Invoke(this, EventArgs.Empty);
            }
        }


        public BattleState BattleState { get; private set; } = BattleState.WaitingAction;

        public BattleUnit CurrentUnitObject { get; private set; }

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
                    .OrderByDescending(u => u.Unit.UnitType.FirstAttack.Initiative + RandomGenerator.Next(0, INITIATIVE_RANGE)));

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

            CurrentUnitObject.Unit.Effects.AddBattleEffect(new UnitBattleEffect(UnitBattleEffectType.Defend, 1));
            CurrentUnitObject.Unit.Effects.AddMomentalEffect(new UnitMomentalEffect(UnitMomentalEffectType.Defended));

            _delayTime = ACTION_DELAY;
            BattleState = BattleState.Delay;
        }

        public void Wait()
        {
            UnitActionBegin?.Invoke(this, new UnitActionBeginEventArgs(UnitActionType.Wait));

            CurrentUnitObject.Unit.Effects.AddMomentalEffect(new UnitMomentalEffect(UnitMomentalEffectType.Waiting));
            // todo Перебросить в конец очереди.

            _delayTime = ACTION_DELAY;
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
