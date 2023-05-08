using Disciples.Engine.Common.Models;
using Disciples.Engine.Implementation.Base;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.GameObjects;
using Disciples.Scene.Battle.Models;

namespace Disciples.Scene.Battle.Controllers;

/// <inheritdoc cref="IBattleController" />
internal class BattleController : BaseSupportLoading, IBattleController
{
    private readonly IBattleGameObjectContainer _battleGameObjectContainer;
    private readonly BattleProcessor _battleProcessor;
    private readonly BattleContext _context;
    private readonly BattleAiProcessor _battleAiProcessor;
    private readonly BattleUnitActionController _unitActionController;

    /// <summary>
    /// Создать объект типа <see cref="BattleController" />.
    /// </summary>
    public BattleController(
        IBattleGameObjectContainer battleGameObjectContainer,
        BattleProcessor battleProcessor,
        BattleContext context,
        BattleAiProcessor battleAiProcessor,
        BattleUnitActionController unitActionController)
    {
        _battleGameObjectContainer = battleGameObjectContainer;
        _battleProcessor = battleProcessor;
        _context = context;
        _battleAiProcessor = battleAiProcessor;
        _unitActionController = unitActionController;
    }

    /// <summary>
    ///  Юнит, который выполняет свой ход.
    /// </summary>
    private BattleUnit CurrentBattleUnit
    {
        get => _context.CurrentBattleUnit;
        set => _context.CurrentBattleUnit = value;
    }

    /// <summary>
    /// Признак, что битва проходит в автоматическом режиме.
    /// </summary>
    private bool IsAutoBattle => _context.IsAutoBattle;

    /// <inheritdoc />
    public void BeforeSceneUpdate()
    {
        if (IsAutoBattle && _context.BattleState is BattleState.WaitPlayerTurn or BattleState.CompletedUnitAction)
            CheckAndProcessIfAiTurn();
    }

    /// <inheritdoc />
    public void AfterSceneUpdate()
    {
        if (_context.BattleState == BattleState.CompletedUnitAction)
        {
            var battleWinner = _battleProcessor.GetBattleWinnerSquad(_context.AttackingSquad, _context.DefendingSquad);
            if (battleWinner != null)
            {
                // TODO Снять все эффекты.
                var battleWinnerSquad = _context.AttackingSquad == battleWinner
                    ? BattleSquadPosition.Attacker
                    : BattleSquadPosition.Defender;
                _context.SetBattleCompleted(battleWinnerSquad);
            }
            else if (_context.CompletedUnitAction!.ShouldPassTurn)
            {
                NextTurn();
            }
            else
            {
                CheckAndProcessIfAiTurn();
            }
        }
    }

    /// <inheritdoc />
    protected override void LoadInternal()
    {
        ArrangeUnits();
        StartNewRound();
    }

    /// <inheritdoc />
    protected override void UnloadInternal()
    {
    }

    /// <summary>
    /// Расставить юнитов по позициям.
    /// </summary>
    private void ArrangeUnits()
    {
        var units = new List<BattleUnit>();

        foreach (var attackSquadUnit in _context.AttackingSquad.Units)
            units.Add(_battleGameObjectContainer.AddBattleUnit(attackSquadUnit, true));

        foreach (var defendSquadUnit in _context.DefendingSquad.Units)
            units.Add(_battleGameObjectContainer.AddBattleUnit(defendSquadUnit, false));

        _context.BattleUnits = units;
    }

    // Начать новый раунд.
    private void StartNewRound()
    {
        ++_context.Round;
        var turnOrder = _battleProcessor.GetTurnOrder(_context.AttackingSquad, _context.DefendingSquad);
        var nextUnit = _context.UnitTurnQueue.NextRound(turnOrder);
        BeginUnitTurn(nextUnit);
    }

    // Передать ход следующему юниту.
    private void NextTurn()
    {
        var nextUnit = _context.UnitTurnQueue.GetNextUnit();
        if (nextUnit != null)
        {
            BeginUnitTurn(nextUnit);
            return;
        }

        StartNewRound();
    }

    /// <summary>
    /// Обработать ход искусственного интеллекта.
    /// </summary>
    private void CheckAndProcessIfAiTurn()
    {
        // Проверяем, если ход будет делать игрок.
        if (!CurrentBattleUnit.Unit.Player.IsComputer && !IsAutoBattle)
            return;

        var attackingSquad = CurrentBattleUnit.IsAttacker
            ? _context.AttackingSquad
            : _context.DefendingSquad;
        var defendingSquad = CurrentBattleUnit.IsAttacker
            ? _context.DefendingSquad
            : _context.AttackingSquad;
        var command = _battleAiProcessor.GetAiCommand(
            CurrentBattleUnit.Unit,
            attackingSquad,
            defendingSquad);

        switch (command.CommandType)
        {
            case BattleCommandType.Attack:
                _unitActionController.BeginMainAttack(_context.GetBattleUnit(command.Target!));
                break;

            case BattleCommandType.Defend:
                _unitActionController.Defend();
                break;

            case BattleCommandType.Wait:
                _unitActionController.Wait();
                break;

            case BattleCommandType.Retreat:
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    /// Начать ход юнита.
    /// </summary>
    private void BeginUnitTurn(Unit unit)
    {
        CurrentBattleUnit = _context.GetBattleUnit(unit);

        // Запускаем обработку эффектов на юните.
        if (CurrentBattleUnit.Unit.Effects.HasBattleEffects)
            _unitActionController.UnitTurn();

        // Если после этого не появилось нового действия, значит нет эффектов для обработки.
        if (_context.BattleState is BattleState.WaitPlayerTurn or BattleState.CompletedUnitAction)
            CheckAndProcessIfAiTurn();
    }
}