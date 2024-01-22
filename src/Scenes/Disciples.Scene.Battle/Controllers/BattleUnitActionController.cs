using Disciples.Scene.Battle.Controllers.UnitActions;
using Disciples.Scene.Battle.GameObjects;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Providers;

namespace Disciples.Scene.Battle.Controllers;

/// <summary>
/// Контроллер для запуска действий юнитов.
/// </summary>
internal class BattleUnitActionController
{
    private readonly BattleContext _battleContext;
    private readonly BattleProcessor _battleProcessor;
    private readonly IBattleGameObjectContainer _battleGameObjectContainer;
    private readonly BattleUnitPortraitPanelController _unitPortraitPanelController;
    private readonly IBattleUnitResourceProvider _unitResourceProvider;
    private readonly BattleSoundController _soundController;

    /// <summary>
    /// Создать объект типа <see cref="BattleUnitActionController" />.
    /// </summary>
    public BattleUnitActionController(
        BattleContext battleContext,
        BattleProcessor battleProcessor,
        IBattleGameObjectContainer battleGameObjectContainer,
        BattleUnitPortraitPanelController unitPortraitPanelController,
        IBattleUnitResourceProvider unitResourceProvider,
        BattleSoundController soundController)
    {
        _battleContext = battleContext;
        _battleProcessor = battleProcessor;
        _battleGameObjectContainer = battleGameObjectContainer;
        _unitPortraitPanelController = unitPortraitPanelController;
        _unitResourceProvider = unitResourceProvider;
        _soundController = soundController;
    }

    /// <summary>
    /// Обработать атаку текущего юнита на указанного.
    /// </summary>
    public void BeginMainAttack(BattleUnit targetBattleUnit)
    {
        var mainAttack = new MainAttackUnitAction(
            _battleContext,
            _battleProcessor,
            _battleGameObjectContainer,
            _unitPortraitPanelController,
            _unitResourceProvider,
            targetBattleUnit,
            _soundController,
            this);
        _battleContext.AddUnitAction(mainAttack);
    }

    /// <summary>
    /// Обработать результат второй атаки текущего юнита.
    /// </summary>
    public void BeginSecondaryAttack(BattleUnit attackerBattleUnit, IReadOnlyList<BattleUnit> targetBattleUnits, bool shouldPassTurn)
    {
        var secondaryAttack = new SecondaryAttackUnitAction(
            _battleContext,
            _battleGameObjectContainer,
            _unitPortraitPanelController,
            _unitResourceProvider,
            _soundController,
            _battleProcessor,
            attackerBattleUnit,
            targetBattleUnits,
            shouldPassTurn);
        _battleContext.AddUnitAction(secondaryAttack);
    }

    /// <summary>
    /// Защита юнита.
    /// </summary>
    public void Defend()
    {
        var defend = new DefendUnitAction(
            _battleContext,
            _battleGameObjectContainer,
            _unitPortraitPanelController,
            _unitResourceProvider,
            _soundController);
        _battleContext.AddUnitAction(defend);
    }

    /// <summary>
    /// Ожидание юнита.
    /// </summary>
    public void Wait()
    {
        var defend = new WaitUnitAction(
            _battleContext,
            _battleGameObjectContainer,
            _unitPortraitPanelController,
            _unitResourceProvider,
            _soundController);
        _battleContext.AddUnitAction(defend);
    }

    /// <summary>
    /// Убежать с поля боя.
    /// </summary>
    public void Retreat()
    {
        var retreat = new RetreatUnitAction(
            _battleContext,
            _battleGameObjectContainer,
            _unitPortraitPanelController,
            _unitResourceProvider,
            _soundController);
        _battleContext.AddUnitAction(retreat);
    }

    /// <summary>
    /// Обработать начало ход юнита.
    /// </summary>
    public void UnitTurn()
    {
        var turn = new TurnUnitAction(
            _battleContext,
            _battleGameObjectContainer,
            _unitPortraitPanelController,
            _unitResourceProvider,
            _soundController,
            _battleProcessor);
        _battleContext.AddUnitAction(turn);
    }
}