using System;
using System.Collections.Generic;
using System.Linq;

using Disciples.Engine.Battle.Controllers;
using Disciples.Engine.Battle.Enums;
using Disciples.Engine.Battle.GameObjects;
using Disciples.Engine.Battle.Models;
using Disciples.Engine.Battle.Providers;
using Disciples.Engine.Common.Controllers;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.SceneObjects;
using Disciples.Engine.Extensions;
using Disciples.Engine.Models;

namespace Disciples.Engine.Implementation.Controllers
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

        private IReadOnlyCollection<BattleUnit> _rightPanelUnits;
        private IImageSceneObject _currentUnitFace;
        private BattleUnitInfoGameObject _currentUnitTextInfoObject;
        private IImageSceneObject _targetUnitFace;
        private BattleUnitInfoGameObject _targetUnitTextInfoObject;
        private BattleUnit _targetUnitObject;
        private DetailUnitInfoObject _detailUnitInfoObject;

        /// <summary>
        /// Позволяет заблокировать действия пользователя на время анимации.
        /// </summary>
        private bool _isAnimating;
        /// <summary>
        /// Отображается ли подробная информация о юните в данный момент.
        /// </summary>
        private bool _isUnitInfoShowing;
        /// <summary>
        /// Объект, над которым была зажата кнопка мыши.
        /// </summary>
        private GameObject _pressedObject;

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
        /// Отряд, который в данный момент отображается на правой панели.
        /// </summary>
        private BattleDirection _rightUnitsPanelSquadDirection;
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

            _game.GameObjectAction += OnGameObjectAction;

            _attackController.UnitActionBegin += OnUnitActionBegin;
            _attackController.UnitActionEnded += OnUnitActionEnded;
            _attackController.BattleEnded += OnBattleEnded;
        }


        /// <inheritdoc />
        public void Initialize()
        {
            InitializeMainInterface();
            InitializeButtons();

            AttachSelectedAnimation(_attackController.CurrentUnitObject);
            InitializeUnitsOnRightPanel();
            InitializeAnimationsOnRightUnitsPanel();
        }

        /// <summary>
        /// Разместить на сцене изображение поля боя и панелей, текст на панелях.
        /// </summary>
        private void InitializeMainInterface()
        {
            foreach (var battleground in _interfaceProvider.Battleground) {
                _visualSceneController.AddImage(battleground, 0, 0, 0);
            }
            _visualSceneController.AddImage(_interfaceProvider.BottomPanel, 0, GameInfo.OriginalHeight - _interfaceProvider.BottomPanel.Height, 1);
            _visualSceneController.AddImage(_interfaceProvider.RightPanel, GameInfo.OriginalWidth - _interfaceProvider.RightPanel.Width, 30, INTERFACE_LAYER);

            var currentUnitBattleFace = _attackController.CurrentUnitObject.Unit.UnitType.BattleFace;
            _currentUnitFace = _visualSceneController.AddImage(
                currentUnitBattleFace,
                0,
                GameInfo.OriginalHeight - currentUnitBattleFace.Height - 5,
                INTERFACE_LAYER + 1);

            _currentUnitTextInfoObject = _visualSceneController.AddBattleUnitInfo(190, 520, INTERFACE_LAYER + 1);
            _currentUnitTextInfoObject.Unit = _attackController.CurrentUnitObject.Unit;


            // Используем размеры лица текущего юнита, так как юнит-цель не инициализируются в начале боя.
            _targetUnitFace = _visualSceneController.AddImage(
                null,
                currentUnitBattleFace.Width,
                currentUnitBattleFace.Height,
                GameInfo.OriginalWidth - currentUnitBattleFace.Width,
                GameInfo.OriginalHeight - currentUnitBattleFace.Height - 5,
                INTERFACE_LAYER + 1);
            // todo Добавить перегрузку в метод?
            _targetUnitFace.IsReflected = true;

            _targetUnitTextInfoObject = _visualSceneController.AddBattleUnitInfo(510, 520, INTERFACE_LAYER + 1);
        }


        /// <summary>
        /// Разместить на сцене кнопки.
        /// </summary>
        private void InitializeButtons()
        {
            _reflectUnitPanelButton = _visualSceneController.AddToggleButton(_interfaceProvider.ToggleRightButton,
                ReflectRightUnitsPanel, 633, 402, INTERFACE_LAYER + 2, KeyboardButton.Tab);

            _defendButton = _visualSceneController.AddButton(_interfaceProvider.DefendButton, () => {
                    _attackController.Defend();
                }, 380, 504, INTERFACE_LAYER + 2, KeyboardButton.D);

            _retreatButton = _visualSceneController.AddButton(_interfaceProvider.RetreatButton, () => {
                //todo
            }, 343, 524, INTERFACE_LAYER + 2, KeyboardButton.R);

            _waitButton = _visualSceneController.AddButton(_interfaceProvider.WaitButton, () => {
                    _attackController.Wait();
                }, 419, 524, INTERFACE_LAYER + 2, KeyboardButton.W);

            _instantResolveButton = _visualSceneController.AddButton(_interfaceProvider.InstantResolveButton, () => {
                // todo
            }, 359, 563, INTERFACE_LAYER + 2, KeyboardButton.I);

            _autoBattleButton = _visualSceneController.AddToggleButton(_interfaceProvider.AutoBattleButton, () => {
                // todo
            }, 403, 563, INTERFACE_LAYER + 2, KeyboardButton.A);


            ActivateButtons(_reflectUnitPanelButton, _defendButton, _retreatButton, _waitButton, _instantResolveButton, _autoBattleButton);
        }


        /// <inheritdoc />
        public void UpdateTargetUnit(BattleUnit targetUnitObject, bool animateTarget = true)
        {
            if (targetUnitObject == null) {
                DetachTargetAnimations();
                _targetUnitObject = null;

                return;
            }

            _targetUnitObject = targetUnitObject;
            _targetUnitFace.Bitmap = targetUnitObject.Unit.UnitType.BattleFace;
            _targetUnitTextInfoObject.Unit = targetUnitObject.Unit;
            _animateTarget = animateTarget;

            var isInterfaceActive = _attackController.BattleState == BattleState.WaitingAction ||
                                    _attackController.BattleState == BattleState.BattleEnd;
            if (animateTarget && isInterfaceActive)
                SelectTargetUnits();
        }

        /// <inheritdoc />
        public void ShowDetailUnitInfo(Unit unit)
        {
            _detailUnitInfoObject?.Destroy();
            _detailUnitInfoObject = _visualSceneController.ShowDetailUnitInfo(unit);
        }

        /// <inheritdoc />
        public void StopShowDetailUnitInfo()
        {
            _detailUnitInfoObject?.Destroy();
            _detailUnitInfoObject = null;
        }


        #region Interaction

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

                UpdateTargetUnit(battleUnit);
            }
            else if (gameObject is UnitPortraitObject unitPortrait) {
                var targetUnitObject = _attackController.GetUnitObject(unitPortrait.Unit);
                UpdateTargetUnit(targetUnitObject, false);
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
                UpdateTargetUnit(null);
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

                _attackController.AttackUnit(targetUnitGameObject);
            }
            else if (gameObject is UnitPortraitObject unitPortrait) {
                if (_isAnimating)
                    return;

                var targetUnitObject = _attackController.GetUnitObject(unitPortrait.Unit);
                _attackController.AttackUnit(targetUnitObject);
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
            else if (gameObject is UnitPortraitObject unitPortrait) {
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
            ShowDetailUnitInfo(unit);
        }

        /// <summary>
        /// Обработать событие того, что на игровой объект нажали ПКМ.
        /// </summary>
        private void GameObjectRightButtonReleased(GameObject gameObject)
        {
            if (!_isUnitInfoShowing)
                return;

            _isUnitInfoShowing = false;
            StopShowDetailUnitInfo();

            // Если во время того, как отображалась информация о юните,
            // Курсор мыши был перемещён, то после того, как отпустили ПКМ, мы должны выделить нового юнита.
            if (_pressedObject != gameObject)
            {
                GameObjectUnselected(_pressedObject);
                GameObjectSelected(gameObject);
            }
        }

        #endregion


        private void OnUnitActionBegin(object sender, UnitActionBeginEventArgs args)
        {
            _isAnimating = true;

            DisableButtons(_reflectUnitPanelButton, _defendButton, _retreatButton, _waitButton);
            DetachSelectedAnimation();
            DetachTargetAnimations();

            // Если юнит атакует, то возвращаем состояние панели в нормальное значение.
            if (args.UnitActionType == UnitActionType.Attack) {
                CheckRightPanelReflection();
            }
            // Если юнит защищается/ждёт и т.д., то в любом случае нужно показывать его отряд.
            else {
                _isRightUnitPanelReflected = _attackController.CurrentUnitObject.IsAttacker;
            }

            // Обновляем портреты, если это требуется.
            InitializeUnitsOnRightPanel();

            // Во время анимации атаки рамки на панели с юнитами отображать не нужно.
            CleanAnimationsOnRightUnitsPanel();
        }

        private void OnUnitActionEnded(object sender, EventArgs eventArgs)
        {
            _isAnimating = false;

            ActivateButtons(_reflectUnitPanelButton);

            // Если юнит наносит второй удар, то указанные кнопки активировать не нужно.
            if (!_attackController.IsSecondAttack)
                ActivateButtons(_defendButton, _retreatButton, _waitButton);

            AttachSelectedAnimation(_attackController.CurrentUnitObject);

            CheckRightPanelReflection();
            InitializeUnitsOnRightPanel();
            InitializeAnimationsOnRightUnitsPanel();

            if (_targetUnitObject != null && _animateTarget)
                SelectTargetUnits();
        }

        private void OnBattleEnded(object sender, EventArgs eventArgs)
        {
            _isAnimating = false;

            _defendButton.Destroy();
            _retreatButton.Destroy();
            _waitButton.Destroy();
            _instantResolveButton.Destroy();
            _autoBattleButton.Destroy();

            // todo Создать две новые кнопки - "выйти" и "выйти и открыть" интерфейс.

            // Отображаем отряд победителя.
            _isRightUnitPanelReflected = _attackController.CurrentUnitObject.IsAttacker;
            _reflectUnitPanelButton.Activate();
            _reflectUnitPanelButton.SetState(_isRightUnitPanelReflected);

            InitializeUnitsOnRightPanel();
            CleanAnimationsOnRightUnitsPanel();
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

            if (_currentUnitTextInfoObject != null)
                _currentUnitTextInfoObject.Unit = battleUnit.Unit;

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
        /// Установить какой отряд должен отображаться на правой панели.
        /// </summary>
        private void CheckRightPanelReflection()
        {
            var currentUnit = _attackController.CurrentUnitObject.Unit;
            var showEnemies = currentUnit.HasEnemyAbility();

            // Если текущий юнит находится в атакующем отряде, то мы отражаем панель только, если он лекарь и т.д.
            if (_attackController.CurrentUnitObject.IsAttacker) {
                _isRightUnitPanelReflected = !showEnemies;
            }
            // Иначе для защищающегося отряда для отображения врагов нужно отражать панель.
            else {
                _isRightUnitPanelReflected = showEnemies;
            }

            _reflectUnitPanelButton.SetState(_isRightUnitPanelReflected);
        }

        /// <summary>
        /// Поменять отряд, который отображается на правой панели с юнитам.
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
            var direction = _isRightUnitPanelReflected
                ? BattleDirection.Attacker
                : BattleDirection.Defender;

            // Если юниты уже расположены на панели и отряд, который необходимо отображать не изменился,
            // То нет необходимости что-либо менять.
            if (_rightPanelUnitPortraits != null && _rightUnitsPanelSquadDirection == direction)
                return;

            _rightUnitsPanelSquadDirection = direction;

            // Удаляем старые портреты.
            if (_rightPanelUnitPortraits != null) {
                foreach (var portrait in _rightPanelUnitPortraits) {
                    portrait.Destroy();
                }
            }

            _rightPanelUnits = _attackController
                .Units
                .Where(u => u.IsAttacker && direction == BattleDirection.Attacker ||
                            !u.IsAttacker && direction == BattleDirection.Defender)
                .ToList();

            var portraits = new List<UnitPortraitObject>();
            foreach (var battleUnit in _rightPanelUnits) {
                var lineOffset = direction == BattleDirection.Defender
                    ? (battleUnit.Unit.SquadLinePosition + 1) % 2
                    : battleUnit.Unit.SquadLinePosition;

                var portrait = _visualSceneController.AddUnitPortrait(battleUnit.Unit,
                    battleUnit.Direction == BattleDirection.Defender,
                    battleUnit.Unit.UnitType.SizeSmall
                        ? 644 + 79 * lineOffset
                        : 644, // todo проверить большого юнита, который атакует.
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
            if (currentUnit.Player == _rightPanelUnits.First().Unit.Player) {
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
                _rightPanelUnits.Any(_attackController.CanAttack)) {
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
                foreach (var targetUnit in _rightPanelUnits) {
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