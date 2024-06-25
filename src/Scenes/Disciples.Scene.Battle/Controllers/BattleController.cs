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
    private readonly BattleActionFactory _actionFactory;

    /// <summary>
    /// Создать объект типа <see cref="BattleController" />.
    /// </summary>
    public BattleController(
        IBattleGameObjectContainer battleGameObjectContainer,
        BattleProcessor battleProcessor,
        BattleContext context,
        BattleAiProcessor battleAiProcessor,
        BattleActionFactory actionFactory)
    {
        _battleGameObjectContainer = battleGameObjectContainer;
        _battleProcessor = battleProcessor;
        _context = context;
        _battleAiProcessor = battleAiProcessor;
        _actionFactory = actionFactory;
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
    private bool IsInstantBattleRequested => _context.IsInstantBattleRequested;

    /// <inheritdoc />
    public void BeforeSceneUpdate()
    {
        if (_context.BattleState is BattleState.Idle)
        {
            if (IsInstantBattleRequested)
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
        if (_context.BattleActionEvent != BattleActionEvent.ActionCompleted)
            return;

        if (_battleProcessor.WinnerSquad == null)
        {
            // Если один из отрядов победил, то запускаем процесс завершения битвы.
            // Первый шаг - снять все эффекты.
            // Вторым шагом будет последний ход для всех юнитов-целителей, если есть повреждённые бойцы.
            // Третий шаг - распределить опыт.
            var battleWinner = _battleProcessor.CheckAndSetBattleWinnerSquad();
            if (battleWinner != null)
            {
                var battleWinnerSquad = _battleProcessor.AttackingSquad == battleWinner
                    ? BattleSquadPosition.Attacker
                    : BattleSquadPosition.Defender;
                _context.WinnerSquadPosition = battleWinnerSquad;

                _actionFactory.BeforeCompleteBattle();

                // Если нет действий снятия эффектов, то всегда пропускаем ход текущего юнита, так как он точно не целитель/воскрешатель.
                if (_context.BattleState == BattleState.Idle)
                    NextTurn();

                return;
            }
        }

        if (_context.IsInstantBattleRequested)
        {
            ProcessInstantBattle();
        }
        else if (_context.CompletedAction!.ShouldPassTurn)
        {
            NextTurn();
        }
        else
        {
            CheckAndProcessIfAiTurn();
        }
    }

    /// <inheritdoc />
    protected override void LoadInternal()
    {
        _actionFactory.BeginBattle();

        ArrangeUnits();
        NextTurn();
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
        var battleUnits = new List<BattleUnit>();

        foreach (var attackSquadUnit in _battleProcessor.AttackingSquad.Units)
            battleUnits.Add(_battleGameObjectContainer.AddBattleUnit(attackSquadUnit, BattleSquadPosition.Attacker));

        foreach (var defendSquadUnit in _battleProcessor.DefendingSquad.Units)
            battleUnits.Add(_battleGameObjectContainer.AddBattleUnit(defendSquadUnit, BattleSquadPosition.Defender));

        _context.BattleUnits = battleUnits;
    }

    /// <summary>
    /// Передать ход следующему юниту.
    /// </summary>
    private void NextTurn()
    {
        var roundNumber = _battleProcessor.RoundNumber;

        // Если следующего юнита нет, то битва закончена.
        var nextUnit = _battleProcessor.GetNextUnit();
        if (nextUnit == null)
        {
            _actionFactory.CompleteBattle();
            return;
        }

        // BUG Сейчас есть проблемы с эффектами, которые привязаны в ходу юнита, который наложил его.
        // Например, усиление урона или даровать защиту.
        // Если юнит умирает, то игра не сбрасывает такие эффекты.
        // По-хорошему, таких юнитов нужно сохранять в очереди и вызывать для них обработку.
        // Но пока просто сделал, что эффекты мёртвых юнитов сбрасываются между раундами.
        if (roundNumber != _battleProcessor.RoundNumber)
        {
            foreach (var battleUnit in _context.BattleUnits)
            {
                foreach (var unitBattleEffect in battleUnit.Unit.Effects.GetBattleEffects())
                {
                    if (!unitBattleEffect.Duration.IsInfinitive && unitBattleEffect.DurationControlUnit.IsDead)
                        battleUnit.Unit.Effects.Remove(unitBattleEffect);
                }
            }
        }

        CurrentBattleUnit = _context.GetBattleUnit(nextUnit);

        // Запускаем обработку эффектов юнита.
        _actionFactory.UnitTurn();

        // Если после этого не появилось нового действия, значит нет эффектов для обработки.
        if (_context.BattleState is BattleState.Idle)
            CheckAndProcessIfAiTurn();
    }

    /// <summary>
    /// Обработать ход искусственного интеллекта.
    /// </summary>
    private void CheckAndProcessIfAiTurn()
    {
        // Проверяем, если ход будет делать игрок.
        if (!CurrentBattleUnit.Unit.Player.IsComputer && !IsAutoBattle)
            return;

        var command = _battleAiProcessor.GetAiCommand();
        switch (command.CommandType)
        {
            case BattleCommandType.Attack:
                var squadPosition = command.TargetSquad == _battleProcessor.AttackingSquad
                    ? BattleSquadPosition.Attacker
                    : BattleSquadPosition.Defender;
                _actionFactory.BeginMainAttack(squadPosition, command.TargetPosition!.Value);
                break;

            case BattleCommandType.Defend:
                _actionFactory.Defend();
                break;

            case BattleCommandType.Wait:
                _actionFactory.Wait();
                break;

            case BattleCommandType.Retreat:
                _actionFactory.Retreat();
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    /// Обработать быстрое завершение битвы.
    /// </summary>
    private void ProcessInstantBattle()
    {
        _actionFactory.InstantCompleteBattle();
    }
}