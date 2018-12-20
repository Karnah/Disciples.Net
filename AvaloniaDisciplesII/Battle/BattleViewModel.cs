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
        /// <summary>
        /// Отображается ли подробная информация о юните в данный момент.
        /// </summary>
        private bool _isUnitInfoShowing;
        /// <summary>
        /// Объект, над которым была зажата кнопка мыши.
        /// </summary>
        private GameObject _pressedObject;

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
            // Если отпустили ПКМ, то прекращаем отображать информацию о юните.
            if (args.ActionType == GameObjectActionType.RightButtonReleased) {
                GameObjectRightButtonReleased(args.GameObject);
                return;
            }

            // Если ПКМ зажата, то не меняем выбранного юнита до тех пор, пока не будет отпущена кнопка.
            if (_isUnitInfoShowing) {
                return;
            }

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
                case GameObjectActionType.RightButtonPressed:
                    GameObjectRightButtonPressed(args.GameObject);
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
            _pressedObject = gameObject;

            if (gameObject is ButtonObject button) {
                button.OnPressed();
            }
        }

        /// <summary>
        /// Обработчик события клика на игровом объекта.
        /// </summary>
        private void GameObjectClicked(GameObject gameObject)
        {
            // В том случае, если нажали кнопку на одном объекте, а отпустили на другом, то ничего не делаем.
            if (_pressedObject != gameObject)
                return;

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
        /// Обработать событие того, что на игровой объект нажали ПКМ.
        /// </summary>
        private void GameObjectRightButtonPressed(GameObject gameObject)
        {
            Unit unit = null;

            if (gameObject is BattleUnit battleUnit) {
                unit = battleUnit.Unit;
            }
            else if(gameObject is UnitPortraitObject unitPortrait) {
                unit = unitPortrait.Unit;
            }
            // todo Вообще, при наведении на портреты внизу по бокам тоже нужно показывать информацию.
            // Но сейчас они позиционируются только как картинки, а не как объекты.

            // На ПКМ мы показываем только информацию о выбранном юните.
            // Поэтому, если был выбран игровой объект, который никак не относится к юниту,
            // То ничего не делам.
            if (unit == null)
                return;

            _isUnitInfoShowing = true;
            _pressedObject = gameObject;
            InterfaceController.ShowDetailUnitInfo(TargetUnit);
        }

        /// <summary>
        /// Обработать событие того, что на игровой объект нажали ПКМ.
        /// </summary>
        private void GameObjectRightButtonReleased(GameObject gameObject)
        {
            if (!_isUnitInfoShowing)
                return;

            _isUnitInfoShowing = false;
            InterfaceController.StopShowDetailUnitInfo();

            // Если во время того, как отображалась информация о юните,
            // Курсор мыши был перемещён, то после того, как отпустили ПКМ, мы должны выделить нового юнита.
            if (_pressedObject != gameObject) {
                GameObjectUnselected(_pressedObject);
                GameObjectSelected(gameObject);
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
