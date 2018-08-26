using System.Collections.Generic;
using System.Windows.Input;

using ReactiveUI;

using AvaloniaDisciplesII.ViewModels;
using Engine.Battle.Contollers;
using Engine.Battle.GameObjects;
using Engine.Common.Controllers;
using Engine.Common.GameObjects;
using Engine.Common.Models;

namespace AvaloniaDisciplesII.Battle
{
    public class BattleViewModel : PageViewModel
    {
        private readonly IAudioController _audioController;
        private readonly IBattleAttackController _battleAttackController;
        private readonly IBattleInterfaceController _battleInterfaceController;


        /// <summary>
        /// Позволяет заблокировать действия пользователя на время анимации
        /// </summary>
        private bool _isAnimating;
        /// <summary>
        /// Юнит, чей ход активен на данный момент
        /// </summary>
        private Unit _currentUnit;
        /// <summary>
        /// Юнит, который был выбран целью последним
        /// </summary>
        private Unit _targetUnit;

        public BattleViewModel(IMapVisual mapVisual, IAudioController audioController, IBattleAttackController battleAttackController, IBattleInterfaceController battleInterfaceController)
        {
            MapVisual = mapVisual;
            _audioController = audioController;
            _battleAttackController = battleAttackController;
            _battleInterfaceController = battleInterfaceController;

            // todo Библиотека слишком долго грузит и декодирует данные
            // Для тестов убрал, потом нужно будет заменить на кроссплатформенное решение
            //_audioService.PlayBackground("battle");

            UnitSelectedOnPanelCommand = ReactiveCommand.Create<BattleUnit>(UnitSelectedOnPanel);
            GameObjectSelectedCommand = ReactiveCommand.Create<GameObject>(GameObjectSelected);
            GameObjectUnselectedCommand = ReactiveCommand.Create<GameObject>(GameObjectUnselected);
            GameObjectPressedCommand = ReactiveCommand.Create<GameObject>(GameObjectPressed);
            GameObjectClickedCommand = ReactiveCommand.Create<GameObject>(GameObjectClicked);

            OnUnitActionEnded();
            _battleAttackController.UnitActionBegin += (sender, args) => OnUnitActionBegin();
            _battleAttackController.UnitActionEnded += (sender, args) => OnUnitActionEnded();
            _battleAttackController.BattleEnded += (sender, args) => OnBattleEnded();

            _battleInterfaceController.Initialize();
        }


        public IMapVisual MapVisual { get; }

        public IReadOnlyList<BattleUnit> Units => _battleAttackController.Units;

        public IBattleInterfaceController InterfaceController => _battleInterfaceController;

        public Unit CurrentUnit {
            get => _currentUnit;
            private set => this.RaiseAndSetIfChanged(ref _currentUnit, value);
        }

        public Unit TargetUnit {
            get => _targetUnit;
            private set => this.RaiseAndSetIfChanged(ref _targetUnit, value);
        }


        public ICommand UnitSelectedOnPanelCommand { get; }

        public ICommand GameObjectSelectedCommand { get; }

        public ICommand GameObjectUnselectedCommand { get; }

        public ICommand GameObjectPressedCommand { get; }

        public ICommand GameObjectClickedCommand { get; }


        /// <summary>
        /// Обработчик события, что юнит выбран на панели
        /// </summary>
        private void UnitSelectedOnPanel(BattleUnit battleUnit)
        {
            TargetUnit = battleUnit.Unit;
            _battleInterfaceController.UpdateTargetUnit(battleUnit, false);
        }

        /// <summary>
        /// Обработчик события, что игровой объект был выбран
        /// </summary>
        private void GameObjectSelected(GameObject gameObject)
        {
            if (gameObject is BattleUnit battleUnit) {
                // Если выбрали кости юнита, то не нужно менять портрет
                if (battleUnit.Unit.IsDead)
                    return;

                TargetUnit = battleUnit.Unit;
                _battleInterfaceController.UpdateTargetUnit(battleUnit);
            }
            else if (gameObject is ButtonObject button) {
                button.OnSelected();
            }
        }

        /// <summary>
        /// Обработчик события, что с игрового объекта был смещён фокус
        /// </summary>
        private void GameObjectUnselected(GameObject gameObject)
        {
            if (gameObject is BattleUnit) {
                _battleInterfaceController.UpdateTargetUnit(null);
            }
            else if (gameObject is ButtonObject button) {
                button.OnUnselected();
            }
        }

        /// <summary>
        /// Обработчик события, что на объект нажали мышью
        /// </summary>
        private void GameObjectPressed(GameObject gameObject)
        {
            if (gameObject is ButtonObject button) {
                button.OnPressed();
            }
        }

        /// <summary>
        /// Обработчик события клика на игровом объекта
        /// </summary>
        private void GameObjectClicked(GameObject gameObject)
        {
            if (gameObject is BattleUnit targetUnitGameObject) {
                if (_isAnimating)
                    return;

                _battleAttackController.AttackUnit(targetUnitGameObject);
            }
            else if (gameObject is ButtonObject button) {
                button.OnReleased();
            }
        }


        /// <summary>
        /// Заблокировать интерфейс на время действия юнита
        /// </summary>
        private void OnUnitActionBegin()
        {
            _isAnimating = true;
        }

        /// <summary>
        /// Возникает, когда закончилась анимации атаки у юнита
        /// </summary>
        private void OnUnitActionEnded()
        {
            CurrentUnit = _battleAttackController.CurrentUnitObject.Unit;
            _isAnimating = false;
        }

        /// <summary>
        /// Разблокировать интерфейс под конец битвы
        /// </summary>
        private void OnBattleEnded()
        {
            _isAnimating = false;
        }
    }
}
