using Disciples.Engine.Common.Models;
using Disciples.Engine.Implementation.Base;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.GameObjects;
using Disciples.Scene.Battle.Models;
using Disciples.Scene.Battle.Processors;

namespace Disciples.Scene.Battle.Controllers;

/// <inheritdoc cref="IBattleController" />
internal class BattleController : BaseSupportLoading, IBattleController
{
    private readonly IBattleGameObjectContainer _battleGameObjectContainer;
    private readonly BattleProcessor _battleProcessor;
    private readonly BattleContext _context;
    private readonly BattleAiProcessor _battleAiProcessor;
    private readonly BattleInstantProcessor _battleInstantProcessor;
    private readonly BattleUnitActionFactory _unitActionFactory;

    /// <summary>
    /// Создать объект типа <see cref="BattleController" />.
    /// </summary>
    public BattleController(
        IBattleGameObjectContainer battleGameObjectContainer,
        BattleProcessor battleProcessor,
        BattleContext context,
        BattleAiProcessor battleAiProcessor,
        BattleInstantProcessor battleInstantProcessor,
        BattleUnitActionFactory unitActionFactory)
    {
        _battleGameObjectContainer = battleGameObjectContainer;
        _battleProcessor = battleProcessor;
        _context = context;
        _battleAiProcessor = battleAiProcessor;
        _battleInstantProcessor = battleInstantProcessor;
        _unitActionFactory = unitActionFactory;
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

    /// <summary>
    /// Признак, что битву необходимо завершить в автоматическом режиме.
    /// </summary>
    private bool IsInstantBattle => _context.IsInstantBattle;

    /// <inheritdoc />
    public void BeforeSceneUpdate()
    {
        if (_context.BattleState is BattleState.WaitPlayerTurn or BattleState.CompletedUnitAction)
        {
            if (IsInstantBattle)
            {
                ProcessInstantBattle();
            }
            else if (IsAutoBattle)
            {
                CheckAndProcessIfAiTurn();
            }
        }
    }

    /// <inheritdoc />
    public void AfterSceneUpdate()
    {
        if (_context.BattleState == BattleState.CompletedUnitAction)
        {
            var battleWinner = _battleProcessor.GetBattleWinnerSquad(_context.AttackingSquad, _context.DefendingSquad);
            if (battleWinner != null)
            {
                CompletedBattle(battleWinner);
            }
            else if (_context.IsInstantBattle)
            {
                ProcessInstantBattle();
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
            units.Add(_battleGameObjectContainer.AddBattleUnit(attackSquadUnit, BattleSquadPosition.Attacker));

        foreach (var defendSquadUnit in _context.DefendingSquad.Units)
            units.Add(_battleGameObjectContainer.AddBattleUnit(defendSquadUnit, BattleSquadPosition.Defender));

        _context.BattleUnits = units;
    }

    // Начать новый раунд.
    private void StartNewRound()
    {
        // BUG Сейчас есть проблемы с эффектами, которые привязаны в ходу юнита, который наложил его.
        // Например, усиление урона или даровать защиту.
        // Если юнит умирает, то игра не сбрасывает такие эффекты.
        // По-хорошему, таких юнитов нужно сохранять в очереди и вызывать для них обработку.
        // Но пока просто сделал, что эффекты мёртвых юнитов сбрасываются между раундами.
        foreach (var battleUnit in _context.BattleUnits)
        {
            foreach (var unitBattleEffect in battleUnit.Unit.Effects.GetBattleEffects())
            {
                if (!unitBattleEffect.Duration.IsInfinitive && unitBattleEffect.DurationControlUnit.IsDead)
                    battleUnit.Unit.Effects.Remove(unitBattleEffect);
            }
        }

        ++_context.RoundNumber;
        var turnOrder = _battleProcessor.GetTurnOrder(_context.AttackingSquad, _context.DefendingSquad, _context.RoundNumber);
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
            defendingSquad,
            _context.UnitTurnQueue,
            _context.RoundNumber);

        switch (command.CommandType)
        {
            case BattleCommandType.Attack:
                _unitActionFactory.BeginMainAttack(_context.GetBattleUnit(command.Target!));
                break;

            case BattleCommandType.Defend:
                _unitActionFactory.Defend();
                break;

            case BattleCommandType.Wait:
                _unitActionFactory.Wait();
                break;

            case BattleCommandType.Retreat:
                _unitActionFactory.Retreat();
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

        // Запускаем обработку эффектов юнита.
        _unitActionFactory.UnitTurn();

        // Если после этого не появилось нового действия, значит нет эффектов для обработки.
        if (_context.BattleState is BattleState.WaitPlayerTurn or BattleState.CompletedUnitAction)
            CheckAndProcessIfAiTurn();
    }

    /// <summary>
    /// Обработать быстрое завершение битвы.
    /// </summary>
    private void ProcessInstantBattle()
    {
        var battleWinner = _battleInstantProcessor.Process(CurrentBattleUnit.Unit, _context.AttackingSquad, _context.DefendingSquad, _context.UnitTurnQueue, _context.RoundNumber);

        foreach (var battleUnit in _context.BattleUnits)
        {
            if (battleUnit.Unit.IsDead && battleUnit.UnitState != BattleUnitState.Dead)
                battleUnit.UnitState = BattleUnitState.Dead;
            else if (battleUnit.Unit.IsRetreated)
                battleUnit.UnitState = BattleUnitState.Retreated;
        }

        CompletedBattle(battleWinner);
    }

    /// <summary>
    /// Обработать завершение битвы.
    /// </summary>
    private void CompletedBattle(Squad battleWinner)
    {
        // TODO Снять все эффекты.
        var battleWinnerSquad = _context.AttackingSquad == battleWinner
            ? BattleSquadPosition.Attacker
            : BattleSquadPosition.Defender;
        _context.SetBattleCompleted(battleWinnerSquad);
    }
}