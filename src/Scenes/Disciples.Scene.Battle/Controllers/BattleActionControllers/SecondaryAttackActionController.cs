using Disciples.Scene.Battle.Controllers.BattleActionControllers.Base;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.GameObjects;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Processors;
using Disciples.Scene.Battle.Processors.UnitActionProcessors;
using Disciples.Scene.Battle.Providers;

namespace Disciples.Scene.Battle.Controllers.BattleActionControllers;

/// <summary>
/// Контроллер второй атаки.
/// </summary>
internal sealed class SecondaryAttackActionController : BaseAttackActionController
{
    private readonly BattleContext _context;
    private readonly BattleProcessor _battleProcessor;
    private readonly IReadOnlyList<BattleUnit> _targetBattleUnits;

    /// <summary>
    /// Создать объект типа <see cref="SecondaryAttackActionController" />.
    /// </summary>
    public SecondaryAttackActionController(
        BattleContext context,
        BattleUnitPortraitPanelController unitPortraitPanelController,
        BattleSoundController soundController,
        IBattleGameObjectContainer battleGameObjectContainer,
        IBattleUnitResourceProvider unitResourceProvider,
        IBattleResourceProvider battleResourceProvider,
        BattleProcessor battleProcessor,
        IReadOnlyList<BattleUnit> targetBattleUnits,
        bool shouldPassTurn
        ) : base(context, unitPortraitPanelController, soundController, battleGameObjectContainer, unitResourceProvider, battleResourceProvider, battleProcessor)
    {
        _context = context;
        _battleProcessor = battleProcessor;
        _targetBattleUnits = targetBattleUnits;

        ShouldPassTurn = shouldPassTurn;
    }

    /// <inheritdoc />
    protected override BattleSquadPosition? GetTargetSquadPosition()
    {
        return _targetBattleUnits[0].SquadPosition;
    }

    /// <inheritdoc />
    protected override void InitializeInternal()
    {
        foreach (var targetBattleUnit in _targetBattleUnits)
        {
            var attackProcessor = _battleProcessor.ProcessSecondaryAttack(targetBattleUnit.Unit);

            // Промахи для второй атаки никак не отображаются.
            if (attackProcessor is null or MissAttackProcessor)
                continue;

            AddProcessorAction(attackProcessor);
        }
    }
}