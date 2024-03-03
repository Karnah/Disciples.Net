using DryIoc;
using Microsoft.Extensions.Logging;
using Disciples.Scene.Battle.Controllers.UnitActionControllers;
using Disciples.Scene.Battle.GameObjects;
using Disciples.Scene.Battle.Models;

namespace Disciples.Scene.Battle.Controllers;

/// <summary>
/// Фабрика для запуска действий юнитов.
/// </summary>
internal class BattleUnitActionFactory
{
    private readonly BattleContext _battleContext;
    private readonly IResolver _resolver;
    private readonly ILogger<BattleUnitActionFactory> _logger;

    /// <summary>
    /// Создать объект типа <see cref="BattleUnitActionFactory" />.
    /// </summary>
    public BattleUnitActionFactory(BattleContext battleContext, IResolver resolver, ILogger<BattleUnitActionFactory> logger)
    {
        _battleContext = battleContext;
        _resolver = resolver;
        _logger = logger;
    }

    /// <summary>
    /// Обработать атаку текущего юнита на указанного.
    /// </summary>
    public void BeginMainAttack(BattleUnit targetBattleUnit)
    {
        _logger.LogDebug("Begin main attack, target unit: {targetUnit}", targetBattleUnit.Unit.Id);

        var mainAttackController = _resolver.Resolve<MainAttackActionController>(new object[] { targetBattleUnit });
        _battleContext.AddUnitAction(mainAttackController);
    }

    /// <summary>
    /// Обработать результат второй атаки текущего юнита.
    /// </summary>
    public void BeginSecondaryAttack(IReadOnlyList<BattleUnit> targetBattleUnits, bool shouldPassTurn)
    {
        _logger.LogDebug("Begin secondary attack, target units: {targetUnits}", string.Join(',', targetBattleUnits.Select(bu => bu.Unit.Id)));

        var secondaryAttackController = _resolver.Resolve<SecondaryAttackActionController>(new object[] { targetBattleUnits, shouldPassTurn });
        _battleContext.AddUnitAction(secondaryAttackController);
    }

    /// <summary>
    /// Защита юнита.
    /// </summary>
    public void Defend()
    {
        _logger.LogDebug("Defend");

        var defendController = _resolver.Resolve<DefendUnitActionController>();
        _battleContext.AddUnitAction(defendController);
    }

    /// <summary>
    /// Ожидание юнита.
    /// </summary>
    public void Wait()
    {
        _logger.LogDebug("Wait");

        var waitController = _resolver.Resolve<WaitUnitActionController>();
        _battleContext.AddUnitAction(waitController);
    }

    /// <summary>
    /// Убежать с поля боя.
    /// </summary>
    public void Retreat()
    {
        _logger.LogDebug("Retreat");

        var retreatingController = _resolver.Resolve<RetreatingActionController>();
        _battleContext.AddUnitAction(retreatingController);
    }

    /// <summary>
    /// Обработать начало ход юнита.
    /// </summary>
    public void UnitTurn()
    {
        _logger.LogDebug("Begin unit turn {currentUnit}", _battleContext.CurrentBattleUnit.Unit.Id);

        var beginUnitTurnController = _resolver.Resolve<BeginUnitTurnController>();
        _battleContext.AddUnitAction(beginUnitTurnController);
    }
}