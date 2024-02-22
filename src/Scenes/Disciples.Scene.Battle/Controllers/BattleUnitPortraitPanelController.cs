using Disciples.Common.Models;
using Disciples.Engine;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Extensions;
using Disciples.Engine.Implementation.Base;
using Disciples.Scene.Battle.Constants;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.Extensions;
using Disciples.Scene.Battle.GameObjects;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Processors;

namespace Disciples.Scene.Battle.Controllers;

/// <summary>
/// Класс для работы с панелью с портретами юнитов.
/// </summary>
internal class BattleUnitPortraitPanelController : BaseSupportLoading
{
    /// <summary>
    /// Сдвиг для анимаций панели, чтобы она ложилась поверх портретов.
    /// </summary>
    private const int PANEL_ANIMATION_OFFSET = -2;

    private readonly IBattleGameObjectContainer _battleGameObjectContainer;
    private readonly IBattleInterfaceProvider _interfaceProvider;
    private readonly BattleContext _context;
    private readonly BattleProcessor _battleProcessor;

    private BattleUnitPortraitPanelData _rightPanel = null!;
    private BattleUnitPortraitPanelData _leftPanel = null!;

    private BattleUnitPortraitPanelData _mainPanel = null!;
    private BattleUnitPortraitPanelData _additionalPanel = null!;

    private bool _isActionProcessing;

    /// <summary>
    /// Отряд, который отображается на основной панели.
    /// </summary>
    private BattleSquadPosition? _displayingSquad;

    private ToggleButtonObject _reflectPanelButton = null!;

    /// <summary>
    /// Создать объект типа <see cref="BattleUnitPortraitPanelController" />.
    /// </summary>
    public BattleUnitPortraitPanelController(
        IBattleGameObjectContainer battleGameObjectContainer,
        IBattleInterfaceProvider interfaceProvider,
        BattleContext context,
        BattleProcessor battleProcessor)
    {
        _battleGameObjectContainer = battleGameObjectContainer;
        _interfaceProvider = interfaceProvider;
        _context = context;
        _battleProcessor = battleProcessor;
    }

    /// <summary>
    /// Признак, что отображаются обе панели (правая и левая).
    /// </summary>
    public bool IsDisplayingBothPanels { get; private set; }

    /// <summary>
    /// Отряд, который отображается на панели.
    /// </summary>
    public BattleSquadPosition DisplayingSquad => _displayingSquad!.Value;

    /// <summary>
    ///  Юнит, который выполняет свой ход.
    /// </summary>
    private BattleUnit CurrentBattleUnit => _context.CurrentBattleUnit;

    /// <summary>
    /// Список юнитов.
    /// </summary>
    private IReadOnlyList<BattleUnit> BattleUnits => _context.BattleUnits;

    /// <summary>
    /// Обработать положение курсора.
    /// </summary>
    /// <remarks>
    /// Сейчас нет обработки выхода курсора за пределы сцены.
    /// Поэтому выход за пределы сцены также обрабатывается как необходимо отображать дополнительную панель.
    /// </remarks>
    public void ProcessMousePosition(PointD mousePosition)
    {
        if (_isActionProcessing)
            return;

        var isMainRightPanel = _mainPanel == _rightPanel;

        if (IsDisplayingBothPanels)
        {
            var isNeedHideOppositePanel = isMainRightPanel
                ? mousePosition.X > _additionalPanel.DisplayPanelBounds.X + _additionalPanel.DisplayPanelBounds.Width
                : mousePosition.X < _additionalPanel.DisplayPanelBounds.X;
            if (isNeedHideOppositePanel)
            {
                IsDisplayingBothPanels = false;

                UpdatePanel(_mainPanel, _displayingSquad!.Value);
                RemovePanel(_additionalPanel);
            }
        }
        else
        {
            var isNeedDisplayOppositePanel = isMainRightPanel
                ? mousePosition.X < _additionalPanel.DisplayPanelBounds.X + _additionalPanel.DisplayPanelBounds.Width
                : mousePosition.X > _additionalPanel.DisplayPanelBounds.X;
            if (isNeedDisplayOppositePanel)
            {
                IsDisplayingBothPanels = true;

                UpdatePanel(_leftPanel, BattleSquadPosition.Attacker);
                UpdatePanel(_rightPanel, BattleSquadPosition.Defender);
            }
        }
    }

    /// <summary>
    /// Обработать начало действий на сцене.
    /// </summary>
    public void ProcessActionsBegin(BattleSquadPosition displayingSquad)
    {
        _isActionProcessing = true;
        _reflectPanelButton.SetDisabled();

        _displayingSquad = displayingSquad;

        // На время действий, дополнительная панель всегда скрыта.
        if (IsDisplayingBothPanels)
        {
            IsDisplayingBothPanels = false;

            UpdatePanel(_mainPanel, _displayingSquad.Value);
            RemovePanel(_additionalPanel);
        }
        else
        {
            // Здесь нельзя использовать UpdatePanel, так как требуется обязательный вызов UpdateBorders.
            ArrangePortraits(_mainPanel, _displayingSquad.Value);
            UpdateBorders(_mainPanel);
        }
    }

    /// <summary>
    /// Обработать завершение всех действий на сцене.
    /// </summary>
    public void ProcessActionsCompleted()
    {
        _isActionProcessing = false;
        _reflectPanelButton.SetActive();

        SetDisplayingSquad(GetDefaultDisplayingSquad());
        UpdateBorders(_mainPanel);
    }

    /// <summary>
    /// Завершить битву.
    /// </summary>
    public void ProcessBattleCompleted()
    {
        if (_displayingSquad != _context.BattleWinnerSquad!.Value)
            SetDisplayingSquad(_context.BattleWinnerSquad.Value);

        _isActionProcessing = false;
        _reflectPanelButton.SetActive();

        RemoveBorders(_mainPanel);
        RemoveBorders(_additionalPanel);
    }

    /// <summary>
    /// Получить портрет указанного юнита.
    /// </summary>
    public UnitPortraitObject? GetUnitPortrait(BattleUnit battleUnit)
    {
        return _mainPanel.UnitPortraits.FirstOrDefault(up => up.Unit == battleUnit.Unit)
               ?? _additionalPanel.UnitPortraits.FirstOrDefault(up => up.Unit == battleUnit.Unit);
    }

    /// <inheritdoc />
    protected override void LoadInternal()
    {
        var gameObjects = _battleGameObjectContainer.GameObjects;

        _reflectPanelButton = gameObjects.GetToggleButton(BattleUnitPanelElementNames.RIGHT_UNIT_PANEL_REFLECT_TOGGLE_BUTTON, ExecuteReflectUnitsPanel);

        _leftPanel = new BattleUnitPortraitPanelData
        {
            PanelImage = gameObjects.Get<ImageObject>(BattleUnitPanelElementNames.LEFT_UNIT_PANEL_IMAGE, true),
            PortraitPlaceholders = _interfaceProvider.GetUnitPlaceholders(BattleUnitPanelElementNames.LEFT_UNIT_PANEL_PORTRAIT_PATTERN_PLACEHOLDER),
            HitPointsPlaceholders = _interfaceProvider.GetUnitPlaceholders(BattleUnitPanelElementNames.LEFT_UNIT_PANEL_HP_PATTERN_PLACEHOLDER),
            DisplayPanelBounds = _interfaceProvider.BattleInterface.Elements[BattleUnitPanelElementNames.LEFT_PANEL_DISPLAY_PLACEHOLDER].Position
        };
        _rightPanel = new BattleUnitPortraitPanelData
        {
            PanelImage = gameObjects.Get<ImageObject>(BattleUnitPanelElementNames.RIGHT_UNIT_PANEL_IMAGE, true),
            PortraitPlaceholders = _interfaceProvider.GetUnitPlaceholders(BattleUnitPanelElementNames.RIGHT_UNIT_PANEL_PORTRAIT_PATTERN_PLACEHOLDER),
            HitPointsPlaceholders = _interfaceProvider.GetUnitPlaceholders(BattleUnitPanelElementNames.RIGHT_UNIT_PANEL_HP_PATTERN_PLACEHOLDER),

            // Этой области нет для правой панели, поэтому вычисляем зеркально из данных для левой.
            DisplayPanelBounds = new RectangleD(
                GameInfo.Width - _leftPanel.DisplayPanelBounds.Width,
                _leftPanel.DisplayPanelBounds.Y,
                _leftPanel.DisplayPanelBounds.Width,
                _leftPanel.DisplayPanelBounds.Height)
        };

        _mainPanel = _context.PlayerSquadPosition == BattleSquadPosition.Attacker ? _rightPanel : _leftPanel;
        _additionalPanel = _context.PlayerSquadPosition == BattleSquadPosition.Attacker ? _leftPanel : _rightPanel;

        SetDisplayingSquad(GetDefaultDisplayingSquad());
    }

    /// <inheritdoc />
    protected override void UnloadInternal()
    {
    }

    /// <summary>
    /// Поменять отряд, который отображается на основной панели с юнитам.
    /// </summary>
    private void ExecuteReflectUnitsPanel()
    {
        // Смена отряда не работает, если отображаются обе панели.
        if (IsDisplayingBothPanels)
        {
            // TODO Смена состояния кнопки происходит в самой кнопке. Здесь мы возвращаем его обратно.
            // Думаю, требуется другой механизм для обработки этого.
            _reflectPanelButton.SetState(_displayingSquad == BattleSquadPosition.Attacker);
            return;
        }

        SetDisplayingSquad(_displayingSquad == BattleSquadPosition.Defender
            ? BattleSquadPosition.Attacker
            : BattleSquadPosition.Defender);
    }

    /// <summary>
    /// Установить отображаемый отряд.
    /// </summary>
    private void SetDisplayingSquad(BattleSquadPosition displayingSquad)
    {
        if (_displayingSquad == displayingSquad)
            return;

        _displayingSquad = displayingSquad;

        // Если отображаются обе панели, то запоминаем новый отряд, который отображается на основной панели.
        // При этом само отображение не меняем.
        if (IsDisplayingBothPanels)
            return;

        UpdatePanel(_mainPanel, displayingSquad);

        // Кнопка "зажата", если отображается атакующий отряд.
        _reflectPanelButton.SetState(displayingSquad == BattleSquadPosition.Attacker);
    }

    /// <summary>
    /// Получить тип отряд, который должен отображаться на панели в режиме ожидания хода.
    /// </summary>
    private BattleSquadPosition GetDefaultDisplayingSquad()
    {
        var currentUnit = CurrentBattleUnit.Unit;
        var showEnemies = currentUnit.HasEnemyAbility();

        return showEnemies
            ? CurrentBattleUnit.SquadPosition.GetOpposite()
            : CurrentBattleUnit.SquadPosition;
    }

    /// <summary>
    /// Обновить панель.
    /// </summary>
    private void UpdatePanel(BattleUnitPortraitPanelData panel, BattleSquadPosition squadPosition)
    {
        if (panel.PanelSquadDirection == squadPosition)
            return;

        ArrangePortraits(panel, squadPosition);
        UpdateBorders(panel);
    }

    /// <summary>
    /// Разместить портреты юнитов на панели.
    /// </summary>
    private void ArrangePortraits(BattleUnitPortraitPanelData panel, BattleSquadPosition squadPosition)
    {
        // Если юниты уже расположены на панели и отряд, который необходимо отображать не изменился,
        // То нет необходимости что-либо менять.
        if (panel.PanelSquadDirection == squadPosition)
            return;

        panel.PanelImage.IsHidden = false;
        panel.PanelSquadDirection = squadPosition;

        // Удаляем старые портреты.
        foreach (var portrait in panel.UnitPortraits)
            portrait.Destroy();

        var battleUnits = BattleUnits
            .Where(u => u.SquadPosition == squadPosition)
            .ToList();

        var portraits = new List<UnitPortraitObject>();
        foreach (var battleUnit in battleUnits)
        {
            var portraitBounds = GetBounds(battleUnit.Unit, squadPosition, panel.PortraitPlaceholders);
            var hitPointsBounds = GetBounds(battleUnit.Unit, squadPosition, panel.HitPointsPlaceholders);
            var portrait = _battleGameObjectContainer.AddUnitPortrait(battleUnit.Unit, squadPosition, portraitBounds, hitPointsBounds);
            portraits.Add(portrait);
        }

        panel.BattleUnits = battleUnits;
        panel.UnitPortraits = portraits;
    }

    /// <summary>
    /// Получить координаты на сцене для рамки портрета юнита.
    /// </summary>
    private static RectangleD GetBounds(Unit unit, BattleSquadPosition squadPosition, IReadOnlyDictionary<int, SceneElement> placeholders)
    {
        var placeholderId = GetPlaceholderId(unit.SquadLinePosition, unit.SquadFlankPosition, squadPosition);
        var placeholderPosition = placeholders[placeholderId].Position;
        if (unit.UnitType.IsSmall)
            return placeholderPosition;

        var secondPlaceHolderId = GetPlaceholderId(UnitSquadLinePosition.Back, unit.SquadFlankPosition, squadPosition);
        var secondPlaceHolderPosition = placeholders[secondPlaceHolderId].Position;
        return RectangleD.Union(placeholderPosition, secondPlaceHolderPosition);
    }

    /// <summary>
    /// Получить идентификатор плейсхолдера.
    /// </summary>
    private static int GetPlaceholderId(UnitSquadLinePosition squadLinePosition, UnitSquadFlankPosition squadFlankPosition, BattleSquadPosition squadPosition)
    {
        var lineOffset = (int)squadLinePosition;
        var flankOffset = (int)squadFlankPosition;

        return squadPosition == BattleSquadPosition.Defender
            ? 6 - lineOffset - flankOffset * 2
            : 5 + lineOffset - flankOffset * 2;
    }

    /// <summary>
    /// Удалить целиком панель.
    /// </summary>
    private static void RemovePanel(BattleUnitPortraitPanelData panel)
    {
        foreach (var unitPortrait in panel.UnitPortraits)
        {
            unitPortrait.Destroy();
        }

        foreach (var borderAnimation in panel.BorderAnimations)
        {
            borderAnimation.Destroy();
        }

        panel.PanelImage.IsHidden = true;
        panel.PanelSquadDirection = null;
        panel.BattleUnits = Array.Empty<BattleUnit>();
        panel.UnitPortraits = Array.Empty<UnitPortraitObject>();
        panel.BorderAnimations = Array.Empty<AnimationObject>();
    }

    /// <summary>
    /// Обновить состояние анимаций рамок на панелях.
    /// </summary>
    private void UpdateBorders(BattleUnitPortraitPanelData panel)
    {
        RemoveBorders(panel);

        // Панель не отображается, никаких анимаций размещать не нужно.
        if (panel.PanelSquadDirection == null)
            return;

        // Если битва закончена, никаких дополнительных рамок не нужно.
        if (_context.BattleState == BattleState.WaitExit)
            return;

        var currentUnit = CurrentBattleUnit.Unit;
        var unitPanelAnimations = new List<AnimationObject>();

        // Если отображается отряд текущего юнита, то нужно его выделить на панели.
        if (CurrentBattleUnit.SquadPosition == panel.PanelSquadDirection)
        {
            var currentUnitPortrait = GetUnitPortrait(panel, CurrentBattleUnit);
            unitPanelAnimations.Add(
                _battleGameObjectContainer.AddAnimation(
                    _interfaceProvider.GetUnitSelectionBorder(currentUnit.UnitType.IsSmall),
                    currentUnitPortrait.X + PANEL_ANIMATION_OFFSET,
                    currentUnitPortrait.Y + PANEL_ANIMATION_OFFSET,
                    BattleLayers.INTERFACE_ANIMATION_LAYER));
        }

        // Рамки для целей отображаем только в момент, когда нет действий.
        if (!_isActionProcessing)
        {
            // Если юнит бьёт по площади и цель юнита - отображаемый отряд, то добавляем одну большую рамку.
            if (currentUnit.UnitType.MainAttack.Reach == UnitAttackReach.All &&
                panel.BattleUnits.Any(CanAttack))
            {
                // Плейсхолдер 1 - это всегда левый верхний портрет.
                var topLeftElement = panel.PortraitPlaceholders[1];
                unitPanelAnimations.Add(
                    _battleGameObjectContainer.AddAnimation(
                        currentUnit.HasAllyAbility()
                            ? _interfaceProvider.GetFieldHealBorder()
                            : _interfaceProvider.GetFieldAttackBorder(),
                        topLeftElement.Position.X + PANEL_ANIMATION_OFFSET,
                        topLeftElement.Position.Y + PANEL_ANIMATION_OFFSET,
                        BattleLayers.INTERFACE_ANIMATION_LAYER));
            }
            // Иначе добавляем рамку только тем юнитам, которых можно атаковать.
            else
            {
                foreach (var targetBattleUnit in panel.BattleUnits)
                {
                    if (!CanAttack(targetBattleUnit))
                        continue;

                    var unitPortrait = GetUnitPortrait(panel, targetBattleUnit);
                    unitPanelAnimations.Add(
                        _battleGameObjectContainer.AddAnimation(
                            currentUnit.HasAllyAbility()
                                ? _interfaceProvider.GetUnitHealBorder(targetBattleUnit.Unit.UnitType.IsSmall)
                                : _interfaceProvider.GetUnitAttackBorder(targetBattleUnit.Unit.UnitType.IsSmall),
                            unitPortrait.X + PANEL_ANIMATION_OFFSET,
                            unitPortrait.Y + PANEL_ANIMATION_OFFSET,
                            BattleLayers.INTERFACE_ANIMATION_LAYER));
                }
            }
        }

        panel.BorderAnimations = unitPanelAnimations;
    }

    /// <summary>
    /// Очистить все анимации на панели юнитов.
    /// </summary>
    private static void RemoveBorders(BattleUnitPortraitPanelData panel)
    {
        if (panel.BorderAnimations.Count == 0)
            return;

        foreach (var unitPanelAnimation in panel.BorderAnimations)
            unitPanelAnimation.Destroy();

        panel.BorderAnimations = Array.Empty<AnimationObject>();
    }

    /// <summary>
    /// Проверить, может ли текущий юнит атаковать цель.
    /// </summary>
    private bool CanAttack(BattleUnit targetBattleUnit)
    {
        var attackContext = _context.CreateAttackProcessorContext(targetBattleUnit);
        return _battleProcessor.CanAttack(attackContext);
    }

    /// <summary>
    /// Получить портрет юнита.
    /// </summary>
    private static UnitPortraitObject GetUnitPortrait(BattleUnitPortraitPanelData panelData, BattleUnit battleUnit)
    {
        return panelData.UnitPortraits.First(up => up.Unit == battleUnit.Unit);
    }
}