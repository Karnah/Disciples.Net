using Disciples.Scene.Battle.Controllers.BattleActionControllers.Base;
using Disciples.Scene.Battle.Controllers.BattleActionControllers.Models;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.GameObjects;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Processors;
using Disciples.Scene.Battle.Providers;

namespace Disciples.Scene.Battle.Controllers.BattleActionControllers;

/// <summary>
/// Контроллер быстрого завершения битвы.
/// </summary>
internal class InstantCompleteBattleActionController : BaseBattleActionController
{
    private readonly BattleContext _context;
    private readonly BattleUnitPortraitPanelController _unitPortraitPanelController;
    private readonly BattleInstantProcessor _battleInstantProcessor;
    private readonly BattleProcessor _battleProcessor;

    /// <summary>
    /// Создать объект типа <see cref="InstantCompleteBattleActionController" />.
    /// </summary>
    public InstantCompleteBattleActionController(
        BattleContext context,
        BattleUnitPortraitPanelController unitPortraitPanelController,
        BattleBottomPanelController bottomPanelController,
        BattleSoundController soundController,
        IBattleGameObjectContainer battleGameObjectContainer,
        IBattleUnitResourceProvider unitResourceProvider,
        BattleInstantProcessor battleInstantProcessor,
        BattleProcessor battleProcessor
        ) : base(context, unitPortraitPanelController, bottomPanelController, soundController, battleGameObjectContainer, unitResourceProvider)
    {
        _context = context;
        _unitPortraitPanelController = unitPortraitPanelController;
        _battleInstantProcessor = battleInstantProcessor;
        _battleProcessor = battleProcessor;
    }

    /// <inheritdoc />
    public override bool ShouldPassTurn { get; protected set; }

    /// <inheritdoc />
    protected override void InitializeInternal()
    {
        _battleInstantProcessor.Process();

        var battleWinnerSquadPosition = _battleProcessor.AttackingSquad == _battleProcessor.WinnerSquad
            ? BattleSquadPosition.Attacker
            : BattleSquadPosition.Defender;
        _context.WinnerSquadPosition = battleWinnerSquadPosition;

        _unitPortraitPanelController.SetDisplayingSquad(battleWinnerSquadPosition);
        _unitPortraitPanelController.DisableBorderAnimations();

        // Создаём защитную копию, так как RemoveBattleUnit/ReplaceUnit меняет коллекцию.
        foreach (var battleUnit in _context.BattleUnits.ToArray())
        {
            // Призванных юнитов просто удаляем. Других действий совершать не нужно.
            if (battleUnit.Unit is SummonedUnit)
            {
                RemoveBattleUnit(battleUnit);
                continue;
            }

            if (battleUnit.Unit.IsDead && battleUnit.UnitState != BattleUnitState.Dead)
                battleUnit.UnitState = BattleUnitState.Dead;
            else if (battleUnit.Unit.IsRetreated)
                battleUnit.UnitState = BattleUnitState.Retreated;

            // Для юнитов отряда победителя выводим опыт.
            if (battleUnit.SquadPosition == battleWinnerSquadPosition &&
                !battleUnit.Unit.IsInactive)
            {
                _unitPortraitPanelController.DisplayMessage(battleUnit,
                    new BattleUnitPortraitEventData(UnitActionType.Experience));

                // Если юнит повысил уровень, то сообщение закроется.
                // TODO Вообще нужно явно доставать юнитов, которые повысили уровень. Но сейчас и так сойдёт.
                if (battleUnit.Unit.BattleExperience == 0)
                {
                    AddActionDelay(new BattleTimerDelay(COMMON_ACTION_DELAY,
                        () => OnMessageCompleted(battleUnit)));
                }
            }

            // Обрабатываем превращение юнита (из-за снятого эффекта или повышения уровня).
            var unitSquad = _battleProcessor.GetUnitSquad(battleUnit.Unit);
            var positionUnit = unitSquad.Units.First(u =>
                u.SquadLinePosition == battleUnit.Unit.SquadLinePosition &&
                u.SquadFlankPosition == battleUnit.Unit.SquadFlankPosition);
            if (battleUnit.Unit != positionUnit)
                ReplaceUnit(battleUnit, positionUnit);
        }
    }

    /// <inheritdoc />
    protected override void OnCompleted()
    {
        base.OnCompleted();

        _context.BattleState = BattleState.WaitingExit;
        _context.BattleActionEvent = BattleActionEvent.BattleCompleted;
    }

    /// <summary>
    /// Обработать необходимо завершения вывода сообщения с опытом.
    /// </summary>
    /// <remarks>
    /// Используется для юнитов, которые повысили свой уровень.
    /// </remarks>
    private void OnMessageCompleted(BattleUnit battleUnit)
    {
        _unitPortraitPanelController.CloseMessage(battleUnit);
    }
}
