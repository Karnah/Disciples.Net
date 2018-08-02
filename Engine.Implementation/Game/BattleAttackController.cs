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

using Action = System.Action;

namespace Engine.Implementation.Game
{
    public class BattleAttackController : IBattleAttackController
    {
        /// <summary>
        /// Разброс инициативы при вычислении очередности
        /// </summary>
        private const int InitiativeRange = 5;

        private readonly IUnityContainer _container;
        private readonly IGame _game;

        private List<BattleUnit> _units;

        /// <summary>
        /// Очередность хода юнитов
        /// </summary>
        private Queue<BattleUnit> _turnOrder;

        /// <summary>
        /// Некоторые юниты могут атаковать дважды за ход
        /// </summary>
        private bool _isSecondAttack = false;


        public BattleAttackController(IUnityContainer container, IGame game, Squad attackSquad, Squad defendSquad)
        {
            _container = container;
            _game = game;

            ArrangeUnits(attackSquad, defendSquad);
            StartNextRound();
        }


        public BattleUnit CurrentUnitGameObject { get; private set; }

        public IReadOnlyList<BattleUnit> Units => _units;


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



        private void StartNextRound()
        {
            _turnOrder = new Queue<BattleUnit>(
                _units.OrderByDescending(u => u.Unit.UnitType.FirstAttack.Initiative + RandomGenerator.Next(0, InitiativeRange)));

            CurrentUnitGameObject = _turnOrder.Dequeue();
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
                // Проверка, может ли юнит дотянуть до врага, несмотря на возможное наличие линий между ними
                var targetSquad = GetUnitSquad(targetUnitGameObject);
                if (CanAttackOnFlank(
                        currentUnit.SquadFlankPosition,
                        targetUnit.SquadFlankPosition,
                        targetUnit.SquadLinePosition,
                        targetSquad) == false)
                    return false;

                // Если атакующий юнит находится сзади и есть линия союзников впереди
                var currentSquad = GetUnitSquad(CurrentUnitGameObject);
                if (currentUnit.SquadLinePosition == 0 && IsFirstLineEmpty(currentSquad) == false)
                    return false;

                // Если враг находится сзади, то проверяем, что нет первой вражеской линии
                if (targetUnit.SquadLinePosition == 0 && IsFirstLineEmpty(targetSquad) == false)
                    return false;
            }

            return true;
        }

        public bool AttackUnit(BattleUnit targetUnitGameObject, Action onAttackEnd)
        {
            if (CanAttack(targetUnitGameObject) == false)
                return false;

            var battleUnits = CurrentUnitGameObject.Unit.UnitType.FirstAttack.Reach == Reach.All
                ? GetUnitSquad(targetUnitGameObject)
                : new [] { targetUnitGameObject };

            CurrentUnitGameObject.AttackUnits(battleUnits, () => {
                // Если юнит может атаковать дважды, и сейчас атаковал в первый раз, то не передаём ход дальше
                if (CurrentUnitGameObject.Unit.UnitType.AttackTwice && _isSecondAttack == false) {
                    _isSecondAttack = true;
                }
                else {
                    if (_turnOrder.Any() == false) {
                        StartNextRound();
                    }
                    else {
                        CurrentUnitGameObject = _turnOrder.Dequeue();
                    }
                }

                onAttackEnd.Invoke();
            });

            return true;
        }


        /// <summary>
        /// Получить весь отряд указанного юнита
        /// </summary>
        private IReadOnlyCollection<BattleUnit> GetUnitSquad(BattleUnit battleUnit)
        {
            return Units.Where(u => u.Unit.Player == battleUnit.Unit.Player).ToList();
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
    }
}
