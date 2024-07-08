using Disciples.Engine.Common.Enums;
using Disciples.Engine.Common.Models;
using Disciples.Scene.Battle.Enums;
using Disciples.Scene.Battle.Models;

namespace Disciples.Scene.Battle.Processors;

/// <summary>
/// Обработчик мгновенной битвы.
/// </summary>
internal class BattleInstantProcessor
{
    private readonly BattleProcessor _battleProcessor;
    private readonly BattleAiProcessor _battleAiProcessor;

    /// <summary>
    /// Создать объект типа <see cref="BattleInstantProcessor" />.
    /// </summary>
    public BattleInstantProcessor(BattleProcessor battleProcessor, BattleAiProcessor battleAiProcessor)
    {
        _battleProcessor = battleProcessor;
        _battleAiProcessor = battleAiProcessor;
    }

    /// <summary>
    /// Быстрое завершение битвы
    /// </summary>
    /// <returns>Победивший отряд в битве.</returns>
    public void Process()
    {
        var instantBattleBeginRoundNumber = _battleProcessor.RoundNumber;
        var currentUnit = _battleProcessor.CurrentUnit;

        while (currentUnit != null)
        {
            // Обрабатываем ход юнита.
            var isCurrentUnitAttacker = _battleProcessor.CurrentUnit.Player == _battleProcessor.AttackingSquad.Player;
            var command = GetCompleteBattleAiCommand(isCurrentUnitAttacker, _battleProcessor.RoundNumber, instantBattleBeginRoundNumber) ??
                          _battleAiProcessor.GetAiCommand();
            ProcessAiCommand(command);
            CheckAndProcessIfBattleHasWinner();

            // Обрабатываем вторую атакую юнита, если он бьёт дважды.
            if (currentUnit.UnitType.IsAttackTwice && command.CommandType == BattleCommandType.Attack)
            {
                var secondAttackCommand = _battleAiProcessor.GetAiCommand();
                ProcessAiCommand(secondAttackCommand);
                CheckAndProcessIfBattleHasWinner();
            }

            currentUnit = GetNextUnit();
        }

        var completeBattleProcessors = _battleProcessor.CompleteBattle();
        foreach (var completeBattleProcessor in completeBattleProcessors)
        {
            completeBattleProcessor.ProcessBeginAction();
            completeBattleProcessor.ProcessCompletedAction();
        }
    }

    /// <summary>
    /// Получить команду, которая должна привести к быстрому завершению битвы.
    /// Она используется в том случае, если победителя определить невозможно, поэтому атакующий отряд должен начать отступать.
    /// </summary>
    private static BattleAiCommand? GetCompleteBattleAiCommand(bool isCurrentUnitAttacker, int roundNumber, int instantBattleBeginRoundNumber)
    {
        const int retreatRoundDiff = 10;
        const int noAttackRoundDiff = 20;

        // Прошло меньше 10 раундов, пока отряды сражаются в прежнем режиме.
        if (roundNumber < instantBattleBeginRoundNumber + retreatRoundDiff)
            return null;

        // Прошло 10 или более раундов, атакующие юниты должны отсутпать.
        if (isCurrentUnitAttacker)
            return new BattleAiCommand(BattleCommandType.Retreat);

        // Прошло менее 20 раундов, защищающиеся юнита атакуют отступающих.
        if (roundNumber < instantBattleBeginRoundNumber + noAttackRoundDiff)
            return null;

        // Прошло 20 или более ходов.
        // Возможно в защищающемся отряде слишком много парализаторов.
        // Уводим их в защиту, чтобы атакующие юниты смогли отступить.
        return new BattleAiCommand(BattleCommandType.Defend);
    }

    /// <summary>
    /// Обработать команду компьютера.
    /// </summary>
    private void ProcessAiCommand(BattleAiCommand command)
    {
        switch (command.CommandType)
        {
            case BattleCommandType.Attack:
                ProcessAttack(command.TargetSquad!, command.TargetPosition!.Value);
                break;

            case BattleCommandType.Defend:
                var defendProcessor = _battleProcessor.ProcessDefend();
                defendProcessor.ProcessBeginAction();
                defendProcessor.ProcessCompletedAction();
                break;

            case BattleCommandType.Wait:
                var waitProcessor = _battleProcessor.ProcessWait();
                waitProcessor.ProcessBeginAction();
                waitProcessor.ProcessCompletedAction();
                break;

            case BattleCommandType.Retreat:
                var retreatProcessor = _battleProcessor.ProcessRetreat();
                retreatProcessor.ProcessBeginAction();
                retreatProcessor.ProcessCompletedAction();
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    /// Обработать атаку.
    /// </summary>
    private void ProcessAttack(Squad targetSquad, UnitSquadPosition targetPosition)
    {
        // Обрабатываем основную атаку.
        var attackResult = _battleProcessor.ProcessMainAttack(targetSquad, targetPosition);
        foreach (var attackProcessor in attackResult.AttackProcessors)
        {
            attackProcessor.ProcessBeginAction();
            attackProcessor.ProcessCompletedAction();
        }

        ProcessDeaths(targetSquad);

        // Обрабатываем вторую атаку.
        var secondAttackProcessors = attackResult
            .SecondaryAttackUnits
            .Select(_battleProcessor.ProcessSecondaryAttack)
            .Where(p => p != null)
            .Select(p => p!)
            .ToArray();
        foreach (var attackProcessor in secondAttackProcessors)
        {
            attackProcessor.ProcessBeginAction();
            attackProcessor.ProcessCompletedAction();
        }

        ProcessDeaths(targetSquad);
    }

    /// <summary>
    /// Получить юнита, который будет ходить следующим.
    /// </summary>
    private Unit? GetNextUnit()
    {
        while (true)
        {
            var currentUnit = _battleProcessor.GetNextUnit();
            if (currentUnit == null)
                return null;

            // Этот флаг запоминаем до обработки эффектов, так как после они могут быть сняты.
            // Если паралич снимается на ходу юнита, то он всё равно не совершает действия.
            var isCurrentUnitDisabled = currentUnit.Effects.IsDisabled;

            ProcessUnitTurn();
            CheckAndProcessIfBattleHasWinner();

            if (isCurrentUnitDisabled || currentUnit.IsDead)
                continue;

            return currentUnit;
        }
    }

    /// <summary>
    /// Обработать наступление хода юнита.
    /// </summary>
    private void ProcessUnitTurn()
    {
        var effectProcessors = _battleProcessor.GetCurrentUnitEffectProcessors();
        foreach (var effectProcessor in effectProcessors)
        {
            effectProcessor.ProcessBeginAction();
            effectProcessor.ProcessCompletedAction();

            var targetUnit = effectProcessor.TargetUnit;
            if (targetUnit is { HitPoints: 0, IsDead: false })
            {
                ProcessUnitDeath(targetUnit);
                return;
            }
        }
    }

    /// <summary>
    /// Проверить и обработать ситуацию, когда есть победитель битвы.
    /// </summary>
    private void CheckAndProcessIfBattleHasWinner()
    {
        // Победитель уже был обработан ранее.
        if (_battleProcessor.WinnerSquad != null)
            return;

        var winnerSquad = _battleProcessor.CheckAndSetBattleWinnerSquad();
        if (winnerSquad == null)
            return;

        // Сбрасываем все эффекты с отряда-победителя.
        // Отряд проигравшего проверять не нужно. Там либо все погибли, либо сбежали.
        // Защитная копия нужна, так как при сбросе эффектов может происходить удаление призванных и трансформация юнитов.
        foreach (var unit in winnerSquad.Units.ToArray())
        {
            var effectProcessors = _battleProcessor.GetForceCompleteEffectProcessors(unit);
            foreach (var effectProcessor in effectProcessors)
            {
                effectProcessor.ProcessBeginAction();
                effectProcessor.ProcessCompletedAction();
            }
        }
    }

    /// <summary>
    /// Обработать смерти в отряде, если они есть.
    /// </summary>
    private void ProcessDeaths(Squad squad)
    {
        // Создаём защитную копию, так как обработка смерти может менять состав юнитов в отряде.
        // Например, это происходит когда юнит был превращён.
        var deadUnits = squad
            .Units
            .Where(unit => unit.HitPoints == 0 && !unit.IsDead)
            .ToArray();
        foreach (var deadUnit in deadUnits)
            ProcessUnitDeath(deadUnit);
    }

    /// <summary>
    /// Обработать смерть юнита.
    /// </summary>
    private void ProcessUnitDeath(Unit targetUnit)
    {
        var deathProcessor = _battleProcessor.ProcessDeath(targetUnit);
        deathProcessor.ProcessBeginAction();
        deathProcessor.ProcessCompletedAction();
    }
}