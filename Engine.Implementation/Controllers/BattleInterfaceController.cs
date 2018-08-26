using System;
using System.Collections.Generic;
using System.Linq;

using Avalonia.Media;
using Avalonia.Media.Imaging;
using ReactiveUI;

using Engine.Battle.Contollers;
using Engine.Battle.Enums;
using Engine.Battle.GameObjects;
using Engine.Battle.Providers;
using Engine.Common.Controllers;
using Engine.Common.Enums.Units;
using Engine.Common.GameObjects;
using Engine.Common.Models;
using Engine.Extensions;

namespace Engine.Implementation.Controllers
{
    // Наследование от ReactiveObject только из-за RightPanelUnits, так как они сделаны не через IMapVisual
    // Нужно сгрести всё под одну гребёнку
    public class BattleInterfaceController : ReactiveObject, IBattleInterfaceController
    {
        /// <summary>
        /// Слой для расположения интерфейса
        /// </summary>
        private const int INTERFACE_LAYER = 1000;

        private readonly IGame _game;
        private readonly IVisualSceneController _visualSceneController;
        private readonly IBattleAttackController _attackController;
        private readonly IBattleInterfaceProvider _interfaceProvider;
        private readonly IBattleResourceProvider _battleResourceProvider;

        private VisualObject _currentUnitFace;
        private VisualObject _targetUnitFace;
        private BattleUnit _targetUnitObject;

        /// <summary>
        /// Игровой объект, отрисовывающий на текущем юните аманицию выделения
        /// </summary>
        private AnimationObject _selectionAnimation;
        /// <summary>
        /// Игровые объекты, которые отрисовывают анимации цели на юнитах-целях
        /// </summary>
        private IList<AnimationObject> _targetAnimations;

        private IReadOnlyCollection<BattleUnit> _rightPanelUnits;
        /// <summary>
        /// Изменено ли отображение юнитов на панели
        /// </summary>
        private bool _isRightUnitPanelReflected;
        /// <summary>
        /// Анимации-рамки, которые отрисовываются на панели с юнитами
        /// </summary>
        private IList<AnimationObject> _unitPanelAnimations;

        private ToggleButtonObject _reflectUnitPanelButton;
        private ButtonObject _defendButton;
        private ButtonObject _retreatButton;
        private ButtonObject _waitButton;
        private ButtonObject _instantResolveButton;
        private ToggleButtonObject _autoBattleButton;

        public BattleInterfaceController(IGame game, IVisualSceneController visualSceneController, IBattleAttackController battleAttackController,
            IBattleInterfaceProvider battleInterfaceProvider, IBattleResourceProvider battleResourceProvider)
        {
            _game = game;
            _visualSceneController = visualSceneController;
            _attackController = battleAttackController;
            _interfaceProvider = battleInterfaceProvider;
            _battleResourceProvider = battleResourceProvider;

            _attackController.UnitActionBegin += OnUnitActionBegin;
            _attackController.UnitActionEnded += OnUnitActionEnded;
            _attackController.BattleEnded += OnBattleEnded;
        }


        public Bitmap Battleground => _interfaceProvider.Battleground;

        public Bitmap PanelSeparator => _interfaceProvider.PanelSeparator;

        public Bitmap DeathSkull => _interfaceProvider.DeathSkull;

        public IReadOnlyCollection<BattleUnit> RightPanelUnits
        {
            get => _rightPanelUnits;
            set => this.RaiseAndSetIfChanged(ref _rightPanelUnits, value);
        }


        public void Initialize()
        {
            InitializeStaticImages();
            InitializeButtons();

            AttachSelectedAnimation(_attackController.CurrentUnitObject);
            InitializeUnitsOnRightPanel();
            InitializeAnimationsOnRightUnitsPanel();
        }

        /// <summary>
        /// Разместить на сцене статические картинки
        /// </summary>
        private void InitializeStaticImages()
        {
            // Нижнюю панель распологаем 1 слое, чтобы прозрачная часть не перекрывала нижнего юнита
            _visualSceneController.AddVisual(_interfaceProvider.BottomPanel, 0, GameInfo.OriginalHeight - _interfaceProvider.BottomPanel.PixelHeight, 1);
            _visualSceneController.AddVisual(_interfaceProvider.RightPanel, GameInfo.OriginalWidth - _interfaceProvider.RightPanel.PixelWidth, 30, INTERFACE_LAYER);

            var currentUnitBattleFace = _attackController.CurrentUnitObject.Unit.UnitType.BattleFace;
            _currentUnitFace = _visualSceneController.AddVisual(
                currentUnitBattleFace,
                0,
                GameInfo.OriginalHeight - currentUnitBattleFace.PixelHeight - 5,
                INTERFACE_LAYER + 1);

            // Используем размеры лица текущего юнита, так как юнит-цель не инициализируется в начале боя
            _targetUnitFace = _visualSceneController.AddVisual(
                null,
                currentUnitBattleFace.PixelWidth,
                currentUnitBattleFace.PixelHeight,
                GameInfo.OriginalWidth - currentUnitBattleFace.PixelWidth,
                GameInfo.OriginalHeight - currentUnitBattleFace.PixelHeight - 5,
                INTERFACE_LAYER + 1);
            // todo Добавить перегрузку в метод?
            _targetUnitFace.Transform = new ScaleTransform(-1, 1);
        }

        /// <summary>
        /// Разместить на сцене кнопки
        /// </summary>
        private void InitializeButtons()
        {
            _reflectUnitPanelButton = _visualSceneController.AddToggleButton(_interfaceProvider.ToggleRightButton,
                () => {
                    _isRightUnitPanelReflected = !_isRightUnitPanelReflected;
                    InitializeUnitsOnRightPanel();
                    InitializeAnimationsOnRightUnitsPanel();
                }, 633, 402, INTERFACE_LAYER + 2);

            _defendButton = _visualSceneController.AddButton(_interfaceProvider.DefendButton,
                () => { _attackController.Defend(); }, 380, 504, INTERFACE_LAYER + 2);

            _retreatButton = _visualSceneController.AddButton(_interfaceProvider.RetreatButton, () => {
                //todo
            }, 343, 524, INTERFACE_LAYER + 2);

            _waitButton = _visualSceneController.AddButton(_interfaceProvider.WaitButton,
                () => { _attackController.Wait(); }, 419, 524, INTERFACE_LAYER + 2);

            _instantResolveButton = _visualSceneController.AddButton(_interfaceProvider.InstantResolveButton, () => {
                // todo
            }, 359, 563, INTERFACE_LAYER + 2);

            _autoBattleButton = _visualSceneController.AddToggleButton(_interfaceProvider.AutoBattleButton, () => {
                // todo
            }, 403, 563, INTERFACE_LAYER + 2);


            ActivateButtons(_reflectUnitPanelButton, _defendButton, _retreatButton, _waitButton, _instantResolveButton, _autoBattleButton);
        }


        public void UpdateTargetUnit(BattleUnit targetUnit, bool animateTarget = true)
        {
            if (targetUnit == null) {
                DetachTargetAnimations();
                _targetUnitObject = null;

                return;
            }

            _targetUnitObject = targetUnit;
            _targetUnitFace.Bitmap = targetUnit.Unit.UnitType.BattleFace;

            var isInterfaceActive = _attackController.BattleState == BattleState.WaitingAction ||
                                    _attackController.BattleState == BattleState.BattleEnd;
            if (animateTarget && isInterfaceActive)
                SelectTargetUnits();
        }


        private void OnUnitActionBegin(object sender, EventArgs eventArgs)
        {
            DisableButtons(_reflectUnitPanelButton, _defendButton, _retreatButton, _waitButton);
            DetachSelectedAnimation();
            DetachTargetAnimations();

            // Перед атакой восстанавливаем список юнитов, которые должны отображаться на панели
            // После этого очищаем все анимации
            if (_isRightUnitPanelReflected) {
                _isRightUnitPanelReflected = false;
                InitializeUnitsOnRightPanel();
            }
            CleanAnimationsOnRightUnitsPanel();
        }

        private void OnUnitActionEnded(object sender, EventArgs eventArgs)
        {
            ActivateButtons(_reflectUnitPanelButton, _defendButton, _retreatButton, _waitButton);
            AttachSelectedAnimation(_attackController.CurrentUnitObject);
            InitializeUnitsOnRightPanel();
            InitializeAnimationsOnRightUnitsPanel();

            if (_targetUnitObject != null)
                SelectTargetUnits();
        }

        private void OnBattleEnded(object sender, EventArgs eventArgs)
        {
            _defendButton.Destroy();
            _retreatButton.Destroy();
            _retreatButton.Destroy();
            _waitButton.Destroy();
            _instantResolveButton.Destroy();
            _autoBattleButton.Destroy();

            // todo Создать две новые кнопки - выйти и выйти и открыть интерфейс


            // todo на панели необходимо выводить отряд победителя
            InitializeUnitsOnRightPanel();
            if (_targetUnitObject != null)
                SelectTargetUnits();
        }


        /// <summary>
        /// Отобразить анимацию выделения на текущем юните
        /// </summary>
        private void AttachSelectedAnimation(BattleUnit battleUnit)
        {
            if (_currentUnitFace != null)
                _currentUnitFace.Bitmap = battleUnit.Unit.UnitType.BattleFace;

            if (_selectionAnimation != null)
                DetachSelectedAnimation();

            var frames = _battleResourceProvider.GetBattleAnimation(
                battleUnit.Unit.UnitType.SizeSmall
                    ? "MRKCURSMALLA"
                    : "MRKCURLARGEA");


            // Задаём смещение 190. Возможно, стоит вычислять высоту юнита или что-то в этом роде
            _selectionAnimation = _visualSceneController.AddAnimation(frames, battleUnit.X, battleUnit.Y + 190, 1);
        }

        /// <summary>
        /// Скрыть анимацию выделения на текущем юните
        /// </summary>
        private void DetachSelectedAnimation()
        {
            _game.DestroyObject(_selectionAnimation);
            _selectionAnimation = null;
        }


        /// <summary>
        /// Отобразить анимацию выделения цели исходя из типа атаки текущего юнита
        /// </summary>
        private void SelectTargetUnits()
        {
            if (_targetUnitObject == null)
                return;

            var currentUnit = _attackController.CurrentUnitObject.Unit;
            var targetUnit = _targetUnitObject.Unit;

            if (_targetUnitFace != null)
                _targetUnitFace.Bitmap = targetUnit.UnitType.BattleFace;

            if (targetUnit.IsDead)
                return;

            // Если текущий юнит может атаковать только одну цель,
            // то всегда будет выделена только одна цель
            if (currentUnit.UnitType.FirstAttack.Reach != Reach.All) {
                AttachTargetAnimations(_targetUnitObject);
                return;
            }


            BattleUnit[] targetUnits;

            // Если юнит применяет способность на союзников (например, лекарь), то при наведении на союзника, будут выделяться все
            // Также наооборот, если юнит применяет способность на врагов, то выделятся все враги
            // Иначе, как например, лекарь при наведении на врага будет выделять только 1 врага
            if (currentUnit.Player == targetUnit.Player && currentUnit.HasAllyAbility() ||
                currentUnit.Player != targetUnit.Player && currentUnit.HasEnemyAbility()) {
                targetUnits = _attackController.Units
                    .Where(u => u.Unit.Player == targetUnit.Player && u.Unit.IsDead == false)
                    .ToArray();
            }
            else {
                targetUnits = new[] { _targetUnitObject };
            }

            AttachTargetAnimations(targetUnits);
        }

        /// <summary>
        /// Отобразить анимацию выделения цели на указанных юнитах
        /// </summary>
        private void AttachTargetAnimations(params BattleUnit[] battleUnits)
        {
            if (_targetAnimations != null)
                DetachTargetAnimations();

            _targetAnimations = new List<AnimationObject>(battleUnits.Length);
            foreach (var battleUnit in battleUnits)
            {
                var frames = _battleResourceProvider.GetBattleAnimation(
                    battleUnit.Unit.UnitType.SizeSmall
                        ? "MRKSMALLA"
                        : "MRKLARGEA");


                // Задаём смещение 190. Возможно, стоит вычислять высоту юнита или что-то в этом роде
                var targetAnimation = _visualSceneController.AddAnimation(frames, battleUnit.X, battleUnit.Y + 190, 1);
                _targetAnimations.Add(targetAnimation);
            }
        }

        /// <summary>
        /// Скрыть анимации выделения цели на всех юнитах
        /// </summary>
        private void DetachTargetAnimations()
        {
            if (_targetAnimations == null)
                return;

            foreach (var targetAnimation in _targetAnimations) {
                _game.DestroyObject(targetAnimation);
            }

            _targetAnimations = null;
        }


        /// <summary>
        /// Определить юнитов, которые будут отображатся на панели
        /// </summary>
        private void InitializeUnitsOnRightPanel()
        {
            var currentUnit = _attackController.CurrentUnitObject.Unit;

            var showEnemies = currentUnit.HasEnemyAbility();
            if (_isRightUnitPanelReflected)
                showEnemies = !showEnemies;

            RightPanelUnits = _attackController.Units
                .Where(u => showEnemies && u.Unit.Player != currentUnit.Player ||
                            showEnemies == false && u.Unit.Player == currentUnit.Player)
                .ToList();
        }

        /// <summary>
        /// Получить координаты на сцене для портрета юнита
        /// </summary>
        private static (double X, double Y) GetRightUnitPanelPosition(int linePosition, int flankPosition, BattleDirection unitDirection)
        {
            // Защищающиеся на правой панели распологаются ли справа налево,
            // А атакающие слева направо
            var lineOffset = unitDirection == BattleDirection.Defender
                ? linePosition
                : (linePosition + 1) % 2;

            return (
                642 + (1 - lineOffset) * 79,
                83 + (2 - flankPosition) * 106
            );
        }

        /// <summary>
        /// Разместить анимации, которые показывают какие юниты доступны для атаки, на правой панели
        /// </summary>
        private void InitializeAnimationsOnRightUnitsPanel()
        {
            if (_unitPanelAnimations != null)
                CleanAnimationsOnRightUnitsPanel();

            var currentUnit = _attackController.CurrentUnitObject.Unit;
            _unitPanelAnimations = new List<AnimationObject>();

            // Если отображается отряд текущего юнита, то нужно его выделить на панели
            if (currentUnit.Player == RightPanelUnits.First().Unit.Player) {
                var position = GetRightUnitPanelPosition(currentUnit.SquadLinePosition, currentUnit.SquadFlankPosition, _attackController.CurrentUnitObject.Direction);

                _unitPanelAnimations.Add(
                    _visualSceneController.AddAnimation(
                        _interfaceProvider.GetUnitSelectionBorder(currentUnit.UnitType.SizeSmall),
                        position.X,
                        position.Y,
                        INTERFACE_LAYER + 2));
            }

            // Если юнит бьёт по площади и цель юнита - отображаемый отряд, то добавляем одну большую рамку
            if (currentUnit.UnitType.FirstAttack.Reach == Reach.All &&
                RightPanelUnits.Any(_attackController.CanAttack)) {
                var position = GetRightUnitPanelPosition(1, 2, BattleDirection.Defender);

                _unitPanelAnimations.Add(
                    _visualSceneController.AddAnimation(
                        currentUnit.HasAllyAbility()
                            ? _interfaceProvider.GetFieldHealBorder()
                            : _interfaceProvider.GetFieldAttackBorder(),
                        position.X,
                        position.Y,
                        INTERFACE_LAYER + 2));
            }
            // Иначе добавляем рамку только тем юнитам, которых можно атаковать
            else {
                foreach (var targetUnit in RightPanelUnits) {
                    if (_attackController.CanAttack(targetUnit) == false)
                        continue;

                    var position = GetRightUnitPanelPosition(targetUnit.Unit.SquadLinePosition,
                        targetUnit.Unit.SquadFlankPosition, targetUnit.Direction);

                    _unitPanelAnimations.Add(
                        _visualSceneController.AddAnimation(
                            currentUnit.HasAllyAbility()
                                ? _interfaceProvider.GetUnitHealBorder(targetUnit.Unit.UnitType.SizeSmall)
                                : _interfaceProvider.GetUnitAttackBorder(targetUnit.Unit.UnitType.SizeSmall),
                            position.X,
                            position.Y,
                            INTERFACE_LAYER + 2));
                }
            }


            foreach (var unitPanelAnimation in _unitPanelAnimations) {
                _game.CreateObject(unitPanelAnimation);
            }
        }

        /// <summary>
        /// Очистить все анимации на панели юнитов
        /// </summary>
        private void CleanAnimationsOnRightUnitsPanel()
        {
            foreach (var unitPanelAnimation in _unitPanelAnimations) {
                _game.DestroyObject(unitPanelAnimation);
            }

            _unitPanelAnimations = null;
        }



        /// <summary>
        /// Активировать указанные кнопки
        /// </summary>
        private static void ActivateButtons(params ButtonObject[] buttons)
        {
            if (buttons == null)
                return;

            foreach (var button in buttons) {
                button.Activate();
            }
        }

        /// <summary>
        /// Деактивировать указанные кнопки
        /// </summary>
        private static void DisableButtons(params ButtonObject[] buttons)
        {
            if (buttons == null)
                return;

            foreach (var button in buttons) {
                button.Disable();
            }
        }
    }
}
