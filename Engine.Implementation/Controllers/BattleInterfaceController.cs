using System;
using System.Collections.Generic;
using System.Linq;

using Avalonia.Media;
using Avalonia.Media.Imaging;

using Engine.Battle.Contollers;
using Engine.Battle.Enums;
using Engine.Battle.GameObjects;
using Engine.Battle.Models;
using Engine.Battle.Providers;
using Engine.Common.Controllers;
using Engine.Common.Enums.Units;
using Engine.Common.GameObjects;
using Engine.Common.Models;
using Engine.Extensions;

namespace Engine.Implementation.Controllers
{
    /// <inheritdoc cref="IBattleInterfaceController" />
    public class BattleInterfaceController : IBattleInterfaceController
    {
        /// <summary>
        /// Слой для расположения интерфейса.
        /// </summary>
        private const int INTERFACE_LAYER = 1000;

        private readonly IGame _game;
        private readonly IVisualSceneController _visualSceneController;
        private readonly IBattleAttackController _attackController;
        private readonly IBattleInterfaceProvider _interfaceProvider;
        private readonly IBattleResourceProvider _battleResourceProvider;

        private ImageVisualObject _currentUnitFace;
        private UnitInfoTextVisualObject _currentUnitText;
        private ImageVisualObject _targetUnitFace;
        private UnitInfoTextVisualObject _targetUnitText;
        private BattleUnit _targetUnitObject;

        /// <summary>
        /// Игровой объект, отрисовывающий на текущем юните анимацию выделения.
        /// </summary>
        private AnimationObject _selectionAnimation;
        /// <summary>
        /// Игровые объекты, которые отрисовывают анимации цели на юнитах-целях.
        /// </summary>
        private IList<AnimationObject> _targetAnimations;
        /// <summary>
        /// Необходимо ли отрисовывать анимации цели на юнитах.
        /// </summary>
        private bool _animateTarget;

        /// <summary>
        /// Объекты портретов на правой панели.
        /// </summary>
        private IReadOnlyList<UnitPortraitObject> _rightPanelUnitPortraits;
        /// <summary>
        /// Изменено ли отображение юнитов на панели.
        /// </summary>
        private bool _isRightUnitPanelReflected;
        /// <summary>
        /// Анимации-рамки, которые отрисовываются на панели с юнитами.
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


        /// <inheritdoc />
        public Bitmap Battleground => _interfaceProvider.Battleground;

        /// <inheritdoc />
        public Bitmap PanelSeparator => _interfaceProvider.PanelSeparator;

        /// <inheritdoc />
        public Bitmap DeathSkull => _interfaceProvider.DeathSkull;

        /// <inheritdoc />
        public IReadOnlyCollection<BattleUnit> RightPanelUnits { get; private set; }


        /// <inheritdoc />
        public void Initialize()
        {
            InitializePanels();
            InitializeButtons();

            AttachSelectedAnimation(_attackController.CurrentUnitObject);
            InitializeUnitsOnRightPanel();
            InitializeAnimationsOnRightUnitsPanel();
        }

        /// <summary>
        /// Разместить на сцене картинки для панелей и текст.
        /// </summary>
        private void InitializePanels()
        {
            // Нижнюю панель располагаем 1 слое, чтобы прозрачная часть не перекрывала нижнего юнита.
            _visualSceneController.AddImageVisual(_interfaceProvider.BottomPanel, 0, GameInfo.OriginalHeight - _interfaceProvider.BottomPanel.PixelSize.Height, 1);
            _visualSceneController.AddImageVisual(_interfaceProvider.RightPanel, GameInfo.OriginalWidth - _interfaceProvider.RightPanel.PixelSize.Width, 30, INTERFACE_LAYER);

            var currentUnitBattleFace = _attackController.CurrentUnitObject.Unit.UnitType.BattleFace;
            _currentUnitFace = _visualSceneController.AddImageVisual(
                currentUnitBattleFace,
                0,
                GameInfo.OriginalHeight - currentUnitBattleFace.PixelSize.Height - 5,
                INTERFACE_LAYER + 1);

            _currentUnitText = _visualSceneController.AddUnitInfoTextVisualObject(GetUnitNameAndHitPoints, 14, 190, 520, INTERFACE_LAYER + 1, true);
            _currentUnitText.Unit = _attackController.CurrentUnitObject.Unit;
            _currentUnitText.Width = 120;
            _currentUnitText.Height = 40;


            // Используем размеры лица текущего юнита, так как юнит-цель не инициализируются в начале боя.
            _targetUnitFace = _visualSceneController.AddImageVisual(
                null,
                currentUnitBattleFace.PixelSize.Width,
                currentUnitBattleFace.PixelSize.Height,
                GameInfo.OriginalWidth - currentUnitBattleFace.PixelSize.Width,
                GameInfo.OriginalHeight - currentUnitBattleFace.PixelSize.Height - 5,
                INTERFACE_LAYER + 1);
            // todo Добавить перегрузку в метод?
            _targetUnitFace.Transform = new ScaleTransform(-1, 1);

            _targetUnitText = _visualSceneController.AddUnitInfoTextVisualObject(GetUnitNameAndHitPoints, 14, 510, 520, INTERFACE_LAYER + 1, true);
            _targetUnitText.Width = 120;
            _targetUnitText.Height = 40;
        }

        private static string GetUnitNameAndHitPoints(Unit unit)
        {
            return $"{unit.UnitType.Name}{Environment.NewLine}" +
                   $"ОЗ : {unit.HitPoints}/{unit.UnitType.HitPoints}";
        }


        /// <summary>
        /// Разместить на сцене кнопки.
        /// </summary>
        private void InitializeButtons()
        {
            _reflectUnitPanelButton = _visualSceneController.AddToggleButton(_interfaceProvider.ToggleRightButton,
                ReflectRightUnitsPanel, 633, 402, INTERFACE_LAYER + 2);

            _defendButton = _visualSceneController.AddButton(_interfaceProvider.DefendButton, () => {
                    _attackController.Defend();
                }, 380, 504, INTERFACE_LAYER + 2);

            _retreatButton = _visualSceneController.AddButton(_interfaceProvider.RetreatButton, () => {
                //todo
            }, 343, 524, INTERFACE_LAYER + 2);

            _waitButton = _visualSceneController.AddButton(_interfaceProvider.WaitButton, () => {
                    _attackController.Wait();
                }, 419, 524, INTERFACE_LAYER + 2);

            _instantResolveButton = _visualSceneController.AddButton(_interfaceProvider.InstantResolveButton, () => {
                // todo
            }, 359, 563, INTERFACE_LAYER + 2);

            _autoBattleButton = _visualSceneController.AddToggleButton(_interfaceProvider.AutoBattleButton, () => {
                // todo
            }, 403, 563, INTERFACE_LAYER + 2);


            ActivateButtons(_reflectUnitPanelButton, _defendButton, _retreatButton, _waitButton, _instantResolveButton, _autoBattleButton);
        }


        public void UpdateTargetUnit(BattleUnit targetUnitObject, bool animateTarget = true)
        {
            if (targetUnitObject == null) {
                DetachTargetAnimations();
                _targetUnitObject = null;

                return;
            }

            _targetUnitObject = targetUnitObject;
            _targetUnitFace.Bitmap = targetUnitObject.Unit.UnitType.BattleFace;
            _targetUnitText.Unit = targetUnitObject.Unit;
            _animateTarget = animateTarget;

            var isInterfaceActive = _attackController.BattleState == BattleState.WaitingAction ||
                                    _attackController.BattleState == BattleState.BattleEnd;
            if (animateTarget && isInterfaceActive)
                SelectTargetUnits();
        }


        private void OnUnitActionBegin(object sender, UnitActionBeginEventArgs args)
        {
            DisableButtons(_reflectUnitPanelButton, _defendButton, _retreatButton, _waitButton);
            DetachSelectedAnimation();
            DetachTargetAnimations();

            // Перед началом действия определяемся с тем, какой отряд будет отображаться на панели.
            // Если это юнит атакует врагов, то нужно показать вражеский отряд,
            // Иначе, если это целитель или юнит защищается/ждёт, то показываем его отряд.
            var currentUnit = _attackController.CurrentUnitObject.Unit;
            var showEnemies = currentUnit.HasEnemyAbility();
            var needReflectPanel = showEnemies && args.UnitActionType != UnitActionType.Attack;
            if (needReflectPanel != _isRightUnitPanelReflected) {
                _isRightUnitPanelReflected = !_isRightUnitPanelReflected;
                InitializeUnitsOnRightPanel();
            }

            // Во время анимации атаки рамки на панели с юнитами отображать не нужно.
            CleanAnimationsOnRightUnitsPanel();
        }

        private void OnUnitActionEnded(object sender, EventArgs eventArgs)
        {
            ActivateButtons(_reflectUnitPanelButton, _defendButton, _retreatButton, _waitButton);
            AttachSelectedAnimation(_attackController.CurrentUnitObject);

            _isRightUnitPanelReflected = false;
            InitializeUnitsOnRightPanel();
            InitializeAnimationsOnRightUnitsPanel();

            if (_targetUnitObject != null && _animateTarget)
                SelectTargetUnits();
        }

        private void OnBattleEnded(object sender, EventArgs eventArgs)
        {
            _defendButton.Destroy();
            _retreatButton.Destroy();
            _waitButton.Destroy();
            _instantResolveButton.Destroy();
            _autoBattleButton.Destroy();

            // todo Создать две новые кнопки - "выйти" и "выйти и открыть" интерфейс.


            // todo на панели необходимо выводить отряд победителя.
            InitializeUnitsOnRightPanel();
            if (_targetUnitObject != null)
                SelectTargetUnits();
        }


        /// <summary>
        /// Отобразить анимацию выделения на текущем юните.
        /// </summary>
        private void AttachSelectedAnimation(BattleUnit battleUnit)
        {
            if (_currentUnitFace != null)
                _currentUnitFace.Bitmap = battleUnit.Unit.UnitType.BattleFace;

            if (_currentUnitText != null)
                _currentUnitText.Unit = battleUnit.Unit;

            if (_selectionAnimation != null)
                DetachSelectedAnimation();

            var frames = _battleResourceProvider.GetBattleAnimation(
                battleUnit.Unit.UnitType.SizeSmall
                    ? "MRKCURSMALLA"
                    : "MRKCURLARGEA");


            // Задаём смещение 190. Возможно, стоит вычислять высоту юнита или что-то в этом роде.
            _selectionAnimation = _visualSceneController.AddAnimation(frames, battleUnit.X, battleUnit.Y + 190, 1);
        }

        /// <summary>
        /// Скрыть анимацию выделения на текущем юните.
        /// </summary>
        private void DetachSelectedAnimation()
        {
            _game.DestroyObject(_selectionAnimation);
            _selectionAnimation = null;
        }


        /// <summary>
        /// Отобразить анимацию выделения цели исходя из типа атаки текущего юнита.
        /// </summary>
        private void SelectTargetUnits()
        {
            if (_targetUnitObject == null)
                return;

            var currentUnit = _attackController.CurrentUnitObject.Unit;
            var targetUnit = _targetUnitObject.Unit;

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
            // Также наоборот, если юнит применяет способность на врагов, то выделятся все враги
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
        /// Отобразить анимацию выделения цели на указанных юнитах.
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
        /// Скрыть анимации выделения цели на всех юнитах.
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
        /// Поменять отряд, который отображает на правой панели с юнитам.
        /// </summary>
        private void ReflectRightUnitsPanel()
        {
            _isRightUnitPanelReflected = !_isRightUnitPanelReflected;
            InitializeUnitsOnRightPanel();
            InitializeAnimationsOnRightUnitsPanel();
        }

        /// <summary>
        /// Определить юнитов, которые будут отображаться на панели.
        /// </summary>
        private void InitializeUnitsOnRightPanel()
        {
            // Удаляем старые портреты.
            if (_rightPanelUnitPortraits != null) {
                foreach (var portrait in _rightPanelUnitPortraits) {
                    portrait.Destroy();
                }
            }

            var currentUnit = _attackController.CurrentUnitObject.Unit;
            var showEnemies = currentUnit.HasEnemyAbility();
            if (_isRightUnitPanelReflected)
                showEnemies = !showEnemies;

            RightPanelUnits = _attackController.Units
                .Where(u => showEnemies && u.Unit.Player != currentUnit.Player ||
                            showEnemies == false && u.Unit.Player == currentUnit.Player)
                .ToList();

            var portraits = new List<UnitPortraitObject>();
            foreach (var battleUnit in RightPanelUnits) {
                var lineOffset = battleUnit.Direction == BattleDirection.Defender
                    ? (battleUnit.Unit.SquadLinePosition + 1) % 2
                    : battleUnit.Unit.SquadLinePosition;

                var portrait = _visualSceneController.AddUnitPortrait(battleUnit.Unit,
                    battleUnit.Direction == BattleDirection.Defender,
                    battleUnit.Unit.UnitType.SizeSmall
                        ? 644 + 79 * lineOffset
                        : 644, // todo проверить большого юнита, который атакует
                    85 + 106 * (2 - battleUnit.Unit.SquadFlankPosition));
                portraits.Add(portrait);
            }

            _rightPanelUnitPortraits = portraits;
        }

        /// <summary>
        /// Получить координаты на сцене для портрета юнита.
        /// </summary>
        private static (double X, double Y) GetRightUnitPanelPosition(int linePosition, int flankPosition, BattleDirection unitDirection)
        {
            // Защищающиеся на правой панели располагаются ли справа налево, а атакующие слева направо.
            var lineOffset = unitDirection == BattleDirection.Defender
                ? linePosition
                : (linePosition + 1) % 2;

            return (
                642 + (1 - lineOffset) * 79,
                83 + (2 - flankPosition) * 106
            );
        }

        /// <summary>
        /// Разместить анимации, которые показывают какие юниты доступны для атаки, на правой панели.
        /// </summary>
        private void InitializeAnimationsOnRightUnitsPanel()
        {
            CleanAnimationsOnRightUnitsPanel();

            var currentUnit = _attackController.CurrentUnitObject.Unit;
            _unitPanelAnimations = new List<AnimationObject>();

            // Если отображается отряд текущего юнита, то нужно его выделить на панели.
            if (currentUnit.Player == RightPanelUnits.First().Unit.Player) {
                var position = GetRightUnitPanelPosition(currentUnit.SquadLinePosition, currentUnit.SquadFlankPosition, _attackController.CurrentUnitObject.Direction);

                _unitPanelAnimations.Add(
                    _visualSceneController.AddAnimation(
                        _interfaceProvider.GetUnitSelectionBorder(currentUnit.UnitType.SizeSmall),
                        position.X,
                        position.Y,
                        INTERFACE_LAYER + 3));
            }

            // Если юнит бьёт по площади и цель юнита - отображаемый отряд, то добавляем одну большую рамку.
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
                        INTERFACE_LAYER + 3));
            }
            // Иначе добавляем рамку только тем юнитам, которых можно атаковать.
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
                            INTERFACE_LAYER + 3));
                }
            }
        }

        /// <summary>
        /// Очистить все анимации на панели юнитов.
        /// </summary>
        private void CleanAnimationsOnRightUnitsPanel()
        {
            if (_unitPanelAnimations == null)
                return;

            foreach (var unitPanelAnimation in _unitPanelAnimations) {
                _game.DestroyObject(unitPanelAnimation);
            }

            _unitPanelAnimations = null;
        }



        /// <summary>
        /// Активировать указанные кнопки.
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
        /// Деактивировать указанные кнопки.
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
