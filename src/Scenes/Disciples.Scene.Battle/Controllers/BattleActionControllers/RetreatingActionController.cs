using Disciples.Scene.Battle.Controllers.BattleActionControllers.Base;
using Disciples.Scene.Battle.Controllers.BattleActionControllers.Models;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Processors;

namespace Disciples.Scene.Battle.Controllers.BattleActionControllers;

/// <summary>
/// Контроллер отступления юнита.
/// </summary>
internal class RetreatingActionController : BaseBattleActionController
{
    private readonly BattleProcessor _battleProcessor;

    /// <summary>
    /// Создать объект типа <see cref="RetreatingActionController" />.
    /// </summary>
    public RetreatingActionController(
        BattleContext context,
        BattleUnitPortraitPanelController unitPortraitPanelController,
        BattleSoundController soundController,
        IBattleGameObjectContainer battleGameObjectContainer,
        BattleProcessor battleProcessor
        ) : base(context, unitPortraitPanelController, soundController, battleGameObjectContainer)
    {
        _battleProcessor = battleProcessor;
    }

    /// <inheritdoc />
    public override bool ShouldPassTurn { get; protected set; } = true;

    /// <inheritdoc />
    protected override void InitializeInternal()
    {
        var unitRetreatingProcessor = _battleProcessor.ProcessRetreat();
        unitRetreatingProcessor.ProcessBeginAction();
        unitRetreatingProcessor.ProcessCompletedAction();

        // Добавляем небольшую задержку, чтобы действие не закончилось сразу.
        // Это позволит обработать ShouldPassTurn для контроллера битвы.
        AddActionDelay(new BattleTimerDelay(1));
    }
}