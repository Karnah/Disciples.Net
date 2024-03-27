using Disciples.Scene.Battle.Controllers.BattleActionControllers.Base;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Processors;
using Disciples.Scene.Battle.Providers;

namespace Disciples.Scene.Battle.Controllers.BattleActionControllers;

/// <summary>
/// Действие для снятия всех эффектов с юнитов перед завершением битвы.
/// </summary>
internal class BeforeCompleteBattleActionController : BaseUnitEffectActionController
{
    private readonly BattleContext _context;
    private readonly BattleProcessor _battleProcessor;

    /// <summary>
    /// Создать объект типа <see cref="BeforeCompleteBattleActionController" />.
    /// </summary>
    public BeforeCompleteBattleActionController(
        BattleContext context,
        BattleUnitPortraitPanelController unitPortraitPanelController,
        BattleSoundController soundController,
        IBattleGameObjectContainer battleGameObjectContainer,
        IBattleUnitResourceProvider unitResourceProvider,
        IBattleResourceProvider battleResourceProvider,
        BattleProcessor battleProcessor
        ) : base(context, unitPortraitPanelController, soundController, battleGameObjectContainer, unitResourceProvider, battleResourceProvider, battleProcessor)
    {
        _context = context;
        _battleProcessor = battleProcessor;
    }

    /// <inheritdoc />
    protected override BattleSquadPosition? GetTargetSquadPosition()
    {
        return _context.WinnerSquadPosition!.Value;
    }

    /// <inheritdoc />
    protected override void InitializeInternal()
    {
        ShouldPassTurn = true;

        // Берём только победивший отряд, так как с проигравшего все эффекты уже сняты.
        // Либо при смерти, либо при побеге.
        var battleWinnerSquad = _battleProcessor.WinnerSquad!;
        var unitEffectProcessors = battleWinnerSquad
            .Units
            .Select(_battleProcessor.GetForceCompleteEffectProcessors)
            .SelectMany(ep => ep)
            .ToArray();
        EnqueueEffectProcessors(unitEffectProcessors);
    }
}