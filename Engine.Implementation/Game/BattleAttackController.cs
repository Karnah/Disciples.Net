using System;
using System.Collections.Generic;
using System.Linq;

using Unity;

using Engine.Battle.Enums;
using Engine.Battle.Providers;
using Engine.Enums;
using Engine.Extensions;
using Engine.Interfaces;
using Engine.Models;

namespace Engine.Implementation.Game
{
    public class BattleAttackController : IBattleAttackController
    {
        /// <summary>
        /// Разброс инициативы при вычислении очередности
        /// </summary>
        private const int INITIATIVE_RANGE = 5;
        /// <summary>
        /// Разброс атаки при ударе
        /// </summary>
        private const int ATTACK_RANGE = 5;

        /// <summary>
        /// Слой, который перекрывает всех юнитов
        /// </summary>
        private const int ABOVE_ALL_UNITS_LAYER = 100 * 4;

        private readonly IUnityContainer _container;
        private readonly IGame _game;
        private readonly IMapVisual _mapVisual;

        private List<BattleUnit> _units;
        private List<FrameAnimationObject> _targetAnimations;
        private IReadOnlyCollection<BattleUnit> _targetUnits;


        private AttackState _attackState = AttackState.WaitingAction;

        /// <summary>
        /// Очередность хода юнитов
        /// </summary>
        private Queue<BattleUnit> _turnOrder;

        /// <summary>
        /// Некоторые юниты могут атаковать дважды за ход
        /// </summary>
        private bool _isSecondAttack = false;


        public BattleAttackController(IUnityContainer container, IGame game, IMapVisual mapVisual, Squad attackSquad, Squad defendSquad)
        {
            _container = container;
            _game = game;
            _mapVisual = mapVisual;

            _targetAnimations = new List<FrameAnimationObject>();

            ArrangeUnits(attackSquad, defendSquad);
            StartNewRound();

            _game.SceneEndUpdating += OnUpdate;
        }


        private void OnUpdate(object sender, EventArgs eventArgs)
        {
            if (_attackState == AttackState.WaitingAction || _attackState == AttackState.BattleEnd)
                return;

            if (_attackState == AttackState.BeforeTouch) {
                BeforeTouch();
                return;
            }

            if (_attackState == AttackState.AfterTouch) {
                AfterTouch();
                return;
            }
        }

        /// <summary>
        /// Проверить, что анимация текущего юнита дошла да крайней точки и юнит готов вернуться в начально положение
        /// Если это произошло, то пересчитываем урон и добавляем на сцену анимации атаки
        /// </summary>
        private void BeforeTouch()
        {
            var curUnitAnimation = CurrentUnitGameObject.BattleUnitAnimationComponent;
            var curUnit = CurrentUnitGameObject.Unit;

            // todo нужно научиться определять на каком фрейме происходит удар
            if (curUnitAnimation.FramesCount - curUnitAnimation.FrameIndex <= 12) {
                var isAttacking = curUnit.HasEnemyAbility();
                foreach (var targetUnit in _targetUnits) {
                    // Проверяем меткость юнита
                    var chanceAttack = RandomGenerator.Next(0, 100);
                    if (chanceAttack > curUnit.UnitType.FirstAttack.Accuracy)
                        continue;

                    // Если юнит атакует врага, а не лечит союзника, то у цели вызываем анимацию получения повреждений
                    if (isAttacking) {
                        targetUnit.BattleObjectComponent.Action = BattleAction.TakingDamage;

                        var damage = curUnit.UnitType.FirstAttack.DamagePower + RandomGenerator.Next(ATTACK_RANGE);
                        damage = (int) (damage * (1 - targetUnit.Unit.UnitType.Armor / 100.0));

                        targetUnit.Unit.ChangeHitPoints(-damage);
                    }
                    else {
                        // todo пока только целители
                        var heal = curUnit.UnitType.FirstAttack.HealPower;
                        targetUnit.Unit.ChangeHitPoints(heal);
                    }

                    // Если есть анимация, применяемая к юниту, то добавляем её на сцену
                    if (curUnitAnimation.BattleUnitAnimation.TargetAnimation?.IsSingle == true) {
                        var targetAnimation = new FrameAnimationObject(
                            _mapVisual,
                            curUnitAnimation.BattleUnitAnimation.TargetAnimation.Frames,
                            targetUnit.BattleObjectComponent.Position.X,
                            targetUnit.BattleObjectComponent.Position.Y,
                            targetUnit.BattleUnitAnimationComponent.Layer + 2);

                        _targetAnimations.Add(targetAnimation);
                        _game.CreateObject(targetAnimation);
                    }
                }

                if (curUnitAnimation.BattleUnitAnimation.TargetAnimation?.IsSingle == false) {
                    // Центр анимации будет приходиться на середину между первым и вторым рядом
                    // Дополнительно смещаемся, если это атака на защитников
                    var offsetX = 0.5;
                    if (_targetUnits.First().BattleObjectComponent.Direction == BattleDirection.Defender) {
                        offsetX += 2;
                    }

                    var (x, y) = GameInfo.OffsetCoordinates(offsetX, 1);
                    var areaAnimation = new FrameAnimationObject(
                        _mapVisual,
                        curUnitAnimation.BattleUnitAnimation.TargetAnimation.Frames,
                        x,
                        y,
                        ABOVE_ALL_UNITS_LAYER);

                    _targetAnimations.Add(areaAnimation);
                    _game.CreateObject(areaAnimation);
                }

                _attackState = AttackState.AfterTouch;
            }
        }

        /// <summary>
        /// Удалить со сцены завершившиеся анимации
        /// Завершить атаку, если все анимации завершились
        /// </summary>
        private void AfterTouch()
        {
            var curUnitObject = CurrentUnitGameObject.BattleObjectComponent;
            var curUnitAnimation = CurrentUnitGameObject.BattleUnitAnimationComponent;

            // Переводим юнита в ожидание если его анимация закончилась
            if (curUnitObject.Action == BattleAction.Attacking &&
                curUnitAnimation.FrameIndex == curUnitAnimation.FramesCount - 1) {
                curUnitObject.Action = BattleAction.Waiting;
            }

            // Уничтожаем анимации, которые закончили своё выполнение
            foreach (var frameAnimationObject in _targetAnimations) {
                if (frameAnimationObject.IsDestroyed)
                    continue;

                var animComponent = frameAnimationObject.FrameAnimationComponent;
                if (animComponent.FrameIndex == animComponent.FramesCount - 1) {
                    _game.DestroyObject(frameAnimationObject);
                }
            }

            // Обрабатываем конец анимации получения повреждений для юнитов
            foreach (var targetUnitGameObject in _targetUnits) {
                var tarUnit = targetUnitGameObject.Unit;
                var tarUnitObject = targetUnitGameObject.BattleObjectComponent;
                var tarUnitAnim = targetUnitGameObject.BattleUnitAnimationComponent;

                if (tarUnitObject.Action == BattleAction.TakingDamage &&
                    tarUnitAnim.FrameIndex == tarUnitAnim.FramesCount - 1) {

                    tarUnitObject.Action = BattleAction.Waiting;

                    // В начале просто добавлеяем анимацию смерти юнита
                    // Только спустя несколько тиков он превратится в кучку костей
                    if (tarUnit.HitPoints == 0) {
                        tarUnit.IsDead = true;

                        var deathAnimation = new FrameAnimationObject(
                            _mapVisual,
                            targetUnitGameObject.BattleUnitAnimationComponent.BattleUnitAnimation.DeathFrames,
                            targetUnitGameObject.BattleObjectComponent.Position.X,
                            targetUnitGameObject.BattleObjectComponent.Position.Y,
                            targetUnitGameObject.BattleUnitAnimationComponent.Layer + 2);

                        _targetAnimations.Add(deathAnimation);
                        _game.CreateObject(deathAnimation);
                    }

                    continue;
                }

                // Отображаем мертвого юнита как кости на сцене
                if (tarUnit.IsDead &&
                    tarUnitObject.Action == BattleAction.Waiting &&
                    tarUnitAnim.FrameIndex == tarUnitAnim.FramesCount / 2) {
                    tarUnitObject.Action = BattleAction.Dead;
                }
            }


            // Если активных действий нет, заканчиваем
            if (curUnitObject.Action == BattleAction.Waiting &&
                _targetAnimations.Any(ta => ta.IsDestroyed == false) == false) {
                _targetAnimations = new List<FrameAnimationObject>();

                if (IsBattleEnd()) {
                    // todo Добавить что-то более подходящее
                    Console.WriteLine("Battle end");

                    _attackState = AttackState.BattleEnd;

                    return;
                }

                // Если юнит может атаковать дважды, и сейчас атаковал в первый раз, то не передаём ход дальше
                if (CurrentUnitGameObject.Unit.UnitType.AttackTwice && _isSecondAttack == false) {
                    _isSecondAttack = true;
                }
                else {
                    _isSecondAttack = false;
                    NextTurn();
                }

                _attackState = AttackState.WaitingAction;
                AttackEnded?.Invoke(this, EventArgs.Empty);
            }
        }



        public BattleUnit CurrentUnitGameObject { get; private set; }

        public IReadOnlyList<BattleUnit> Units => _units;


        public event EventHandler AttackEnded;


        /// <summary>
        /// Расставить юнитов по позициям
        /// </summary>
        private void ArrangeUnits(Squad attackSquad, Squad defendSquad)
        {
            _game.ClearScene();

            _units = new List<BattleUnit>();
            var bitmapResources = _container.Resolve<IBattleUnitResourceProvider>();
            var mapVisual = _container.Resolve<IMapVisual>();

            foreach (var attackSquadUnit in attackSquad.Units) {
                var unit = new BattleUnit(
                    mapVisual,
                    bitmapResources,
                    attackSquadUnit,
                    attackSquadUnit.SquadLinePosition,
                    attackSquadUnit.SquadFlankPosition,
                    BattleDirection.Attacker);

                _units.Add(unit);
                _game.CreateObject(unit);
            }


            foreach (var defendSquadUnit in defendSquad.Units) {
                var unit = new BattleUnit(
                    mapVisual,
                    bitmapResources,
                    defendSquadUnit,
                    ((defendSquadUnit.SquadLinePosition + 1) & 1) + 2,
                    defendSquadUnit.SquadFlankPosition,
                    BattleDirection.Defender);

                _units.Add(unit);
                _game.CreateObject(unit);
            }
        }



        // Начать новый раунд
        private void StartNewRound()
        {
            _turnOrder = new Queue<BattleUnit>(
                _units
                    .Where(u => u.Unit.IsDead == false)
                    .OrderByDescending(u => u.Unit.UnitType.FirstAttack.Initiative + RandomGenerator.Next(0, INITIATIVE_RANGE)));

            CurrentUnitGameObject = _turnOrder.Dequeue();
        }

        // Передать ход следующему юниту
        private void NextTurn()
        {
            do {
                if (_turnOrder.TryDequeue(out var nextUnit) && nextUnit.Unit.IsDead == false) {
                    CurrentUnitGameObject = nextUnit;
                    return;
                }
            } while (_turnOrder.Any());

            StartNewRound();
        }

        // Проверить, если ли враги
        private bool IsBattleEnd()
        {
            return Units
                       .Where(u => u.Unit.IsDead == false)
                       .Select(u => u.Unit.Player)
                       .Distinct()
                       .Count() < 2;
        }


        #region AttackMethods

        public bool CanAttack(BattleUnit targetUnitGameObject)
        {
            var currentUnit = CurrentUnitGameObject.Unit;
            var targetUnit = targetUnitGameObject.Unit;

            // Лекарь не может атаковать врага, а воин не может атаковать союзника
            if (currentUnit.Player == targetUnit.Player && currentUnit.HasAllyAbility() == false ||
                currentUnit.Player != targetUnit.Player && currentUnit.HasEnemyAbility() == false) {
                return false;
            }

            // Если юнит может атаковать только ближайшего, то проверяем препятствия
            if (currentUnit.UnitType.FirstAttack.Reach == Reach.Adjacent) {
                // Если атакующий юнит находится сзади и есть линия союзников впереди
                var currentSquad = GetUnitSquad(CurrentUnitGameObject);
                if (currentUnit.SquadLinePosition == 0 && IsFirstLineEmpty(currentSquad) == false)
                    return false;

                // Если враг находится сзади, то проверяем, что нет первой вражеской линии
                var targetSquad = GetUnitSquad(targetUnitGameObject);
                if (targetUnit.SquadLinePosition == 0 && IsFirstLineEmpty(targetSquad) == false)
                    return false;

                // Проверка, может ли юнит дотянуться до врага
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

            CurrentUnitGameObject.BattleObjectComponent.Action = BattleAction.Attacking;

            _targetUnits = CurrentUnitGameObject.Unit.UnitType.FirstAttack.Reach == Reach.All
                ? GetUnitSquad(targetUnitGameObject)
                : new [] { targetUnitGameObject };

            _attackState = AttackState.BeforeTouch;

            return true;
        }


        /// <summary>
        /// Получить весь отряд указанного юнита
        /// </summary>
        private IReadOnlyCollection<BattleUnit> GetUnitSquad(BattleUnit battleUnit)
        {
            return Units.Where(u => u.Unit.Player == battleUnit.Unit.Player && u.Unit.IsDead == false).ToList();
        }


        /// <summary>
        /// Проверить, свободна ли первая линия в отряде
        /// </summary>
        private static bool IsFirstLineEmpty(IReadOnlyCollection<BattleUnit> squad)
        {
            return squad.Any(u => u.Unit.SquadLinePosition == 1 &&
                                  u.Unit.IsDead == false) == false;
        }

        /// <summary>
        /// Проверить, можно ли атаковать цель в зависимости от расположения на фланге
        /// </summary>
        private static bool CanAttackOnFlank(int currentUnitFlankPosition, int targetUnitFlankPosition, int targetUnitLinePosition, IReadOnlyCollection<BattleUnit> targetSquad)
        {
            // Если юниты находятся по разные стороны флагов и занят вражеский центр или соседняя с атакующим клетка, то атаковать нельзя
            if (Math.Abs(currentUnitFlankPosition - targetUnitFlankPosition) > 1 &&
                (IsPlaceEmpty(targetSquad, targetUnitLinePosition, 1) == false || IsPlaceEmpty(targetSquad, targetUnitLinePosition, currentUnitFlankPosition) == false))
                return false;

            return true;
        }

        /// <summary>
        /// Проверить, свободна ли клетка на арене
        /// </summary>
        private static bool IsPlaceEmpty(IReadOnlyCollection<BattleUnit> squad, int line, int flank)
        {
            return squad.Any(u => u.Unit.SquadLinePosition == line &&
                                  u.Unit.SquadFlankPosition == flank &&
                                  u.Unit.IsDead == false) == false;
        }

        #endregion


        private enum AttackState
        {
            WaitingAction,

            AfterTouch,

            BeforeTouch,

            BattleEnd
        }
    }
}
