using DryIoc;
using Microsoft.Extensions.Logging;
using Disciples.Scene.Battle.Controllers.BattleActionControllers;
using Disciples.Scene.Battle.GameObjects;
using Disciples.Scene.Battle.Models;

namespace Disciples.Scene.Battle.Controllers;

/// <summary>
/// Фабрика для запуска действий юнитов.
/// </summary>
internal class BattleActionFactory
{
    private readonly BattleContext _battleContext;
    private readonly IResolver _resolver;
    private readonly ILogger<BattleActionFactory> _logger;

    /// <summary>
    /// Создать объект типа <see cref="BattleActionFactory" />.
    /// </summary>
    public BattleActionFactory(BattleContext battleContext, IResolver resolver, ILogger<BattleActionFactory> logger)
    {
        _battleContext = battleContext;
        _resolver = resolver;
        _logger = logger;
    }

    /// <summary>
    /// Начать битву.
    /// </summary>
    public void BeginBattle()
    {
        _logger.LogDebug("Begin battle");

        var beginBattleController = _resolver.Resolve<BeginBattleActionController>();
        _battleContext.AddAction(beginBattleController);
    }

    /// <summary>
    /// Обработать атаку текущего юнита на указанную позицию.
    /// </summary>
    public void BeginMainAttack(BattleUnitPosition unitPosition)
    {
        _logger.LogDebug("Begin main attack, target position: {targetUnit}", unitPosition);

        var mainAttackController = _resolver.Resolve<MainAttackActionController>(new object[] { unitPosition });
        _battleContext.AddAction(mainAttackController);
    }

    /// <summary>
    /// Обработать результат второй атаки текущего юнита.
    /// </summary>
    public void BeginSecondaryAttack(IReadOnlyList<BattleUnit> targetBattleUnits, bool shouldPassTurn)
    {
        _logger.LogDebug("Begin secondary attack, target units: {targetUnits}", string.Join(',', targetBattleUnits.Select(bu => bu.Unit.Id)));

        var secondaryAttackController = _resolver.Resolve<SecondaryAttackActionController>(new object[] { targetBattleUnits, shouldPassTurn });
        _battleContext.AddAction(secondaryAttackController);
    }

    /// <summary>
    /// Защита юнита.
    /// </summary>
    public void Defend()
    {
        _logger.LogDebug("Defend");

        var defendController = _resolver.Resolve<DefendUnitActionController>();
        _battleContext.AddAction(defendController);
    }

    /// <summary>
    /// Ожидание юнита.
    /// </summary>
    public void Wait()
    {
        _logger.LogDebug("Wait");

        var waitController = _resolver.Resolve<WaitUnitActionController>();
        _battleContext.AddAction(waitController);
    }

    /// <summary>
    /// Убежать с поля боя.
    /// </summary>
    public void Retreat()
    {
        _logger.LogDebug("Retreat");

        var retreatingController = _resolver.Resolve<RetreatingActionController>();
        _battleContext.AddAction(retreatingController);
    }

    /// <summary>
    /// Обработать начало ход юнита.
    /// </summary>
    public void UnitTurn()
    {
        _logger.LogDebug("Begin unit turn {currentUnit}", _battleContext.CurrentBattleUnit.Unit.Id);

        var beginUnitTurnController = _resolver.Resolve<BeginUnitTurnController>();
        _battleContext.AddAction(beginUnitTurnController);
    }

    /// <summary>
    /// Снять все эффекты с оставшихся в живых юнитов.
    /// </summary>
    public void BeforeCompleteBattle()
    {
        _logger.LogDebug("Pre complete battle, winner squad: {winnerSquad}", _battleContext.WinnerSquadPosition);

        var beforeCompleteBattleActionController = _resolver.Resolve<BeforeCompleteBattleActionController>();
        _battleContext.AddAction(beforeCompleteBattleActionController);
    }

    /// <summary>
    /// Мгновенно завершить битву.
    /// </summary>
    public void InstantCompleteBattle()
    {
        _logger.LogDebug("Instant complete battle");

        var instantCompleteBattleActionController = _resolver.Resolve<InstantCompleteBattleActionController>();
        _battleContext.AddAction(instantCompleteBattleActionController);
    }

    /// <summary>
    /// Завершить битву и распределить опыт.
    /// </summary>
    public void CompleteBattle()
    {
        _logger.LogDebug("Complete battle");

        var completeBattleActionController = _resolver.Resolve<CompleteBattleActionController>();
        _battleContext.AddAction(completeBattleActionController);
    }
}