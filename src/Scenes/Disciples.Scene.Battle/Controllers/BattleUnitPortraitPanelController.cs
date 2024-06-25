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
using Microsoft.Extensions.Logging;

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
    /// <summary>
    /// Слой на котором располагаются анимации.
    /// </summary>
    private const int PANEL_ANIMATION_LAYER = BattleLayers.INTERFACE_ANIMATION_LAYER + 5;

    private readonly ILogger<BattleUnitPortraitPanelController> _logger;
    private readonly IBattleGameObjectContainer _battleGameObjectContainer;
    private readonly IBattleInterfaceProvider _interfaceProvider;
    private readonly BattleContext _context;
    private readonly BattleProcessor _battleProcessor;

    private readonly Dictionary<string, BattleUnitPortraitEventData> _unitMessages = new();

    private BattleUnitPortraitPanelData _rightPanel = null!;
    private BattleUnitPortraitPanelData _leftPanel = null!;

    private BattleUnitPortraitPanelData _mainPanel = null!;
    private BattleUnitPortraitPanelData _additionalPanel = null!;

    private bool _isActionProcessing;
    private bool _isBorderAnimationsDisabled;

    /// <summary>
    /// Отряд, который отображается на основной панели.
    /// </summary>
    private BattleSquadPosition? _displayingSquad;

    private ToggleButtonObject _reflectPanelButton = null!;

    /// <summary>
    /// Создать объект типа <see cref="BattleUnitPortraitPanelController" />.
    /// </summary>
    public BattleUnitPortraitPanelController(
        ILogger<BattleUnitPortraitPanelController> logger,
        IBattleGameObjectContainer battleGameObjectContainer,
        IBattleInterfaceProvider interfaceProvider,
        BattleContext context,
        BattleProcessor battleProcessor)
    {
        _logger = logger;
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
    /// Курсор может выходить за пределы сцены,
    /// Если это происходит, то отображаем дополнительную панель.
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
    /// Обработать начало действия на сцене.
    /// </summary>
    public void ProcessActionBegin()
    {
        _isActionProcessing = true;
        _reflectPanelButton.SetDisabled();

        // На время действий, дополнительная панель всегда скрыта.
        if (IsDisplayingBothPanels)
        {
            IsDisplayingBothPanels = false;
            RemovePanel(_additionalPanel);
        }

        UpdateBorders(_mainPanel);
        UpdateSummonPlaceholders(_mainPanel);
    }

    /// <summary>
    /// Обработать завершение всех действий на сцене.
    /// </summary>
    public void ProcessActionCompleted()
    {
        _isActionProcessing = false;
        _reflectPanelButton.SetActive();

        SetDisplayingSquad(GetDefaultDisplayingSquad());
        UpdateBorders(_mainPanel);
        UpdateSummonPlaceholders(_mainPanel);
    }

    /// <summary>
    /// Завершить битву.
    /// </summary>
    public void ProcessBattleCompleted()
    {
        _isActionProcessing = false;
        _reflectPanelButton.SetActive();
    }

    /// <summary>
    /// Обработать изменение состояния юнита.
    /// Это может быть удаление, обновление или трансформация.
    /// </summary>
    public void ProcessBattleUnitUpdated(BattleUnit battleUnit)
    {
        var panel = _mainPanel.PanelSquadDirection == battleUnit.SquadPosition
            ? _mainPanel
            : _additionalPanel.PanelSquadDirection == battleUnit.SquadPosition
                ? _additionalPanel
                : null;
        if (panel == null)
            return;

        // Принудительно перерисовываем панель целиком.
        // TODO Также перевести на эту схему юнитов после превращения/повышения уровня.
        // TODO Не слишком быстрая операция. Будет создано много объектов, если менялось сразу несколько юнитов.
        panel.PanelSquadDirection = null;
        UpdatePanel(panel, battleUnit.SquadPosition);
    }

    /// <summary>
    /// Убрать все анимации на панели юнитов (текущий юнит, допустимые цели).
    /// </summary>
    /// <remarks>
    /// Используется при завершении битвы, флаг сбрасывать не нужно.
    /// </remarks>
    public void DisableBorderAnimations()
    {
        _isBorderAnimationsDisabled = true;
        RemoveBorders(_mainPanel);
    }

    /// <summary>
    /// Отобразить указанный отряд.
    /// </summary>
    public void SetDisplayingSquad(BattleSquadPosition displayingSquad)
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
    /// Отобразить сообщение на портрете юнита.
    /// </summary>
    public void DisplayMessage(BattleUnit battleUnit, BattleUnitPortraitEventData portraitEventData)
    {
        var unitId = battleUnit.Unit.Id;

        _logger.LogDebug($"Display message: unit {unitId}, message {portraitEventData.UnitActionType}");

        if (_unitMessages.ContainsKey(unitId))
        {
            _logger.LogCritical($"Unit {unitId} already has a message");
            CloseMessage(battleUnit);
        }

        _unitMessages.Add(unitId, portraitEventData);

        var unitPortrait = GetUnitPortrait(battleUnit);
        unitPortrait?.ShowMessage(portraitEventData);
    }

    /// <summary>
    /// Закрыть сообщение на портрете юнита.
    /// </summary>
    public void CloseMessage(BattleUnit battleUnit)
    {
        _unitMessages.Remove(battleUnit.Unit.Id);

        var unitPortrait = GetUnitPortrait(battleUnit);
        unitPortrait?.CloseMessage();
    }

    /// <summary>
    /// Получить портрет указанного юнита.
    /// </summary>
    public UnitPortraitObject? GetUnitPortrait(BattleUnit battleUnit)
    {
        return _mainPanel.UnitPortraits.FirstOrDefault(up => up.Unit.Id == battleUnit.Unit.Id)
               ?? _additionalPanel.UnitPortraits.FirstOrDefault(up => up.Unit.Id == battleUnit.Unit.Id);
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

            // В ресурсах нет границы отображения для правой панели, поэтому вычисляем зеркально из данных для левой.
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
    /// Получить тип отряд, который должен отображаться на панели в режиме ожидания хода.
    /// </summary>
    /// <remarks>
    /// Если текущий юнит может атаковать хотя бы одного врага, то отображается вражеский отряд.
    /// Иначе его собственный отряд.
    /// </remarks>
    private BattleSquadPosition GetDefaultDisplayingSquad()
    {
        var enemySquad = _battleProcessor.GetUnitEnemySquad(CurrentBattleUnit.Unit);
        var isNeedShowEnemies = enemySquad
            .Units
            .Select(_context.GetBattleUnit)
            .Any(CanAttack);
        return isNeedShowEnemies
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
        UpdateSummonPlaceholders(panel);
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
            var portraitBounds = GetBounds(battleUnit.SquadPosition, battleUnit.Unit.Position, panel.PortraitPlaceholders);
            var hitPointsBounds = GetBounds(battleUnit.SquadPosition, battleUnit.Unit.Position, panel.HitPointsPlaceholders);
            var portrait = _battleGameObjectContainer.AddUnitPortrait(battleUnit.Unit, squadPosition, portraitBounds, hitPointsBounds);
            portraits.Add(portrait);

            if (_unitMessages.TryGetValue(battleUnit.Unit.Id, out var message))
                portrait.ShowMessage(message);
        }

        panel.BattleUnits = battleUnits;
        panel.UnitPortraits = portraits;
    }

    /// <summary>
    /// Получить координаты на сцене для рамки портрета юнита.
    /// </summary>
    private static RectangleD GetBounds(BattleSquadPosition squadPosition, UnitSquadPosition unitPosition, IReadOnlyDictionary<int, SceneElement> placeholders)
    {
        var placeholderId = GetPlaceholderId(unitPosition.Line, unitPosition.Flank, squadPosition);
        var placeholderPosition = placeholders[placeholderId].Position;
        if (unitPosition.Line != UnitSquadLinePosition.Both)
            return placeholderPosition;

        var secondPlaceHolderId = GetPlaceholderId(UnitSquadLinePosition.Back, unitPosition.Flank, squadPosition);
        var secondPlaceHolderPosition = placeholders[secondPlaceHolderId].Position;
        return RectangleD.Union(placeholderPosition, secondPlaceHolderPosition);
    }

    /// <summary>
    /// Получить идентификатор плейсхолдера.
    /// </summary>
    private static int GetPlaceholderId(UnitSquadLinePosition squadLinePosition, UnitSquadFlankPosition squadFlankPosition, BattleSquadPosition squadPosition)
    {
        var lineOffset = squadLinePosition.ToIndex();
        var flankOffset = squadFlankPosition.ToIndex();

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

        foreach (var summonPlaceholder in panel.SummonPlaceholders)
        {
            summonPlaceholder.Destroy();
        }

        panel.PanelImage.IsHidden = true;
        panel.PanelSquadDirection = null;
        panel.BattleUnits = Array.Empty<BattleUnit>();
        panel.UnitPortraits = Array.Empty<UnitPortraitObject>();
        panel.BorderAnimations = Array.Empty<AnimationObject>();
        panel.SummonPlaceholders = Array.Empty<SummonPlaceholder>();
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
        if (_isBorderAnimationsDisabled)
            return;

        var currentUnit = CurrentBattleUnit.Unit;
        var unitPanelAnimations = new List<AnimationObject>();

        // Если отображается отряд текущего юнита, то нужно его выделить на панели.
        if (CurrentBattleUnit.SquadPosition == panel.PanelSquadDirection && !CurrentBattleUnit.IsDestroyed)
        {
            var currentUnitPortrait = GetUnitPortrait(panel, CurrentBattleUnit);
            unitPanelAnimations.Add(
                _battleGameObjectContainer.AddAnimation(
                    _interfaceProvider.GetUnitSelectionBorder(currentUnit.UnitType.IsSmall),
                    currentUnitPortrait.X + PANEL_ANIMATION_OFFSET,
                    currentUnitPortrait.Y + PANEL_ANIMATION_OFFSET,
                    PANEL_ANIMATION_LAYER));
        }

        // Рамки для целей отображаем только в момент, когда нет действий.
        if (!_isActionProcessing)
        {
            var mainAttack = currentUnit.MainAttack;
            var alternativeAttack = currentUnit.AlternativeAttack;
            var secondaryAttack = currentUnit.SecondaryAttack;

            // Если юнит бьёт по площади и цель юнита - отображаемый отряд, то добавляем одну большую рамку.
            var canMainAttack = mainAttack.Reach == UnitAttackReach.All &&
                                panel.BattleUnits.Any(bu => CanAttack(bu, mainAttack, secondaryAttack));
            var canAlternativeAttack = alternativeAttack?.Reach == UnitAttackReach.All &&
                                       panel.BattleUnits.Any(bu => CanAttack(bu, alternativeAttack, secondaryAttack));
            if (canMainAttack || canAlternativeAttack)
            {
                // Плейсхолдер 1 - это всегда левый верхний портрет.
                var attack = canMainAttack
                    ? mainAttack
                    : alternativeAttack!;
                var topLeftElement = panel.PortraitPlaceholders[1];
                unitPanelAnimations.Add(
                    _battleGameObjectContainer.AddAnimation(
                        attack.AttackType.IsAllyAttack()
                            ? _interfaceProvider.GetFieldHealBorder()
                            : _interfaceProvider.GetFieldAttackBorder(),
                        topLeftElement.Position.X + PANEL_ANIMATION_OFFSET,
                        topLeftElement.Position.Y + PANEL_ANIMATION_OFFSET,
                        PANEL_ANIMATION_LAYER));
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
                            PANEL_ANIMATION_LAYER));
                }
            }

            // Если отображается отряд текущего юнита и он призыватель,
            // То располагаем рамку на тех местах, куда можно призвать юнита.
            if (CurrentBattleUnit.SquadPosition == panel.PanelSquadDirection && _context.SummonPlaceholders.Count > 0)
            {
                foreach (var summonPlaceholder in _context.SummonPlaceholders)
                {
                    var bounds = GetBounds(summonPlaceholder.SquadPosition, summonPlaceholder.UnitPosition, panel.PortraitPlaceholders);
                    unitPanelAnimations.Add(
                        _battleGameObjectContainer.AddAnimation(
                            _interfaceProvider.GetUnitSummonBorder(),
                            bounds.X + PANEL_ANIMATION_OFFSET,
                            bounds.Y + PANEL_ANIMATION_OFFSET,
                            PANEL_ANIMATION_LAYER));
                }
            }
        }

        panel.BorderAnimations = unitPanelAnimations;
    }

    /// <summary>
    /// Обновить плейхолдеры для вызова юнита.
    /// </summary>
    private void UpdateSummonPlaceholders(BattleUnitPortraitPanelData panel)
    {
        RemoveSummonPlaceholders(panel);

        // Плейхолдеры вызова отображаются только для отряда текущего юнита.
        // Также сразу пропускаем, если текущий юнит не призыватель.
        if (panel.PanelSquadDirection != CurrentBattleUnit.SquadPosition ||
            _context.SummonPlaceholders.Count == 0)
        {
            return;
        }

        var summonPlaceholders = new List<SummonPlaceholder>();
        foreach (var mainAreaSummonPlaceholder in _context.SummonPlaceholders)
        {
            var bounds = GetBounds(mainAreaSummonPlaceholder.SquadPosition, mainAreaSummonPlaceholder.UnitPosition, panel.PortraitPlaceholders);
            var summonPlaceholder = _battleGameObjectContainer.AddSummonPlaceholder(mainAreaSummonPlaceholder.SquadPosition, mainAreaSummonPlaceholder.UnitPosition, bounds);
            summonPlaceholder.AnimationComponent.IsEnabled = false;
            summonPlaceholders.Add(summonPlaceholder);

            // Если плейсхолдер перекрывает юнита, то запрещаем выделять его.
            // Все события будет обрабатывать плейсхолдер.
            var hiddenPortraits = _context
                .GetBattleUnits(mainAreaSummonPlaceholder.SquadPosition, mainAreaSummonPlaceholder.UnitPosition)
                .Select(GetUnitPortrait);
            foreach (var hiddenUnitPortrait in hiddenPortraits)
            {
                if (hiddenUnitPortrait != null)
                    hiddenUnitPortrait.SelectionComponent!.IsSelectionEnabled = false;
            }
        }

        panel.SummonPlaceholders = summonPlaceholders;
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
    /// Удалить плейхолдеры для вызова юнитов.
    /// </summary>
    private static void RemoveSummonPlaceholders(BattleUnitPortraitPanelData panel)
    {
        if (panel.SummonPlaceholders.Count == 0)
            return;

        foreach (var summonPlaceholder in panel.SummonPlaceholders)
            summonPlaceholder.Destroy();

        foreach (var unitPortrait in panel.UnitPortraits)
            unitPortrait.SelectionComponent!.IsSelectionEnabled = true;

        panel.SummonPlaceholders = Array.Empty<SummonPlaceholder>();
    }

    /// <summary>
    /// Проверить, может ли текущий юнит атаковать цель.
    /// </summary>
    private bool CanAttack(BattleUnit targetBattleUnit)
    {
        return _battleProcessor.CanAttack(targetBattleUnit.Unit);
    }

    /// <summary>
    /// Проверить, может ли текущий юнит атаковать цель.
    /// </summary>
    private bool CanAttack(BattleUnit targetBattleUnit, CalculatedUnitAttack mainAttack, CalculatedUnitAttack? secondaryAttack)
    {
        return _battleProcessor.CanAttack(targetBattleUnit.Unit, mainAttack, secondaryAttack);
    }

    /// <summary>
    /// Получить портрет юнита.
    /// </summary>
    private static UnitPortraitObject GetUnitPortrait(BattleUnitPortraitPanelData panelData, BattleUnit battleUnit)
    {
        return panelData.UnitPortraits.First(up => up.Unit.Id == battleUnit.Unit.Id);
    }
}