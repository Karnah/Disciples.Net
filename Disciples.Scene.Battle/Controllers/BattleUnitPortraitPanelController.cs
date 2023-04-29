using Disciples.Engine.Base;
using Disciples.Engine;
using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Enums.Units;
using Disciples.Engine.Common.GameObjects;
using Disciples.Engine.Common.Models;
using Disciples.Engine.Common.Providers;
using Disciples.Engine.Extensions;
using Disciples.Engine.Implementation.Base;
using Disciples.Engine.Models;
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
    /// Слой для расположения интерфейса.
    /// </summary>
    private const int INTERFACE_LAYER = 1000;

    private readonly IBattleGameObjectContainer _battleGameObjectContainer;
    private readonly IBattleInterfaceProvider _interfaceProvider;
    private readonly BattleContext _context;
    private readonly BattleProcessor _battleProcessor;
    private readonly ISceneObjectContainer _sceneObjectContainer;

    // TODO Такая обработка должна быть во многих местах кода. Вынести её в одно место.
    /// <summary>
    /// Если начальная позиция <see cref="BattleSquadPosition.Attacker" />, значит место на панели справа.
    /// Иначе панель будет располагаться слева.
    /// </summary>
    private readonly BattleSquadPosition _initialSquadPosition = BattleSquadPosition.Attacker;

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
        BattleProcessor battleProcessor,
        ISceneObjectContainer sceneObjectContainer)
    {
        _battleGameObjectContainer = battleGameObjectContainer;
        _interfaceProvider = interfaceProvider;
        _context = context;
        _battleProcessor = battleProcessor;
        _sceneObjectContainer = sceneObjectContainer;
    }

    /// <inheritdoc />
    public override bool IsSharedBetweenScenes => false;

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
    public void ProcessMousePosition(Point mousePosition)
    {
        var isMainRightPanel = _initialSquadPosition == BattleSquadPosition.Attacker;

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
        // TODO. Если юнит защищается, то рамка-анимация остаётся на нём. Возможно, что целителем такая же тема.
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
        var buttonPosition = _initialSquadPosition == BattleSquadPosition.Attacker
            ? new Point(633, 402)
            : new Point(142, 402);
        _reflectPanelButton = _battleGameObjectContainer.AddToggleButton(_interfaceProvider.ToggleRightButton,
            ReflectUnitsPanel, buttonPosition.X, buttonPosition.Y, INTERFACE_LAYER + 2, KeyboardButton.Tab);
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

        var isRightMainPanel = _initialSquadPosition == BattleSquadPosition.Attacker;
        ArrangePortraits(displayingSquad, isRightMainPanel);
        ArrangeBorders(isRightMainPanel);

        // Кнопка "зажата", если отображается атакующий отряд.
        _reflectPanelButton.SetState(displayingSquad == BattleSquadPosition.Attacker);
    }

    /// <summary>
    /// Поменять отряд, который отображается на основной панели с юнитам.
    /// </summary>
    private void ReflectUnitsPanel()
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

        _sceneObjectContainer.RemoveSceneObject(panel.PanelImage);

        foreach (var unitPortrait in panel.UnitPortraits)
        {
            unitPortrait.Destroy();
        }

        foreach (var borderAnimation in panel.BorderAnimations)
        {
            borderAnimation.Destroy();
        }

        panel.PanelImage = null;
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

        // Добавляем изображение панели, если её еще нет.
        if (panel.PanelImage == null)
        {
            var bitmap = isRightPanel
                ? _interfaceProvider.RightPanel
                : _interfaceProvider.LeftPanel;
            var xPosition = isRightPanel
                ? GameInfo.OriginalWidth - bitmap.Width
                : 0;
            var panelImage = _sceneObjectContainer.AddImage(bitmap, xPosition, 30, INTERFACE_LAYER);

            panel.PanelImage = panelImage;
        }

        panel.PanelSquadDirection = squadPosition;

        // Удаляем старые портреты.
        foreach (var portrait in panel.UnitPortraits)
            portrait.Destroy();

        var battleUnits = BattleUnits
            .Where(u => u.SquadPosition == squadPosition)
            .ToList();

        var isAttackingSquad = squadPosition == BattleSquadPosition.Attacker;
        var portraits = new List<UnitPortraitObject>();
        foreach (var battleUnit in battleUnits)
        {
            var position = GetUnitPortraitPanelPosition(isRightPanel, battleUnit.Unit, squadPosition);
            var portrait = _battleGameObjectContainer.AddUnitPortrait(battleUnit.Unit,
                !isAttackingSquad,
                position.X,
                position.Y);
            portraits.Add(portrait);
        }

        panel.BattleUnits = battleUnits;
        panel.UnitPortraits = portraits;
    }

    /// <summary>
    /// Получить координаты на сцене для рамки портрета юнита.
    /// </summary>
    private static Point GetUnitPortraitPanelPosition(bool isRightPanel, Unit unit, BattleSquadPosition unitSquadPosition)
    {
        var unitLineOffset = GetUnitLineOffset(unitSquadPosition, unit.SquadLinePosition);
        return isRightPanel
            ? GetRightUnitPortraitPanelPosition(unitLineOffset, unit)
            : GetLeftUnitPortraitPanelPosition(unitLineOffset, unit);
    }

    /// <summary>
    /// Получить координаты на сцене для рамки портрета юнита на правой панели.
    /// </summary>
    private static Point GetRightUnitPortraitPanelPosition(int unitLineOffset, Unit unit)
    {
        return new Point(unit.UnitType.IsSmall
                ? 644 + 79 * unitLineOffset
                : 644,
            85 + 106 * (2 - (int)unit.SquadFlankPosition));
    }

    /// <summary>
    /// Получить координаты на сцене для портрета юнита на левой панели.
    /// </summary>
    private static Point GetLeftUnitPortraitPanelPosition(int unitLineOffset, Unit unit)
    {
        return new Point(unit.UnitType.IsSmall
                ? 8 + 79 * unitLineOffset
                : 8,
            85 + 106 * (2 - (int)unit.SquadFlankPosition));
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
            var position = GetUnitBorderPanelPosition(isRightPanel, currentUnit, CurrentBattleUnit.SquadPosition);

            unitPanelAnimations.Add(
                _battleGameObjectContainer.AddAnimation(
                    _interfaceProvider.GetUnitSelectionBorder(currentUnit.UnitType.IsSmall),
                    position.X,
                    position.Y,
                    INTERFACE_LAYER + 3));
        }

        // Если юнит бьёт по площади и цель юнита - отображаемый отряд, то добавляем одну большую рамку.
        if (currentUnit.UnitType.MainAttack.Reach == UnitAttackReach.All &&
            panel.BattleUnits.Any(CanAttack))
        {
            var position = GetBigBorderPanelPosition(isRightPanel);

            unitPanelAnimations.Add(
                _battleGameObjectContainer.AddAnimation(
                    currentUnit.HasAllyAbility()
                        ? _interfaceProvider.GetFieldHealBorder()
                        : _interfaceProvider.GetFieldAttackBorder(),
                    position.X,
                    position.Y,
                    INTERFACE_LAYER + 3));
        }
        // Иначе добавляем рамку только тем юнитам, которых можно атаковать.
        else
        {
            foreach (var targetBattleUnit in panel.BattleUnits)
            {
                if (!CanAttack(targetBattleUnit))
                    continue;

                var position = GetUnitBorderPanelPosition(isRightPanel, targetBattleUnit.Unit, targetBattleUnit.SquadPosition);

                unitPanelAnimations.Add(
                    _battleGameObjectContainer.AddAnimation(
                        currentUnit.HasAllyAbility()
                            ? _interfaceProvider.GetUnitHealBorder(targetBattleUnit.Unit.UnitType.IsSmall)
                            : _interfaceProvider.GetUnitAttackBorder(targetBattleUnit.Unit.UnitType.IsSmall),
                        position.X,
                        position.Y,
                        INTERFACE_LAYER + 3));
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
    /// Получить координаты на сцене для рамки портрета юнита.
    /// </summary>
    private static Point GetUnitBorderPanelPosition(bool isRightPanel, Unit unit, BattleSquadPosition unitSquadPosition)
    {
        var unitLineOffset = GetUnitLineOffset(unitSquadPosition, unit.SquadLinePosition);
        return isRightPanel
            ? GetRightUnitBorderPanelPosition(unitLineOffset, unit)
            : GetLeftUnitBorderPanelPosition(unitLineOffset, unit);
    }

    /// <summary>
    /// Получить координаты для большой рамки (для юнитов, которые атакуют по области).
    /// </summary>
    private static Point GetBigBorderPanelPosition(bool isRightPanel)
    {
        return isRightPanel
            ? new Point(642, 83)
            : new Point(6, 83);
    }

    /// <summary>
    /// Получить координаты на сцене для рамки портрета юнита на правой панели.
    /// </summary>
    private static Point GetRightUnitBorderPanelPosition(int unitLineOffset, Unit unit)
    {
        return new Point(
            unit.UnitType.IsSmall
                ? 642 + unitLineOffset * 79
                : 642,
            83 + (2 - (int)unit.SquadFlankPosition) * 106
        );
    }

    /// <summary>
    /// Получить координаты на сцене для портрета юнита на левой панели.
    /// </summary>
    private static Point GetLeftUnitBorderPanelPosition(int unitLineOffset, Unit unit)
    {
        return new Point(
            unit.UnitType.IsSmall
                ? 6 + unitLineOffset * 79
                : 6,
            83 + (2 - (int)unit.SquadFlankPosition) * 106
        );
    }

    /// <summary>
    /// Получить линию, на которой располагается юнит.
    /// </summary>
    /// <remarks>
    /// Для защищающегося отряда: слева первая линия, справа вторая.
    /// Для атакующего отряда: слева вторая линия, справа первая.
    /// </remarks>
    private static int GetUnitLineOffset(BattleSquadPosition unitSquadPosition, UnitSquadLinePosition linePosition)
    {
        return unitSquadPosition == BattleSquadPosition.Attacker
            ? (int)linePosition
            : ((int)linePosition + 1) % 2;
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
}