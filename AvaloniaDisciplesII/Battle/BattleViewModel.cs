using System;
using System.Collections.Generic;

using ReactiveUI;

using AvaloniaDisciplesII.ViewModels;
using Engine;
using Engine.Battle.Contollers;
using Engine.Battle.GameObjects;
using Engine.Common.Controllers;
using Engine.Common.GameObjects;
using Engine.Common.Models;
using Engine.Models;

namespace AvaloniaDisciplesII.Battle
{
    public class BattleViewModel : PageViewModel
    {
        private readonly IGame _game;
        private readonly IAudioController _audioController;
        private readonly IBattleAttackController _battleAttackController;


        /// <summary>
        /// Позволяет заблокировать действия пользователя на время анимации.
        /// </summary>
        private bool _isAnimating;
        /// <summary>
        /// Юнит, чей ход активен на данный момент.
        /// </summary>
        private Unit _currentUnit;
        /// <summary>
        /// Юнит, который был выбран целью последним.
        /// </summary>
        private Unit _targetUnit;

        public BattleViewModel(
            IGame game,
            IMapVisual mapVisual,
            IAudioController audioController,
            IBattleAttackController battleAttackController,
            IBattleInterfaceController battleInterfaceController)
        {
            _game = game;
            _audioController = audioController;
            _battleAttackController = battleAttackController;

            MapVisual = mapVisual;
            InterfaceController = battleInterfaceController;

            // todo Библиотека слишком долго грузит и декодирует данные.
            // Для тестов убрал, потом нужно будет заменить на кроссплатформенное решение.
            //_audioService.PlayBackground("battle");

            OnUnitActionEnded();

            _game.GameObjectAction += OnGameObjectAction;

            _battleAttackController.UnitActionBegin += (sender, args) => OnUnitActionBegin();
            _battleAttackController.UnitActionEnded += (sender, args) => OnUnitActionEnded();
            _battleAttackController.BattleEnded += (sender, args) => OnBattleEnded();

            InterfaceController.Initialize();
        }

        /// <summary>
        /// Все объекты, которые отрисовываются на сцене.
        /// </summary>
        public IMapVisual MapVisual { get; }

        /// <summary>
        /// Юниты, которые находятся на сцене.
        /// </summary>
        public IReadOnlyList<BattleUnit> Units => _battleAttackController.Units;

        /// <summary>
        /// Менеджер для управления интерфейсом.
        /// </summary>
        public IBattleInterfaceController InterfaceController { get; }

        /// <summary>
        /// Юнит, чей ход активен на данный момент.
        /// </summary>
        public Unit CurrentUnit {
            get => _currentUnit;
            private set => this.RaiseAndSetIfChanged(ref _currentUnit, value);
        }

        /// <summary>
        /// Юнит, который был выбран целью последним.
        /// </summary>
        public Unit TargetUnit {
            get => _targetUnit;
            private set => this.RaiseAndSetIfChanged(ref _targetUnit, value);
        }


        /// <summary>
        /// Обработчик события воздействия с игровым объектом (наведение, клик мышью и т.д.).
        /// </summary>
        private void OnGameObjectAction(object o, GameObjectActionEventArgs args)
        {
            switch (args.ActionType) {
                case GameObjectActionType.Selected:
                    GameObjectSelected(args.GameObject);
                    break;
                case GameObjectActionType.Unselected:
                    GameObjectUnselected(args.GameObject);
                    break;
                case GameObjectActionType.LeftButtonPressed:
                    GameObjectPressed(args.GameObject);
                    break;
                case GameObjectActionType.LeftButtonReleased:
                    GameObjectClicked(args.GameObject);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Обработчик события, что игровой объект был выбран.
        /// </summary>
        private void GameObjectSelected(GameObject gameObject)
        {
            if (gameObject is BattleUnit battleUnit) {
                // Если выбрали кости юнита, то не нужно менять портрет.
                if (battleUnit.Unit.IsDead)
                    return;

                TargetUnit = battleUnit.Unit;
                InterfaceController.UpdateTargetUnit(battleUnit);
            }
            else if(gameObject is UnitPortraitObject unitPortrait) {
                TargetUnit = unitPortrait.Unit;

                var targetUnitObject = _battleAttackController.GetUnitObject(unitPortrait.Unit);
                InterfaceController.UpdateTargetUnit(targetUnitObject, false);
            }
            else if (gameObject is ButtonObject button) {
                button.OnSelected();
            }
        }

        /// <summary>
        /// Обработчик события, что с игрового объекта был смещён фокус.
        /// </summary>
        private void GameObjectUnselected(GameObject gameObject)
        {
            if (gameObject is BattleUnit) {
                InterfaceController.UpdateTargetUnit(null);
            }
            else if (gameObject is ButtonObject button) {
                button.OnUnselected();
            }
        }

        /// <summary>
        /// Обработчик события, что на объект нажали мышью.
        /// </summary>
        private void GameObjectPressed(GameObject gameObject)
        {
            if (gameObject is ButtonObject button) {
                button.OnPressed();
            }
        }

        /// <summary>
        /// Обработчик события клика на игровом объекта.
        /// </summary>
        private void GameObjectClicked(GameObject gameObject)
        {
            if (gameObject is BattleUnit targetUnitGameObject) {
                if (_isAnimating)
                    return;

                _battleAttackController.AttackUnit(targetUnitGameObject);
            }
            else if(gameObject is UnitPortraitObject unitPortrait) {
                if (_isAnimating)
                    return;

                var targetUnitObject = _battleAttackController.GetUnitObject(unitPortrait.Unit);
                _battleAttackController.AttackUnit(targetUnitObject);
            }
            else if (gameObject is ButtonObject button) {
                button.OnReleased();
            }
        }


        /// <summary>
        /// Заблокировать интерфейс на время действия юнита.
        /// </summary>
        private void OnUnitActionBegin()
        {
            _isAnimating = true;
        }

        /// <summary>
        /// Возникает, когда закончилась анимации атаки у юнита.
        /// </summary>
        private void OnUnitActionEnded()
        {
            CurrentUnit = _battleAttackController.CurrentUnitObject.Unit;
            _isAnimating = false;
        }

        /// <summary>
        /// Разблокировать интерфейс под конец битвы.
        /// </summary>
        private void OnBattleEnded()
        {
            _isAnimating = false;
        }
    }
}
