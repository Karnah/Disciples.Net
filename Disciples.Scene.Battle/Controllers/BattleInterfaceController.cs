using Disciples.Engine;
using Disciples.Engine.Base;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Common.SceneObjects;
using Disciples.Engine.Enums;
using Disciples.Engine.Extensions;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Models;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.GameObjects;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Models.BattleActions;
using Disciples.Scene.Battle.Providers;

namespace Disciples.Scene.Battle.Controllers
{
    /// <inheritdoc cref="IBattleInterfaceController" />
    public class BattleInterfaceController : BaseSupportLoading, IBattleInterfaceController
    {
        /// <summary>
        /// Слой для расположения интерфейса.
        /// </summary>
        private const int INTERFACE_LAYER = 1000;

        private readonly IGameController _gameController;
        private readonly IBattleSceneController _battleSceneController;
        private readonly IBattleInterfaceProvider _interfaceProvider;
        private readonly IBattleResourceProvider _battleResourceProvider;
        private readonly BattleContext _context;
        private readonly BattleProcessor _battleProcessor;

        private IReadOnlyCollection<BattleUnit> _rightPanelUnits;
        private IImageSceneObject? _currentUnitFace;
        private BattleUnitInfoGameObject? _currentUnitTextInfoObject;
        private IImageSceneObject? _targetUnitFace;
        private BattleUnitInfoGameObject? _targetUnitTextInfoObject;
        private BattleUnit? _targetUnitObject;
        private DetailUnitInfoObject? _detailUnitInfoObject;

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
        private GameObject? _pressedObject;

        /// <summary>
        /// Игровой объект, отрисовывающий на текущем юните анимацию выделения.
        /// </summary>
        private AnimationObject? _currentUnitSelectionAnimation;
        /// <summary>
        /// Игровые объекты, которые отрисовывают анимации цели на юнитах-целях.
        /// </summary>
        private IList<AnimationObject>? _targetUnitsAnimations;
        /// <summary>
        /// Необходимо ли отрисовывать анимации цели на юнитах.
        /// </summary>
        private bool _shouldAnimateTargets;

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

        /// <summary>
        /// Создать объект типа <see cref="BattleInterfaceController" />.
        /// </summary>
        public BattleInterfaceController(
            IGameController gameController,
            IBattleSceneController battleSceneController,
            IBattleInterfaceProvider battleInterfaceProvider,
            IBattleResourceProvider battleResourceProvider,
            BattleContext context,
            BattleProcessor battleProcessor)
        {
            _gameController = gameController;
            _battleSceneController = battleSceneController;
            _interfaceProvider = battleInterfaceProvider;
            _battleResourceProvider = battleResourceProvider;
            _context = context;
            _battleProcessor = battleProcessor;
        }

        /// <inheritdoc />
        public override bool IsSharedBetweenScenes => false;

        /// <summary>
        ///  Юнит, который выполняет свой ход.
        /// </summary>
        private BattleUnit CurrentBattleUnit => _context.CurrentBattleUnit;

        /// <summary>
        /// Признак того, что юнит атакует второй раз за текущий ход.
        /// </summary>
        /// <remarks>Актуально только для юнитов, которые бьют дважды за ход.</remarks>
        private bool IsSecondAttack => _context.IsSecondAttack;

        /// <summary>
        /// Список юнитов.
        /// </summary>
        private IReadOnlyList<BattleUnit> BattleUnits => _context.BattleUnits;

        /// <summary>
        /// Список всех действий на поле боя.
        /// </summary>
        private BattleActionContainer Actions => _context.Actions;

        /// <inheritdoc />
        public void BeforeSceneUpdate()
        {
            foreach (var inputDeviceEvent in _context.InputDeviceEvents)
            {
                ProcessInputDeviceEvent(inputDeviceEvent);
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// Порядок обработки важен.
        /// </remarks>
        public void AfterSceneUpdate()
        {
            if (Actions.IsActionsBeginThisUpdate)
            {
                ProcessActionsBegin();
            }

            foreach (var beginAction in Actions.New)
            {
                ProcessBeginAction(beginAction);
            }

            foreach (var beginAction in Actions.Completed)
            {
                ProcessCompletedAction(beginAction);
            }

            if (Actions.IsAllActionsCompletedThisUpdate)
            {
                ProcessActionsCompleted();
            }
        }

        /// <inheritdoc />
        protected override void LoadInternal()
        {
            InitializeMainInterface();
            InitializeButtons();

            AttachSelectedAnimation(CurrentBattleUnit);
            InitializeUnitsOnRightPanel();
            InitializeAnimationsOnRightUnitsPanel();
        }

        /// <inheritdoc />
        protected override void UnloadInternal()
        {
            // todo Зачистить все элементы интерфейса.
        }

        /// <summary>
        /// Разместить на сцене изображение поля боя и панелей, текст на панелях.
        /// </summary>
        private void InitializeMainInterface()
        {
            foreach (var battleground in _interfaceProvider.Battleground) {
                _battleSceneController.AddImage(battleground, 0, 0, 0);
            }
            _battleSceneController.AddImage(_interfaceProvider.BottomPanel, 0, GameInfo.OriginalHeight - _interfaceProvider.BottomPanel.Height, 1);
            _battleSceneController.AddImage(_interfaceProvider.RightPanel, GameInfo.OriginalWidth - _interfaceProvider.RightPanel.Width, 30, INTERFACE_LAYER);

            var currentUnitBattleFace = CurrentBattleUnit.Unit.UnitType.BattleFace;
            _currentUnitFace = _battleSceneController.AddImage(
                currentUnitBattleFace,
                0,
                GameInfo.OriginalHeight - currentUnitBattleFace.Height - 5,
                INTERFACE_LAYER + 1);

            _currentUnitTextInfoObject = _battleSceneController.AddBattleUnitInfo(190, 520, INTERFACE_LAYER + 1);
            _currentUnitTextInfoObject.Unit = CurrentBattleUnit.Unit;


            // Используем размеры лица текущего юнита, так как юнит-цель не инициализируются в начале боя.
            _targetUnitFace = _battleSceneController.AddImage(
                null,
                currentUnitBattleFace.Width,
                currentUnitBattleFace.Height,
                GameInfo.OriginalWidth - currentUnitBattleFace.Width,
                GameInfo.OriginalHeight - currentUnitBattleFace.Height - 5,
                INTERFACE_LAYER + 1);
            // todo Добавить перегрузку в метод?
            _targetUnitFace.IsReflected = true;

            _targetUnitTextInfoObject = _battleSceneController.AddBattleUnitInfo(510, 520, INTERFACE_LAYER + 1);
        }


        /// <summary>
        /// Разместить на сцене кнопки.
        /// </summary>
        private void InitializeButtons()
        {
            _reflectUnitPanelButton = _battleSceneController.AddToggleButton(_interfaceProvider.ToggleRightButton,
                ReflectRightUnitsPanel, 633, 402, INTERFACE_LAYER + 2, KeyboardButton.Tab);

            _defendButton = _battleSceneController.AddButton(_interfaceProvider.DefendButton, () => {
                //todo вообще убрать прямую обработку кнопок.
                }, 380, 504, INTERFACE_LAYER + 2, KeyboardButton.D);

            _retreatButton = _battleSceneController.AddButton(_interfaceProvider.RetreatButton, () => {
                //todo
            }, 343, 524, INTERFACE_LAYER + 2, KeyboardButton.R);

            _waitButton = _battleSceneController.AddButton(_interfaceProvider.WaitButton, () => {
                    // 
                }, 419, 524, INTERFACE_LAYER + 2, KeyboardButton.W);

            _instantResolveButton = _battleSceneController.AddButton(_interfaceProvider.InstantResolveButton, () => {
                // todo
            }, 359, 563, INTERFACE_LAYER + 2, KeyboardButton.I);

            _autoBattleButton = _battleSceneController.AddToggleButton(_interfaceProvider.AutoBattleButton, () => {
                // todo
            }, 403, 563, INTERFACE_LAYER + 2, KeyboardButton.A);


            ActivateButtons(_reflectUnitPanelButton, _defendButton, _retreatButton, _waitButton, _instantResolveButton, _autoBattleButton);
        }


        /// <summary>
        /// Обновить цель.
        /// </summary>
        /// <param name="targetUnitObject">Юнит, на которого навели курсором.</param>
        /// <param name="shouldAnimateTarget">Необходимо ли выделить юнита с помощью анимации (красный крутящийся круг).</param>
        private void UpdateTargetUnit(BattleUnit? targetUnitObject, bool shouldAnimateTarget = true)
        {
            if (targetUnitObject == null)
            {
                DetachTargetAnimations();
                _targetUnitObject = null;

                return;
            }

            _targetUnitObject = targetUnitObject;
            _targetUnitFace!.Bitmap = targetUnitObject.Unit.UnitType.BattleFace;
            _targetUnitTextInfoObject!.Unit = targetUnitObject.Unit;
            _shouldAnimateTargets = shouldAnimateTarget;

            if (shouldAnimateTarget && !_isAnimating)
                SelectTargetUnits();
        }

        /// <summary>
        /// Отобразить детальную информацию по указанному юниту.
        /// </summary>
        /// <param name="unit">Юнит, информацию о котором необходимо отобразить.</param>
        private void ShowDetailUnitInfo(Unit unit)
        {
            _detailUnitInfoObject?.Destroy();
            _detailUnitInfoObject = _battleSceneController.ShowDetailUnitInfo(unit);
        }

        /// <summary>
        /// Прекратить отображение детальной информации по юниту.
        /// </summary>
        private void StopShowDetailUnitInfo()
        {
            _detailUnitInfoObject?.Destroy();
            _detailUnitInfoObject = null;
        }


        #region Interaction

        /// <summary>
        /// Обработать событие воздействия с игровым объектом (наведение, клик мышью и т.д.).
        /// </summary>
        private void ProcessInputDeviceEvent(InputDeviceEvent inputDeviceEvent)
        {
            var actionType = inputDeviceEvent.ActionType;
            var actionState = inputDeviceEvent.ActionState;
            var gameObject = inputDeviceEvent.GameObject;

            // Если отпустили ПКМ, то прекращаем отображать информацию о юните.
            if (actionType == InputDeviceActionType.MouseRight && actionState == InputDeviceActionState.Deactivated) {
                GameObjectRightButtonReleased(gameObject);
                return;
            }

            // Если ПКМ зажата, то не меняем выбранного юнита до тех пор, пока не будет отпущена кнопка.
            if (_isUnitInfoShowing) {
                return;
            }

            switch (actionType) {
                case InputDeviceActionType.Selection when actionState == InputDeviceActionState.Activated:
                    GameObjectSelected(gameObject);
                    break;
                case InputDeviceActionType.Selection when actionState == InputDeviceActionState.Deactivated:
                    GameObjectUnselected(gameObject);
                    break;

                case InputDeviceActionType.MouseLeft when actionState == InputDeviceActionState.Activated:
                    GameObjectPressed(gameObject);
                    break;
                case InputDeviceActionType.MouseLeft when actionState == InputDeviceActionState.Deactivated:
                    GameObjectClicked(gameObject);
                    break;

                case InputDeviceActionType.MouseRight when actionState == InputDeviceActionState.Activated:
                    GameObjectRightButtonPressed(gameObject);
                    break;

                case InputDeviceActionType.UiButton:
                    GameObjectPressed(gameObject);
                    GameObjectClicked(gameObject);
                    break;
            }
        }

        /// <summary>
        /// Обработчик события, что игровой объект был выбран.
        /// </summary>
        private void GameObjectSelected(GameObject? gameObject)
        {
            if (gameObject is BattleUnit battleUnit)
            {
                // Если выбрали кости юнита, то не нужно менять портрет.
                if (battleUnit.Unit.IsDead)
                    return;

                UpdateTargetUnit(battleUnit);
            }
            else if (gameObject is UnitPortraitObject unitPortrait)
            {
                var targetBattleUnit = _context.GetBattleUnit(unitPortrait.Unit);
                UpdateTargetUnit(targetBattleUnit, false);
            }
            else if (gameObject is ButtonObject button)
            {
                button.OnSelected();
            }
        }

        /// <summary>
        /// Обработчик события, что с игрового объекта был смещён фокус.
        /// </summary>
        private void GameObjectUnselected(GameObject? gameObject)
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
        private void GameObjectPressed(GameObject? gameObject)
        {
            if (gameObject == null)
                return;

            _pressedObject = gameObject;

            if (gameObject is ButtonObject button)
            {
                button.OnPressed();
            }
        }

        /// <summary>
        /// Обработчик события клика на игровом объекта.
        /// </summary>
        private void GameObjectClicked(GameObject? gameObject)
        {
            // В том случае, если нажали кнопку на одном объекте, а отпустили на другом, то ничего не делаем.
            if (_pressedObject != gameObject)
                return;

            if (gameObject is BattleUnit targetUnitGameObject)
            {
                if (_isAnimating)
                    return;

                if (CanAttack(targetUnitGameObject))
                    Actions.Add(new BeginAttackUnitBattleAction(targetUnitGameObject));
            }
            else if (gameObject is UnitPortraitObject unitPortrait)
            {
                if (_isAnimating)
                    return;

                var targetUnitObject = _context.GetBattleUnit(unitPortrait.Unit);
                if (CanAttack(targetUnitObject))
                    Actions.Add(new BeginAttackUnitBattleAction(targetUnitObject));
            }
            else if (gameObject is ButtonObject button)
            {
                // TODO Подумать над другим способом обработки.
                //button.OnReleased();

                if (button == _defendButton)
                {
                    Actions.Add(new DefendBattleAction());
                }
                else if (button == _waitButton)
                {
                    Actions.Add(new WaitingBattleAction());
                }
            }
        }

        /// <summary>
        /// Обработать событие того, что на игровой объект нажали ПКМ.
        /// </summary>
        private void GameObjectRightButtonPressed(GameObject? gameObject)
        {
            Unit? unit = null;

            if (gameObject is BattleUnit battleUnit)
            {
                unit = battleUnit.Unit;
            }
            else if (gameObject is UnitPortraitObject unitPortrait)
            {
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
        private void GameObjectRightButtonReleased(GameObject? gameObject)
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


        /// <summary>
        /// Обработать начало действий на сцене.
        /// </summary>
        private void ProcessActionsBegin()
        {
            _isAnimating = true;

            DisableButtons(_reflectUnitPanelButton, _defendButton, _retreatButton, _waitButton);
            DetachSelectedAnimation();
            DetachTargetAnimations();

            // Если юнит атакует, то возвращаем состояние панели в нормальное значение.
            if (Actions.New.OfType<BeginAttackUnitBattleAction>().Any())
            {
                CheckRightPanelReflection();
            }
            // Если юнит защищается/ждёт и т.д., то в любом случае нужно показывать его отряд.
            else
            {
                _isRightUnitPanelReflected = CurrentBattleUnit.IsAttacker;
            }

            // Обновляем портреты, если это требуется.
            InitializeUnitsOnRightPanel();

            // Во время анимации атаки рамки на панели с юнитами отображать не нужно.
            CleanAnimationsOnRightUnitsPanel();
        }

        /// <summary>
        /// Обработать завершение всех действий на сцене.
        /// </summary>
        private void ProcessActionsCompleted()
        {
            _isAnimating = false;

            ActivateButtons(_reflectUnitPanelButton);

            // Если юнит наносит второй удар, то указанные кнопки активировать не нужно.
            if (!IsSecondAttack)
                ActivateButtons(_defendButton, _retreatButton, _waitButton);

            AttachSelectedAnimation(CurrentBattleUnit);

            CheckRightPanelReflection();
            InitializeUnitsOnRightPanel();
            InitializeAnimationsOnRightUnitsPanel();

            if (_targetUnitObject != null && _shouldAnimateTargets)
                SelectTargetUnits();
        }

        /// <summary>
        /// Обработать начало нового действия.
        /// </summary>
        private void ProcessBeginAction(IBattleAction action)
        {
            if (action is UnitBattleAction unitBattleAction)
            {
                var portrait = _rightPanelUnitPortraits.FirstOrDefault(up => up.Unit == unitBattleAction.TargetUnit.Unit);
                if (portrait == null)
                    return;

                portrait.ProcessBeginUnitAction(unitBattleAction);
            }

            if (action is BattleCompletedAction)
            {
                ProcessBattleCompleted();
            }
        }

        /// <summary>
        /// Обработать завершение действия.
        /// </summary>
        private void ProcessCompletedAction(IBattleAction action)
        {
            if (action is UnitBattleAction unitBattleAction)
            {
                var portrait = _rightPanelUnitPortraits.FirstOrDefault(up => up.Unit == unitBattleAction.TargetUnit.Unit);
                if (portrait == null)
                    return;

                portrait.ProcessCompletedUnitAction(unitBattleAction);
            }
        }

        /// <summary>
        /// Обработать завершение битвы.
        /// </summary>
        private void ProcessBattleCompleted()
        {
            // BUG. Не работает информация о юните после завершения битвы.

            _isAnimating = false;

            _defendButton.Destroy();
            _retreatButton.Destroy();
            _waitButton.Destroy();
            _instantResolveButton.Destroy();
            _autoBattleButton.Destroy();

            // todo Создать две новые кнопки - "выйти" и "выйти и открыть" интерфейс.

            // Отображаем отряд победителя.
            _isRightUnitPanelReflected = CurrentBattleUnit.IsAttacker;
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

            DetachSelectedAnimation();

            var frames = _battleResourceProvider.GetBattleAnimation(
                battleUnit.Unit.UnitType.SizeSmall
                    ? "MRKCURSMALLA"
                    : "MRKCURLARGEA");


            // Задаём смещение 190. Возможно, стоит вычислять высоту юнита или что-то в этом роде.
            _currentUnitSelectionAnimation = _battleSceneController.AddAnimation(frames, battleUnit.X, battleUnit.Y + 190, 1);
        }

        /// <summary>
        /// Скрыть анимацию выделения на текущем юните.
        /// </summary>
        private void DetachSelectedAnimation()
        {
            if (_currentUnitSelectionAnimation == null)
                return;

            _gameController.DestroyObject(_currentUnitSelectionAnimation);
            _currentUnitSelectionAnimation = null;
        }


        /// <summary>
        /// Отобразить анимацию выделения цели исходя из типа атаки текущего юнита.
        /// </summary>
        private void SelectTargetUnits()
        {
            if (_targetUnitObject == null)
                return;

            var currentUnit = CurrentBattleUnit.Unit;
            var targetUnit = _targetUnitObject.Unit;

            if (targetUnit.IsDead)
                return;

            // Если текущий юнит может атаковать только одну цель,
            // то всегда будет выделена только одна цель
            if (currentUnit.UnitType.MainAttack.Reach != Reach.All) {
                AttachTargetAnimations(_targetUnitObject);
                return;
            }


            BattleUnit[] targetUnits;

            // Если юнит применяет способность на союзников (например, лекарь), то при наведении на союзника, будут выделяться все
            // Также наоборот, если юнит применяет способность на врагов, то выделятся все враги
            // Иначе, как например, лекарь при наведении на врага будет выделять только 1 врага
            if (currentUnit.Player == targetUnit.Player && currentUnit.HasAllyAbility() ||
                currentUnit.Player != targetUnit.Player && currentUnit.HasEnemyAbility()) {
                targetUnits = BattleUnits
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
            if (_targetUnitsAnimations != null)
                DetachTargetAnimations();

            _targetUnitsAnimations = new List<AnimationObject>(battleUnits.Length);
            foreach (var battleUnit in battleUnits)
            {
                var frames = _battleResourceProvider.GetBattleAnimation(
                    battleUnit.Unit.UnitType.SizeSmall
                        ? "MRKSMALLA"
                        : "MRKLARGEA");

                var targetAnimation = _battleSceneController.AddAnimation(frames, battleUnit.X, battleUnit.Y + 190, 1);
                _targetUnitsAnimations.Add(targetAnimation);
            }
        }

        /// <summary>
        /// Скрыть анимации выделения цели на всех юнитах.
        /// </summary>
        private void DetachTargetAnimations()
        {
            if (_targetUnitsAnimations == null)
                return;

            foreach (var targetAnimation in _targetUnitsAnimations) {
                _gameController.DestroyObject(targetAnimation);
            }

            _targetUnitsAnimations = null;
        }


        /// <summary>
        /// Установить какой отряд должен отображаться на правой панели.
        /// </summary>
        private void CheckRightPanelReflection()
        {
            var currentUnit = CurrentBattleUnit.Unit;
            var showEnemies = currentUnit.HasEnemyAbility();

            // Если текущий юнит находится в атакующем отряде, то мы отражаем панель только, если он лекарь и т.д.
            if (CurrentBattleUnit.IsAttacker) {
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

            _rightPanelUnits = BattleUnits
                .Where(u => u.IsAttacker && direction == BattleDirection.Attacker ||
                            !u.IsAttacker && direction == BattleDirection.Defender)
                .ToList();

            var portraits = new List<UnitPortraitObject>();
            foreach (var battleUnit in _rightPanelUnits) {
                var lineOffset = direction == BattleDirection.Defender
                    ? (battleUnit.Unit.SquadLinePosition + 1) % 2
                    : battleUnit.Unit.SquadLinePosition;

                var portrait = _battleSceneController.AddUnitPortrait(battleUnit.Unit,
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

            var currentUnit = CurrentBattleUnit.Unit;
            _unitPanelAnimations = new List<AnimationObject>();

            // Если отображается отряд текущего юнита, то нужно его выделить на панели.
            if (currentUnit.Player == _rightPanelUnits.First().Unit.Player) {
                var position = GetRightUnitPanelPosition(currentUnit.SquadLinePosition, currentUnit.SquadFlankPosition, CurrentBattleUnit.Direction);

                _unitPanelAnimations.Add(
                    _battleSceneController.AddAnimation(
                        _interfaceProvider.GetUnitSelectionBorder(currentUnit.UnitType.SizeSmall),
                        position.X,
                        position.Y,
                        INTERFACE_LAYER + 3));
            }

            // Если юнит бьёт по площади и цель юнита - отображаемый отряд, то добавляем одну большую рамку.
            if (currentUnit.UnitType.MainAttack.Reach == Reach.All &&
                _rightPanelUnits.Any(CanAttack)) {
                var position = GetRightUnitPanelPosition(1, 2, BattleDirection.Defender);

                _unitPanelAnimations.Add(
                    _battleSceneController.AddAnimation(
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
                    if (!CanAttack(targetUnit))
                        continue;

                    var position = GetRightUnitPanelPosition(targetUnit.Unit.SquadLinePosition,
                        targetUnit.Unit.SquadFlankPosition, targetUnit.Direction);

                    _unitPanelAnimations.Add(
                        _battleSceneController.AddAnimation(
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
                _gameController.DestroyObject(unitPanelAnimation);
            }

            _unitPanelAnimations = null;
        }

        /// <summary>
        /// Активировать указанные кнопки.
        /// </summary>
        private static void ActivateButtons(params ButtonObject[] buttons)
        {
            foreach (var button in buttons)
            {
                button.Activate();
            }
        }

        /// <summary>
        /// Деактивировать указанные кнопки.
        /// </summary>
        private static void DisableButtons(params ButtonObject[] buttons)
        {
            foreach (var button in buttons)
            {
                button.Disable();
            }
        }

        /// <summary>
        /// Проверить, может ли текущий юнит атаковать цель.
        /// </summary>
        private bool CanAttack(BattleUnit targetBattleUnit)
        {
            var attackingSquad = CurrentBattleUnit.IsAttacker
                ? _context.AttackingSquad
                : _context.DefendingSquad;
            var targetSquad = targetBattleUnit.IsAttacker
                ? _context.AttackingSquad
                : _context.DefendingSquad;
            return _battleProcessor.CanAttack(CurrentBattleUnit.Unit, attackingSquad, targetBattleUnit.Unit, targetSquad);
        }
    }
}