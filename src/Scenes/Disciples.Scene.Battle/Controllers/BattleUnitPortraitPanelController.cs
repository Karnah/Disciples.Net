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
using Disciples.Scene.Battle.GameObjects;
using Disciples.Scene.Battle.Models;

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

    private readonly BattleUnitPortraitPanelData _leftPanel = new();
    private readonly BattleUnitPortraitPanelData _rightPanel = new();

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
    public void ProcessMousePosition(PointD mousePosition)
    {
        var isMainRightPanel = _context.PlayerSquadPosition == BattleSquadPosition.Attacker;

        // TODO Область для отображении второй панели определяется в LEFT_PANEL_DISPLAY_PLACEHOLDER.
        if (IsDisplayingBothPanels)
        {
            var isNeedHideOppositePanel = isMainRightPanel
                ? mousePosition.X > _leftPanel.PanelImage!.Width
                : mousePosition.X < GameInfo.OriginalWidth - _rightPanel.PanelImage!.Width;
            if (isNeedHideOppositePanel)
            {
                IsDisplayingBothPanels = false;

                RemovePanel(!isMainRightPanel);

                ArrangePortraits(_displayingSquad!.Value, isMainRightPanel);
                ArrangeBorders(isMainRightPanel);
            }
        }
        else
        {
            var isNeedDisplayOppositePanel = isMainRightPanel
                ? mousePosition.X < 10
                : mousePosition.X > GameInfo.OriginalWidth - 10;
            if (isNeedDisplayOppositePanel)
            {
                IsDisplayingBothPanels = true;

                ArrangePortraits(BattleSquadPosition.Attacker, false);
                ArrangeBorders(false);

                // TODO Возможно, его нужно закрывать дополнительно.
                ArrangePortraits(BattleSquadPosition.Defender, true);
                ArrangeBorders(true);
            }
        }
    }

    /// <summary>
    /// Разрешить переключать юнитов на панели.
    /// </summary>
    public void EnablePanelSwitch(BattleSquadPosition displayingSquad)
    {
        if (_displayingSquad != displayingSquad)
            SetDisplayingSquad(displayingSquad);

        ActivateButtons(_reflectPanelButton);
        ArrangeBorders(true);
        ArrangeBorders(false);
    }

    /// <summary>
    /// Запретить переключать юнитов на панели.
    /// </summary>
    public void DisablePanelSwitch(BattleSquadPosition displayingSquad)
    {
        if (_displayingSquad != displayingSquad)
            SetDisplayingSquad(displayingSquad);

        DisableButtons(_reflectPanelButton);
        // TODO. Рамка-анимация текущего юнита всегда должна присутствовать.
        CleanAnimationsOnUnitsPanel(_rightPanel);
        CleanAnimationsOnUnitsPanel(_leftPanel);
    }

    /// <summary>
    /// Завершить битву.
    /// </summary>
    public void CompleteBattle()
    {
        if (_displayingSquad != _context.BattleWinnerSquad!.Value)
            SetDisplayingSquad(_context.BattleWinnerSquad.Value);

        ActivateButtons(_reflectPanelButton);
        CleanAnimationsOnUnitsPanel(_rightPanel);
        CleanAnimationsOnUnitsPanel(_leftPanel);
    }

    /// <summary>
    /// Получить портрет указанного юнита.
    /// </summary>
    public UnitPortraitObject? GetUnitPortrait(BattleUnit battleUnit)
    {
        return _rightPanel.UnitPortraits.FirstOrDefault(up => up.Unit == battleUnit.Unit)
               ?? _leftPanel.UnitPortraits.FirstOrDefault(up => up.Unit == battleUnit.Unit);
    }

    /// <inheritdoc />
    protected override void LoadInternal()
    {
        var gameObjects = _battleGameObjectContainer.GameObjects;

        _reflectPanelButton = gameObjects.GetToggleButton(BattleUnitPanelElementNames.RIGHT_UNIT_PANEL_REFLECT_TOGGLE_BUTTON, ExecuteReflectUnitsPanel);

        _leftPanel.PanelImage = gameObjects.Get<ImageObject>(BattleUnitPanelElementNames.LEFT_UNIT_PANEL_IMAGE, true);
        _leftPanel.PortraitPlaceholders = _interfaceProvider.GetUnitPlaceholders(BattleUnitPanelElementNames.LEFT_UNIT_PANEL_PORTRAIT_PATTERN_PLACEHOLDER);
        _leftPanel.HitPointsPlaceholders = _interfaceProvider.GetUnitPlaceholders(BattleUnitPanelElementNames.LEFT_UNIT_PANEL_HP_PATTERN_PLACEHOLDER);

        _rightPanel.PanelImage = gameObjects.Get<ImageObject>(BattleUnitPanelElementNames.RIGHT_UNIT_PANEL_IMAGE, true);
        _rightPanel.PortraitPlaceholders = _interfaceProvider.GetUnitPlaceholders(BattleUnitPanelElementNames.RIGHT_UNIT_PANEL_PORTRAIT_PATTERN_PLACEHOLDER);
        _rightPanel.HitPointsPlaceholders = _interfaceProvider.GetUnitPlaceholders(BattleUnitPanelElementNames.RIGHT_UNIT_PANEL_HP_PATTERN_PLACEHOLDER);
    }

    /// <inheritdoc />
    protected override void UnloadInternal()
    {
    }

    /// <summary>
    /// Установить отображаемый отряд.
    /// </summary>
    private void SetDisplayingSquad(BattleSquadPosition displayingSquad)
    {
        _displayingSquad = displayingSquad;

        // Если отображаются обе панели, то запоминаем новый отряд, который отображается на основной панели.
        // При этом само отображение не меняем.
        if (IsDisplayingBothPanels)
            return;

        var isRightMainPanel = _context.PlayerSquadPosition == BattleSquadPosition.Attacker;
        ArrangePortraits(displayingSquad, isRightMainPanel);
        ArrangeBorders(isRightMainPanel);

        // Кнопка "зажата", если отображается атакующий отряд.
        _reflectPanelButton.SetState(displayingSquad == BattleSquadPosition.Attacker);
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
    /// Удалить целиком панель.
    /// </summary>
    /// <param name="isRightPanel">
    /// <see langword="true"/>, если нужно инициализировать юнитов на ПРАВОЙ панели.
    /// <see langword="false"/>, если нужно инициализировать юнитов на ЛЕВОЙ панели.
    /// </param>
    private void RemovePanel(bool isRightPanel)
    {
        var panel = isRightPanel
            ? _rightPanel
            : _leftPanel;

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
    /// Разместить портреты юнитов на панели.
    /// </summary>
    /// <param name="squadPosition">Отряд, который должен отображаться.</param>
    /// <param name="isRightPanel">
    /// <see langword="true"/>, если нужно инициализировать юнитов на ПРАВОЙ панели.
    /// <see langword="false"/>, если нужно инициализировать юнитов на ЛЕВОЙ панели.
    /// </param>
    private void ArrangePortraits(BattleSquadPosition squadPosition, bool isRightPanel)
    {
        var panel = isRightPanel
            ? _rightPanel
            : _leftPanel;

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
    /// Разместить анимации рамок, которые показывают какие юниты доступны для атаки / текущий юнит.
    /// </summary>
    /// <param name="isRightPanel">
    /// <see langword="true"/>, если нужно инициализировать юнитов на ПРАВОЙ панели.
    /// <see langword="false"/>, если нужно инициализировать юнитов на ЛЕВОЙ панели.
    /// </param>
    private void ArrangeBorders(bool isRightPanel)
    {
        var panel = isRightPanel
            ? _rightPanel
            : _leftPanel;

        // Панель не отображается, никаких анимаций размещать не нужно.
        if (panel.PanelSquadDirection == null)
            return;

        CleanAnimationsOnUnitsPanel(panel);

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

        panel.BorderAnimations = unitPanelAnimations;
    }

    /// <summary>
    /// Очистить все анимации на панели юнитов.
    /// </summary>
    private static void CleanAnimationsOnUnitsPanel(BattleUnitPortraitPanelData panel)
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
        var attackingSquad = CurrentBattleUnit.IsAttacker
            ? _context.AttackingSquad
            : _context.DefendingSquad;
        var targetSquad = targetBattleUnit.IsAttacker
            ? _context.AttackingSquad
            : _context.DefendingSquad;
        return _battleProcessor.CanAttack(CurrentBattleUnit.Unit, attackingSquad, targetBattleUnit.Unit, targetSquad);
    }

    /// <summary>
    /// Получить портрет юнита.
    /// </summary>
    private static UnitPortraitObject GetUnitPortrait(BattleUnitPortraitPanelData panelData, BattleUnit battleUnit)
    {
        return panelData.UnitPortraits.First(up => up.Unit == battleUnit.Unit);
    }

    /// <summary>
    /// Активировать указанные кнопки.
    /// </summary>
    private static void ActivateButtons(params BaseButtonObject?[] buttons)
    {
        foreach (var button in buttons)
        {
            button?.SetActive();
        }
    }

    /// <summary>
    /// Деактивировать указанные кнопки.
    /// </summary>
    private static void DisableButtons(params BaseButtonObject?[] buttons)
    {
        foreach (var button in buttons)
        {
            button?.SetDisabled();
        }
    }
}