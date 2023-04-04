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
using Disciples.Scene.Battle.Extensions;
using Disciples.Scene.Battle.GameObjects;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Models.BattleActions;
using Disciples.Scene.Battle.Providers;

namespace Disciples.Scene.Battle.Controllers;

/// <inheritdoc cref="IBattleInterfaceController" />
internal class BattleInterfaceController : BaseSupportLoading, IBattleInterfaceController
{
    /// <summary>
    /// Слой для расположения интерфейса.
    /// </summary>
    private const int INTERFACE_LAYER = 1000;

    private readonly IBattleGameObjectContainer _battleGameObjectContainer;
    private readonly IBattleInterfaceProvider _interfaceProvider;
    private readonly IBattleResourceProvider _battleResourceProvider;
    private readonly IBattleUnitResourceProvider _battleUnitResourceProvider;
    private readonly BattleContext _context;
    private readonly BattleProcessor _battleProcessor;
    private readonly ISceneObjectContainer _sceneObjectContainer;
    private readonly BattleUnitPortraitPanelController _unitPortraitPanelController;

    private IImageSceneObject _currentUnitFace = null!;
    private BattleUnitInfoGameObject _currentUnitTextInfoObject = null!;
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

    private ButtonObject? _defendButton;
    private ButtonObject? _retreatButton;
    private ButtonObject? _waitButton;
    private ButtonObject? _instantResolveButton;
    private ToggleButtonObject? _autoBattleButton;

    /// <summary>
    /// Создать объект типа <see cref="BattleInterfaceController" />.
    /// </summary>
    public BattleInterfaceController(
        IBattleGameObjectContainer battleGameObjectContainer,
        IBattleInterfaceProvider battleInterfaceProvider,
        IBattleResourceProvider battleResourceProvider,
        IBattleUnitResourceProvider battleUnitResourceProvider,
        BattleContext context,
        BattleProcessor battleProcessor,
        ISceneObjectContainer sceneObjectContainer,
        BattleUnitPortraitPanelController unitPortraitPanelController)
    {
        _battleGameObjectContainer = battleGameObjectContainer;
        _interfaceProvider = battleInterfaceProvider;
        _battleResourceProvider = battleResourceProvider;
        _battleUnitResourceProvider = battleUnitResourceProvider;
        _context = context;
        _battleProcessor = battleProcessor;
        _sceneObjectContainer = sceneObjectContainer;
        _unitPortraitPanelController = unitPortraitPanelController;
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
    /// Признак, что ходит юнит, который "ждал" в этом раунде.
    /// </summary>
    private bool IsWaitingUnitTurn => _context.IsWaitingUnitTurn;

    /// <summary>
    /// Признак, что битва проходит в автоматическом режиме.
    /// </summary>
    private bool IsAutoBattle
    {
        get => _context.IsAutoBattle;
        set => _context.IsAutoBattle = value;
    }

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

        _unitPortraitPanelController.Load();

        var displayingSquad = GetPanelDisplayingSquad();
        if (Actions.IsNoActions)
            _unitPortraitPanelController.EnablePanelSwitch(displayingSquad);
        else
            _unitPortraitPanelController.DisablePanelSwitch(displayingSquad);

        AttachSelectedAnimation(CurrentBattleUnit);
    }

    /// <inheritdoc />
    protected override void UnloadInternal()
    {
        _unitPortraitPanelController.Unload();

        // todo Зачистить все элементы интерфейса.
    }

    /// <summary>
    /// Разместить на сцене изображение поля боя и панелей, текст на панелях.
    /// </summary>
    private void InitializeMainInterface()
    {
        foreach (var battleground in _interfaceProvider.Battleground)
            _sceneObjectContainer.AddImage(battleground, 0, 0, 0);

        _sceneObjectContainer.AddImage(_interfaceProvider.BottomPanel, 0, GameInfo.OriginalHeight - _interfaceProvider.BottomPanel.Height, 1);

        var currentUnitBattleFace = _battleUnitResourceProvider.GetUnitBattleFace(CurrentBattleUnit.Unit.UnitType);
        _currentUnitFace = _sceneObjectContainer.AddImage(
            currentUnitBattleFace,
            0,
            GameInfo.OriginalHeight - currentUnitBattleFace.Height - 5,
            INTERFACE_LAYER + 1);

        _currentUnitTextInfoObject = _battleGameObjectContainer.AddBattleUnitInfo(190, 520, INTERFACE_LAYER + 1);
        _currentUnitTextInfoObject.Unit = CurrentBattleUnit.Unit;


        // Используем размеры лица текущего юнита, так как юнит-цель не инициализируются в начале боя.
        _targetUnitFace = _sceneObjectContainer.AddImage(
            null,
            currentUnitBattleFace.Width,
            currentUnitBattleFace.Height,
            GameInfo.OriginalWidth - currentUnitBattleFace.Width,
            GameInfo.OriginalHeight - currentUnitBattleFace.Height - 5,
            INTERFACE_LAYER + 1);
        // todo Добавить перегрузку в метод?
        _targetUnitFace.IsReflected = true;

        _targetUnitTextInfoObject = _battleGameObjectContainer.AddBattleUnitInfo(510, 520, INTERFACE_LAYER + 1);
    }


    /// <summary>
    /// Разместить на сцене кнопки.
    /// </summary>
    private void InitializeButtons()
    {
        _defendButton = _battleGameObjectContainer.AddButton(_interfaceProvider.DefendButton, () => {
            Actions.Add(new DefendBattleAction());
        }, 380, 504, INTERFACE_LAYER + 2, KeyboardButton.D);

        _retreatButton = _battleGameObjectContainer.AddButton(_interfaceProvider.RetreatButton, () => {
            //todo
        }, 343, 524, INTERFACE_LAYER + 2, KeyboardButton.R);

        _waitButton = _battleGameObjectContainer.AddButton(_interfaceProvider.WaitButton, () => {
            Actions.Add(new WaitingBattleAction());
        }, 419, 524, INTERFACE_LAYER + 2, KeyboardButton.W);

        _instantResolveButton = _battleGameObjectContainer.AddButton(_interfaceProvider.InstantResolveButton, () => {
            // todo
        }, 359, 563, INTERFACE_LAYER + 2, KeyboardButton.I);

        _autoBattleButton = _battleGameObjectContainer.AddToggleButton(_interfaceProvider.AutoBattleButton, () => {
            IsAutoBattle = _autoBattleButton!.IsChecked;
        }, 403, 563, INTERFACE_LAYER + 2, KeyboardButton.A);


        // Эти кнопки доступны всегда.
        ActivateButtons(_instantResolveButton, _autoBattleButton);

        // Эти кнопки могут быть недоступны, если первый ход - компьютера.
        if (Actions.IsAllActionsCompleted)
            ActivateButtons(_defendButton, _retreatButton, _waitButton);
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
        _targetUnitFace!.Bitmap = _battleUnitResourceProvider.GetUnitBattleFace(targetUnitObject.Unit.UnitType);
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
        _detailUnitInfoObject = _battleGameObjectContainer.ShowDetailUnitInfo(unit);
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
        if (actionType == InputDeviceActionType.MouseRight && actionState == InputDeviceActionState.Deactivated)
        {
            GameObjectRightButtonReleased(gameObject);
            return;
        }

        // Если ПКМ зажата, то не меняем выбранного юнита до тех пор, пока не будет отпущена кнопка.
        if (_isUnitInfoShowing)
            return;

        switch (actionType)
        {
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
            button.SetSelected();
        }
    }

    /// <summary>
    /// Обработчик события, что с игрового объекта был смещён фокус.
    /// </summary>
    private void GameObjectUnselected(GameObject? gameObject)
    {
        if (gameObject is BattleUnit)
        {
            UpdateTargetUnit(null);
        }
        else if (gameObject is ButtonObject button)
        {
            button.SetUnselected();
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
            button.Press();
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
            button.Release();
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

        var attackAction = Actions.New.OfType<BeginAttackUnitBattleAction>().FirstOrDefault();
        var displayingSquad = attackAction != null
            // Показываем отряд атакуемого юнита.
            ? attackAction.TargetBattleUnit.SquadPosition
            // Иначе это защита/ожидания и другое действие. Показываем отряд текущего юнита.
            : CurrentBattleUnit.SquadPosition;
        _unitPortraitPanelController.DisablePanelSwitch(displayingSquad);

        DisableButtons(_defendButton, _retreatButton, _waitButton);
        DetachSelectedAnimation();
        DetachTargetAnimations();
    }

    /// <summary>
    /// Обработать завершение всех действий на сцене.
    /// </summary>
    private void ProcessActionsCompleted()
    {
        _isAnimating = false;

        _unitPortraitPanelController.EnablePanelSwitch(GetPanelDisplayingSquad());

        // Если юнит наносит второй удар, то указанные кнопки активировать не нужно.
        if (!IsSecondAttack)
        {
            ActivateButtons(_defendButton, _retreatButton);

            // Если юнит уже ждал на этом ходу, то больше ждать не может.
            if (!IsWaitingUnitTurn)
                ActivateButtons(_waitButton);
        }

        AttachSelectedAnimation(CurrentBattleUnit);

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
            var portrait = _unitPortraitPanelController.GetUnitPortrait(unitBattleAction.TargetUnit);
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
            var portrait = _unitPortraitPanelController.GetUnitPortrait(unitBattleAction.TargetUnit);
            if (portrait == null)
                return;

            portrait.ProcessCompletedUnitAction();
        }
    }

    /// <summary>
    /// Обработать завершение битвы.
    /// </summary>
    private void ProcessBattleCompleted()
    {
        // BUG. Не работает информация о юните после завершения битвы.

        _isAnimating = false;

        _defendButton?.Destroy();
        _defendButton = null;

        _retreatButton?.Destroy();
        _retreatButton = null;


        _waitButton?.Destroy();
        _waitButton = null;

        _instantResolveButton?.Destroy();
        _instantResolveButton = null;

        _autoBattleButton?.Destroy();
        _autoBattleButton = null;

        // todo Создать две новые кнопки - "выйти" и "выйти и открыть" интерфейс.

        // Отображаем отряд победителя.
        _unitPortraitPanelController.CompleteBattle(CurrentBattleUnit.SquadPosition);

        if (_targetUnitObject != null)
            SelectTargetUnits();
    }


    /// <summary>
    /// Отобразить анимацию выделения на текущем юните.
    /// </summary>
    private void AttachSelectedAnimation(BattleUnit battleUnit)
    {
        _currentUnitFace.Bitmap = _battleUnitResourceProvider.GetUnitBattleFace(battleUnit.Unit.UnitType);
        _currentUnitTextInfoObject.Unit = battleUnit.Unit;

        DetachSelectedAnimation();

        var frames = _battleResourceProvider.GetBattleAnimation(
            battleUnit.Unit.UnitType.IsSmall
                ? "MRKCURSMALLA"
                : "MRKCURLARGEA");


        // Задаём смещение 190. Возможно, стоит вычислять высоту юнита или что-то в этом роде.
        _currentUnitSelectionAnimation = _battleGameObjectContainer.AddAnimation(frames, battleUnit.X, battleUnit.Y + 190, 1);
    }

    /// <summary>
    /// Скрыть анимацию выделения на текущем юните.
    /// </summary>
    private void DetachSelectedAnimation()
    {
        if (_currentUnitSelectionAnimation == null)
            return;

        _currentUnitSelectionAnimation?.Destroy();
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
        if (currentUnit.UnitType.MainAttack.Reach != UnitAttackReach.All)
        {
            AttachTargetAnimations(_targetUnitObject);
            return;
        }


        BattleUnit[] targetUnits;

        // Если юнит применяет способность на союзников (например, лекарь), то при наведении на союзника, будут выделяться все
        // Также наоборот, если юнит применяет способность на врагов, то выделятся все враги
        // Иначе, как например, лекарь при наведении на врага будет выделять только 1 врага
        if (currentUnit.Player == targetUnit.Player && currentUnit.HasAllyAbility() ||
            currentUnit.Player != targetUnit.Player && currentUnit.HasEnemyAbility())
        {
            targetUnits = BattleUnits
                .Where(u => u.Unit.Player == targetUnit.Player && u.Unit.IsDead == false)
                .ToArray();
        }
        else
        {
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
                battleUnit.Unit.UnitType.IsSmall
                    ? "MRKSMALLA"
                    : "MRKLARGEA");

            var targetAnimation = _battleGameObjectContainer.AddAnimation(frames, battleUnit.X, battleUnit.Y + 190, 1);
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

        foreach (var targetAnimation in _targetUnitsAnimations)
            targetAnimation.Destroy();

        _targetUnitsAnimations = null;
    }

    /// <summary>
    /// Получить тип отряд, который должен отображаться на панели.
    /// </summary>
    private BattleSquadPosition GetPanelDisplayingSquad()
    {
        var currentUnit = CurrentBattleUnit.Unit;
        var showEnemies = currentUnit.HasEnemyAbility();

        return showEnemies
            ? CurrentBattleUnit.SquadPosition.GetOpposite()
            : CurrentBattleUnit.SquadPosition;
    }

    /// <summary>
    /// Активировать указанные кнопки.
    /// </summary>
    private static void ActivateButtons(params ButtonObject?[] buttons)
    {
        foreach (var button in buttons)
        {
            button?.SetActive();
        }
    }

    /// <summary>
    /// Деактивировать указанные кнопки.
    /// </summary>
    private static void DisableButtons(params ButtonObject?[] buttons)
    {
        foreach (var button in buttons)
        {
            button?.SetDisabled();
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